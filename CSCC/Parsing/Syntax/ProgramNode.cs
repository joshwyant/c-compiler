namespace CSCC.Parsing.Syntax;

class ProgramNode(FunctionDefinitionNode function) : ASTNode
{
    public FunctionDefinitionNode Function { get; } = function;
}