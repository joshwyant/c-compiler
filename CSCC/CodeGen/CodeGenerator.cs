using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;
using CSCC.CodeGen.Syntax.Operands;
using CSCC.Lexing;
using CSCC.TAC.Syntax;
using CSCC.TAC.Syntax.Instructions;

using static CSCC.CodeGen.Syntax.AssemblyRegister;

namespace CSCC.CodeGen;

class CodeGenerator(ProgramTACNode programTACNode)
{
    ProgramTACNode programTACNode = programTACNode;
    ProgramAsmNode? programNode;

    public ProgramAsmNode Generate()
    {
        if (programNode != null) return programNode;

        var program = Pass1_GenerateProgram();
        var stackOffset = Pass2_ReplacePseudoInstructions(program);
        Pass3_Fixup(program, -stackOffset);

        programNode = program;

        return programNode;
    }

    ProgramAsmNode Pass1_GenerateProgram()
    {
        var astFunc = programTACNode.Function;
        return new ProgramAsmNode(new FunctionDefinitionAsmNode(astFunc.Name, Instructions(astFunc.Instructions)));
    }
    int Pass2_ReplacePseudoInstructions(ProgramAsmNode program)
    {
        Dictionary<string, OperandAsmNode> operandMap = [];
        var offset = 0;

        foreach (var instruction in program.Function.Instructions)
        {
            for (var i = 0; i < instruction.Operands.Length; i++)
            {
                if (instruction.Operands[i] is not PseudoAsmNode ps) continue;

                operandMap.TryGetValue(ps.Identifier, out var op);

                if (op == null)
                {
                    // Create new stack space
                    offset -= 4;
                    op = new StackAsmNode(offset);
                    operandMap.Add(ps.Identifier, op);
                }

                // Replace the pseudo operand with the stack one we found/created
                instruction.Operands[i] = op;
            }
        }

        return offset;
    }
    void Pass3_Fixup(ProgramAsmNode program, int stackSpace)
    {
        var instructions = program.Function.Instructions;

        // Insert stack allocation instruction
        instructions.Insert(0, new AllocateStackAsmNode(stackSpace));

        // Fix invalid mov instructions
        for (var i = 1; i < instructions.Count; i++)
        {
            var instruction = instructions[i];
            if (instruction is not MovAsmNode mov) continue;

            // Fix the ones with both operands on the stack
            if (mov.Source is not StackAsmNode || mov.Destination is not StackAsmNode) continue;

            // Use R10 as an intermediate register
            var tmp = new RegisterAsmNode(R10);
            var dest = mov.Destination;

            // Add another mov
            mov.Destination = tmp;
            var newMov = new MovAsmNode(tmp, dest);
            instructions.Insert(++i, newMov);
        }
    }
    List<InstructionAsmNode> Instructions(InstructionTACNode[] tac)
    {
        var instructions = new List<InstructionAsmNode>();

        foreach (var instruction in tac)
        {
            switch (instruction)
            {
                case ReturnInstructionTACNode ret:
                    var retVal = Operand(ret.Value);
                    instructions.Add(new MovAsmNode(retVal, new RegisterAsmNode(AX)));
                    instructions.Add(new RetAsmNode());
                    break;
                case UnaryInstructionTACNode unary:
                    var srcOperand = Operand(unary.Source);
                    var destOperand = Operand(unary.Destination);
                    var unaryOperator = unary.Operator switch
                    {
                        TokenType.Hyphen => AssemblyOperator.Neg,
                        TokenType.Tilde => AssemblyOperator.Not,
                        _ => throw new NotImplementedException($"No conversion from unary operator \"{TokenPrinter.Print(unary.Operator)}\" to assembly")
                    };

                    instructions.Add(new MovAsmNode(srcOperand, destOperand));
                    instructions.Add(new UnaryAsmNode(unaryOperator, destOperand));
                    break;
                default:
                    throw new NotImplementedException($"No conversion from {instruction.GetType().Name} to assembly is implemented");
            }
        }
        return instructions;
    }

    OperandAsmNode Operand(ValueTACNode value)
    {
        return value switch
        {
            ConstantTACNode constant => new ImmediateAsmNode(constant.Value),
            VarTACNode variable => new PseudoAsmNode(variable.Name),
            _ => throw new NotImplementedException($"No assembly operand defined for {value.GetType()}")
        };
    }
}