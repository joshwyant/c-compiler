namespace CSCC.Lexing;

readonly struct Token(TokenType type)
{
    readonly TokenType Type = type;
}