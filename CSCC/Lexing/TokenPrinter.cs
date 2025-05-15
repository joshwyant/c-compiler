using CSCC.Lexing.Tokens;
using static CSCC.Lexing.TokenType;

namespace CSCC.Lexing;

static class TokenPrinter
{
    public static string Print(TokenType tokenType) =>
        tokenType switch
        {
            Int => "int",
            TokenType.Void => "void",
            Return => "return",
            OpenParenthesis => "(",
            CloseParenthesis => ")",
            OpenBrace => "{",
            CloseBrace => "}",
            Semicolon => ";",
            Tilde => "~",
            Hyphen => "-",
            Decrement => "--",
            _ => tokenType.ToString()
        };

    public static string Print(Token token) =>
        token switch
        {
            ConstantToken constant => constant.Value.ToString(),
            IdentifierToken ident when ident.Characters is string chars => chars,
            Token t when t.Characters is string chars => chars,
            _ => Print(token.Type)
        };
}