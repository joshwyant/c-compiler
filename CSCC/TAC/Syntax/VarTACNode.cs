namespace CSCC.TAC.Syntax;

class VarTACNode(string name) : ValueTACNode
{
    public string Name { get; } = name;
}