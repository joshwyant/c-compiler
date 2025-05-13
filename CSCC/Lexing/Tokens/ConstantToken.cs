namespace CSCC.Lexing.Tokens;

class ConstantToken(int value) : Token(TokenType.Constant)
{
    public readonly int Value = value;
}