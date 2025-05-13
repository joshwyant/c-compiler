namespace CSCC.Lexing.Tokens;

class ConstantToken(int value) : Token(TokenType.Constant)
{
    public readonly int Value = value;

    public override string ToString() => Value.ToString();
}