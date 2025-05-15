namespace CSCC.TAC.Syntax;

class ProgramTACNode(FunctionDefinitionTACNode func) : TACNode
{
    public FunctionDefinitionTACNode Function { get; } = func;
}