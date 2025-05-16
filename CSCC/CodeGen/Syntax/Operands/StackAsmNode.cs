namespace CSCC.CodeGen.Syntax.Operands;

class StackAsmNode(int position) : OperandAsmNode
{
    public int Position { get; } = position;
}