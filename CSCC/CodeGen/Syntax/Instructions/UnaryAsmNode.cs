using CSCC.CodeGen.Syntax.Operands;

namespace CSCC.CodeGen.Syntax.Instructions;

class UnaryAsmNode(AssemblyOperator @operator, OperandAsmNode operand) : InstructionAsmNode([operand])
{
    public AssemblyOperator Operator { get; } = @operator;
    public OperandAsmNode Operand => Operands[0];
}