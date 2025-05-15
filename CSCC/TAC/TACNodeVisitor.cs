using CSCC.TAC.Syntax;
using CSCC.TAC.Syntax.Instructions;

namespace CSCC.TAC;

abstract class TACNodeVisitor
{
    public void Visit(TACNode node)
    {
        switch (node)
        {
            case ValueTACNode value: VisitValue(value); break;
            case InstructionTACNode instruction: VisitInstruction(instruction); break;
            case ProgramTACNode program: VisitProgramTACNode(program); break;
            case FunctionDefinitionTACNode function: VisitFunctionTACNode(function); break;
            default: throw new NotImplementedException($"No visitor for node type {node.GetType()}");
        }
    }
    public void VisitValue(ValueTACNode value)
    {
        switch (value)
        {
            case VarTACNode variable: VisitVarTACNode(variable); break;
            case ConstantTACNode constant: VisitConstantTACNode(constant); break;
            default: throw new NotImplementedException($"No visitor for value type {value.GetType()}");
        }
    }

    public void VisitInstruction(InstructionTACNode instruction)
    {
        switch (instruction)
        {
            case ReturnInstructionTACNode ret: VisitReturnTACNode(ret); break;
            case UnaryInstructionTACNode unary: VisitUnaryTACNode(unary); break;
            default: throw new NotImplementedException($"No visitor for instruction type {instruction.GetType()}");
        }
    }

    public abstract void VisitVarTACNode(VarTACNode variable);
    public abstract void VisitConstantTACNode(ConstantTACNode constant);
    public abstract void VisitProgramTACNode(ProgramTACNode program);
    public abstract void VisitFunctionTACNode(FunctionDefinitionTACNode function);
    public abstract void VisitReturnTACNode(ReturnInstructionTACNode instruction);
    public abstract void VisitUnaryTACNode(UnaryInstructionTACNode instruction);
}