
using System.Runtime.CompilerServices;

namespace CSCC.Lexing;

class Lexer(TextReader reader) : IAsyncEnumerable<Token>
{
    IAsyncEnumerable<Token>? lexerEnumerable;

    public Lexer(string fileName) : this(new StreamReader(fileName)) { }

    public IAsyncEnumerator<Token> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => (lexerEnumerable ??= Lex(cancellationToken)).GetAsyncEnumerator(cancellationToken);

    private async IAsyncEnumerable<Token> Lex([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using (reader)
        {
            await reader.ReadLineAsync(cancellationToken); // dummy operation
            yield return new Token(TokenType.None);
        }
    }

    public async Task<Token[]> ToArrayAsync(CancellationToken cancellationToken = default)
    {
        var list = new List<Token>();

        await foreach (var token in this.WithCancellation(cancellationToken))
        {
            list.Add(token);
        }

        return [.. list];
    }
}