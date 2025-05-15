using CSCC.Lexing;

namespace CSCC.Parsing.Syntax.Expressions;

class UnaryExpressionNode : ExpressionNode
{
    public TokenType Operator { get; }
    public ExpressionNode Expression { get; }

    public UnaryExpressionNode(TokenType @operator, ExpressionNode expression)
    {
        switch (@operator)
        {
            case TokenType.Hyphen:
            case TokenType.Tilde:
                break;
            default:
                throw new ArgumentException($"\"{TokenPrinter.Print(@operator)}\" can't be a unary operator.");
        }
        Operator = @operator;
        Expression = expression;
    }
}