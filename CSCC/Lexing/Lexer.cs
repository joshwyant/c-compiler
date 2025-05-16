
using System.Runtime.CompilerServices;
using System.Text;
using CSCC.Lexing.Tokens;
using static CSCC.Lexing.TokenType;

namespace CSCC.Lexing;

class Lexer(TextReader reader) : IAsyncEnumerable<Token>
{
    IAsyncEnumerator<Token>? tokenEnumerator;
    IAsyncEnumerator<char>? charEnumerator;
    readonly Stack<char> stack = [];
    readonly char[] buffer = [];
    readonly List<string> errors = [];

    public IReadOnlyList<string> Errors => errors;

    public Lexer(string fileName) : this(new StreamReader(fileName)) { }

    public IAsyncEnumerator<Token> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => tokenEnumerator ??= Lex(cancellationToken).GetAsyncEnumerator(cancellationToken);

    async Task<char?> PeekAsync(CancellationToken cancellationToken = default)
    {
        if (stack.Count > 0)
        {
            return stack.Peek();
        }
        var result = await ReadAsync(cancellationToken);

        if (result is not char read) return null;

        stack.Push(read);

        return read;
    }
    async IAsyncEnumerable<char> ReadTextBufferAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var buffer = new char[512];
        int numChars;

        do
        {
            numChars = await reader.ReadAsync(buffer, cancellationToken);
            for (var i = 0; i < numChars; i++)
            {
                yield return buffer[i];
            }
        } while (numChars != 0);
    }
    async Task<char?> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (stack.TryPop(out var c))
        {
            return c;
        }

        charEnumerator ??= ReadTextBufferAsync(cancellationToken).GetAsyncEnumerator(cancellationToken);

        if (!await charEnumerator.MoveNextAsync()) return null;

        return charEnumerator.Current;
    }

    private async IAsyncEnumerable<Token> Lex([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        char prev = default;
        char c = default;
        async Task<bool> move()
        {
            var result = await ReadAsync(cancellationToken);
            if (result is char value)
            {
                prev = c;
                c = value;
                return true;
            }
            return false;
        }
        async Task<char> peek() => await PeekAsync(cancellationToken) ?? '\0';
        using (reader)
        {
            while (await move())
            {
                var builder = new StringBuilder();
                var tokenType = None;
                while (char.IsWhiteSpace(c))
                {
                    if (!await move()) yield break;
                }
                if (char.IsDigit(c))
                {
                    var isBadIdentifier = false; // The book tests force us to do this
                    do
                    {
                        if (char.IsLetter(c) || c == '_')
                        {
                            isBadIdentifier = true;
                        }
                        builder.Append(c);
                        c = await peek();
                    } while ((char.IsLetterOrDigit(c) || c == '_') && await move()); // c is peeked

                    if (isBadIdentifier)
                    {
                        errors.Add($"Bad identifier \"{builder}\"");
                        continue;
                    }

                    var n = int.Parse(builder.ToString());
                    yield return new ConstantToken(n);
                    continue;
                }
                if (char.IsLetter(c) || c == '_')
                {
                    do
                    {
                        builder.Append(c);
                        c = await peek();
                    } while ((char.IsLetterOrDigit(c) || c == '_') && await move()); // c is peeked

                    var ident = builder.ToString();
                    tokenType = ident switch
                    {
                        "int" => Int,
                        "void" => TokenType.Void,
                        "return" => Return,
                        _ => Identifier
                    };
                    yield return tokenType == Identifier
                        ? new IdentifierToken(ident)
                        : new Token(tokenType); // Keyword
                    continue;
                }
                tokenType = c switch
                {
                    '(' => OpenParenthesis,
                    ')' => CloseParenthesis,
                    '{' => OpenBrace,
                    '}' => CloseBrace,
                    ';' => Semicolon,
                    '~' => Tilde,
                    '-' => Hyphen,
                    '+' => Plus,
                    '*' => Asterisk,
                    '/' => ForwardSlash,
                    '%' => Percent,
                    _ => None,
                };

                switch (tokenType)
                {
                    case None:
                        errors.Add($"Unexpected token: {prev}");
                        continue;
                    case Hyphen:
                        if (await peek() == '-')
                        {
                            await move();
                            tokenType = Decrement;
                        }
                        break;
                }
                yield return new Token(tokenType);
            }
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