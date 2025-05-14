namespace CSCC.Lexing;
using static TokenType;

class Token(TokenType type, string? characters = null)
{
    public TokenType Type { get; } = type;
    public string? Characters { get; } = characters;

    public override string ToString() => TokenPrinter.Print(this);
}