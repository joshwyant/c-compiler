namespace CSCC.Lexing;

class Token(TokenType type, string? characters = null)
{
    readonly TokenType Type = type;
    readonly string? Characters = characters;
}