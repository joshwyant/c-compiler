namespace CSCC.TAC.Syntax;

class ConstantTACNode(int constant) : ValueTACNode
{
    public int Value { get; } = constant;
}