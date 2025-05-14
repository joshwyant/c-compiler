namespace CSCC.CodeGen.Syntax;

class ImmediateAsmNode(int value) : OperandAsmNode
{
    public int Value { get; } = value;
}