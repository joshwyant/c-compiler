namespace CSCC.Lexing;
using static TokenType;

class Token(TokenType type, string? characters = null)
{
    readonly TokenType Type = type;
    readonly string? Characters = characters;

    public override string ToString() =>
        Characters ?? Type switch
        {
            Int => "int",
            Void => "void",
            Return => "return",
            OpenParenthesis => "(",
            CloseParenthesis => ")",
            OpenBrace => "{",
            CloseBrace => "}",
            Semicolon => ";",
            _ => Type.ToString()
        };
}