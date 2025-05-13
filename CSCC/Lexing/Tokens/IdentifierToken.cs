namespace CSCC.Lexing.Tokens;

class IdentifierToken(string name) : Token(TokenType.Identifier, name) { }