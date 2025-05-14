using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;

namespace CSCC.CodeGen;

abstract class AssemblyNodeVisitor
{
    public void Visit(AssemblyNode node)
    {
        switch (node)
        {
            case OperandAsmNode operand: VisitOperand(operand); break;
            case InstructionAsmNode instruction: VisitInstruction(instruction); break;
            case ProgramAsmNode program: VisitProgram(program); break;
            case FunctionDefinitionAsmNode func: VisitFunctionDefinition(func); break;
        }
    }

    protected abstract void VisitFunctionDefinition(FunctionDefinitionAsmNode func);
    protected abstract void VisitProgram(ProgramAsmNode program);

    public void VisitOperand(OperandAsmNode operand)
    {
        switch (operand)
        {
            case RegisterAsmNode register: VisitRegister(register); break;
            case ImmediateAsmNode imm: VisitImmediate(imm); break;
        }
    }

    protected abstract void VisitImmediate(ImmediateAsmNode imm);
    protected abstract void VisitRegister(RegisterAsmNode register);

    public void VisitInstruction(InstructionAsmNode instruction)
    {
        switch (instruction)
        {
            case MovAsmNode mov: VisitMov(mov); break;
            case RetAsmNode ret: VisitRet(ret); break;
        }
    }

    protected abstract void VisitRet(RetAsmNode ret);
    protected abstract void VisitMov(MovAsmNode mov);
}