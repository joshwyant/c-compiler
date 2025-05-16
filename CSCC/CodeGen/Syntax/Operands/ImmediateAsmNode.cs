namespace CSCC.CodeGen.Syntax.Operands;

class ImmediateAsmNode(int value) : OperandAsmNode
{
    public int Value { get; } = value;
}