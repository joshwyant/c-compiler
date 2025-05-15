using System.Runtime.CompilerServices;
using CSCC.Lexing;
using CSCC.Lexing.Tokens;
using CSCC.Parsing.Syntax;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;
using CSCC.Tools;
using static CSCC.Lexing.TokenType;

namespace CSCC.Parsing;

class Parser(IAsyncEnumerable<Token> tokens)
{
    readonly IAsyncEnumerable<Token> tokenEnumerable = tokens;
    readonly List<string> errors = [];
    readonly Stack<Token> tokenStack = [];
    IAsyncEnumerator<Token>? tokenEnumerator;
    public IReadOnlyList<string> Errors => errors;
    ProgramNode? parse;
    Token nextToken = new(None);
    public Parser(IEnumerable<Token> tokens) : this(new SyncAsyncEnumerable<Token>(tokens)) { }
    async Task<Token?> ReadAsync(CancellationToken cancellationToken = default)
    {
        tokenStack.TryPop(out var result);

        if (result is Token popped) return popped;

        tokenEnumerator ??= tokenEnumerable.GetAsyncEnumerator(cancellationToken);

        if (!await tokenEnumerator.MoveNextAsync()) return null;
        return tokenEnumerator.Current;
    }
    async Task<Token?> PeekAsync(CancellationToken cancellationToken = default)
    {
        tokenStack.TryPeek(out var result);

        if (result is Token peekedToken) return peekedToken;

        result = await ReadAsync(cancellationToken);

        if (result is Token readToken)
        {
            tokenStack.Push(readToken);
        }

        return result;
    }

    async ValueTask<bool> Eof(CancellationToken cancellationToken = default)
    {
        return null == await PeekAsync(cancellationToken);
    }

    async ValueTask<Token?> ExpectAsync(TokenType tokenType, string? message = null, CancellationToken ct = default)
    {
        var nextToken = await ReadAsync(ct) ?? new Token(EOF);
        var matches = tokenType == nextToken.Type;

        if (!matches)
        {
            errors.Add(message ?? $"Expected \"{tokenType.ToRepresentation()}\", got \"{nextToken}\"");
        }

        if (nextToken.Type == EOF) return null;

        return nextToken;
    }

    public async Task<ProgramNode?> ParseAsync(CancellationToken cancellationToken = default)
    {
        return parse ??= await ProgramAsync(cancellationToken);
    }

    async Task<ProgramNode?> ProgramAsync(CancellationToken cancellationToken = default)
    {
        if (await FunctionAsync(cancellationToken) is not FunctionDefinitionNode func) return null;

        if (await PeekAsync(cancellationToken) is Token unexpected)
        {
            errors.Add($"Unexpected token \"{unexpected}\"");
        }

        return new ProgramNode(func);
    }

    async Task<FunctionDefinitionNode?> FunctionAsync(CancellationToken cancellationToken = default)
    {
        async Task<Token?> expect(TokenType tt) => await ExpectAsync(tt, ct: cancellationToken);

        if (await expect(Int) is null) return null;
        if (await expect(Identifier) is not IdentifierToken ident) return null;
        await expect(OpenParenthesis);
        if ((await PeekAsync(cancellationToken))?.Type == TokenType.Void) await ReadAsync(cancellationToken);
        await expect(CloseParenthesis);
        await expect(OpenBrace);
        if (await StatementAsync(cancellationToken) is not StatementNode stmt)
        {
            errors.Add("Statement expected.");
            await expect(CloseBrace);
            return null;
        }
        await expect(CloseBrace);

        return new FunctionDefinitionNode(ident.Characters!, stmt);
    }

    async Task<StatementNode?> StatementAsync(CancellationToken cancellationToken = default)
    {
        var returnStatement = await ReturnAsync(cancellationToken);

        await ExpectAsync(Semicolon, ct: cancellationToken);

        return returnStatement;
    }

    async Task<ReturnStatementNode?> ReturnAsync(CancellationToken cancellationToken = default)
    {
        if (await ExpectAsync(Return, ct: cancellationToken) is null) return null;

        if (await ExpressionAsync(cancellationToken) is not ExpressionNode exp)
        {
            errors.Add("Expression expected in return statement.");
            return null;
        }

        return new ReturnStatementNode(exp);
    }

    async Task<ExpressionNode?> ExpressionAsync(CancellationToken cancellationToken = default)
    {
        var token = await PeekAsync(cancellationToken);
        switch (token?.Type)
        {
            case Hyphen:
            case Tilde:
            case OpenParenthesis:
                {
                    await ReadAsync(cancellationToken);
                    var expr = await ExpressionAsync(cancellationToken);
                    if (expr == null)
                    {
                        errors.Add($"Expression expected after {TokenPrinter.Print(token.Type)}");
                        return null;
                    }
                    if (token.Type == OpenParenthesis)
                    {
                        await ExpectAsync(CloseParenthesis, ct: cancellationToken);
                        return expr;
                    }
                    return new UnaryExpressionNode(token.Type, expr);
                }
            case Constant:
                await ReadAsync(cancellationToken);
                return new ConstantExpressionNode(((ConstantToken)token).Value);

            default:
                errors.Add("Expected expression");
                return null;
        }
    }
}