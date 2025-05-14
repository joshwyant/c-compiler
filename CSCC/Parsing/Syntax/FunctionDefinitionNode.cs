using CSCC.Parsing.Syntax.Statements;

namespace CSCC.Parsing.Syntax;

class FunctionDefinitionNode(string name, StatementNode statement) : ASTNode
{
    public string Name { get; } = name;
    public StatementNode Statement { get; } = statement;
}