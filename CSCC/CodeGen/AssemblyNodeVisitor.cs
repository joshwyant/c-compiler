using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;
using CSCC.CodeGen.Syntax.Operands;

namespace CSCC.CodeGen;

abstract class AssemblyNodeVisitor
{
    public async Task VisitAsync(AssemblyNode node, CancellationToken cancellationToken = default)
    {
        switch (node)
        {
            case OperandAsmNode operand: await VisitOperand(operand, cancellationToken); break;
            case InstructionAsmNode instruction: await VisitInstructionAsync(instruction, cancellationToken); break;
            case ProgramAsmNode program: await VisitProgramAsync(program, cancellationToken); break;
            case FunctionDefinitionAsmNode func: await VisitFunctionDefinitionAsync(func, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for node {node.GetType().Name}");
        }
    }

    protected abstract Task VisitFunctionDefinitionAsync(FunctionDefinitionAsmNode func, CancellationToken cancellationToken = default);
    protected abstract Task VisitProgramAsync(ProgramAsmNode program, CancellationToken cancellationToken = default);

    public async Task VisitOperand(OperandAsmNode operand, CancellationToken cancellationToken = default)
    {
        switch (operand)
        {
            case RegisterAsmNode register: await VisitRegisterAsync(register, cancellationToken); break;
            case ImmediateAsmNode imm: await VisitImmediateAsync(imm, cancellationToken); break;
            case PseudoAsmNode ps: await VisitPseudoOperandAsync(ps, cancellationToken); break;
            case StackAsmNode stack: await VisitStackOperandAsync(stack, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for operand type {operand.GetType().Name}");
        }
    }

    protected abstract Task VisitImmediateAsync(ImmediateAsmNode imm, CancellationToken cancellationToken = default);
    protected abstract Task VisitRegisterAsync(RegisterAsmNode register, CancellationToken cancellationToken = default);
    protected abstract Task VisitPseudoOperandAsync(PseudoAsmNode ps, CancellationToken cancellationToken = default);
    protected abstract Task VisitStackOperandAsync(StackAsmNode stack, CancellationToken cancellationToken = default);

    public async Task VisitInstructionAsync(InstructionAsmNode instruction, CancellationToken cancellationToken = default)
    {
        switch (instruction)
        {
            case MovAsmNode mov: await VisitMovAsync(mov, cancellationToken); break;
            case RetAsmNode ret: await VisitRetAsync(ret, cancellationToken); break;
            case AllocateStackAsmNode alloc: await VisitAllocateStackAsync(alloc, cancellationToken); break;
            case UnaryAsmNode unary: await VisitUnaryAsync(unary, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for instruction type {instruction.GetType().Name}");
        }
    }

    protected abstract Task VisitRetAsync(RetAsmNode ret, CancellationToken cancellationToken = default);
    protected abstract Task VisitMovAsync(MovAsmNode mov, CancellationToken cancellationToken = default);
    protected abstract Task VisitAllocateStackAsync(AllocateStackAsmNode alloc, CancellationToken cancellationToken = default);
    protected abstract Task VisitUnaryAsync(UnaryAsmNode unary, CancellationToken cancellationToken = default);
}