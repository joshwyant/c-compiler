using CSCC.Lexing.Tokens;

namespace CSCC.Parsing.Syntax.Expressions;

class ConstantExpressionNode(ConstantToken constantToken) : ExpressionNode
{
    public ConstantToken ConstantToken { get; } = constantToken;
}