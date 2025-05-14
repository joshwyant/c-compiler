using CSCC.Lexing.Tokens;

namespace CSCC.Parsing.Syntax.Expressions;

class ConstantExpressionNode(int constant) : ExpressionNode
{
    public int Constant { get; } = constant;
}