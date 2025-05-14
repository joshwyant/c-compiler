namespace CSCC.Lexing;

static class LexicalExtensions
{
    public static string ToRepresentation(this TokenType tokenType) => TokenPrinter.Print(tokenType);
}