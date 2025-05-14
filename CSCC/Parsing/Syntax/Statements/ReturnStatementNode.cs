using CSCC.Parsing.Syntax.Expressions;

namespace CSCC.Parsing.Syntax.Statements;

class ReturnStatementNode(ExpressionNode expression) : StatementNode
{
    public ExpressionNode Expression { get; } = expression;
}