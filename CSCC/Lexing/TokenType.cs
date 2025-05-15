namespace CSCC.Lexing;

enum TokenType
{
    None,
    Identifier,
    Constant,
    Int,
    Void,
    Return,
    OpenParenthesis,
    CloseParenthesis,
    OpenBrace,
    CloseBrace,
    Semicolon,
    Tilde,
    Hyphen,
    Decrement,
    EOF,
}