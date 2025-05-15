using CSCC.Parsing.Syntax;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;

namespace CSCC.Parsing;

abstract class ASTNodeVisitor
{
    public async Task VisitAsync(ASTNode node, CancellationToken cancellationToken = default)
    {
        switch (node)
        {
            case ProgramNode program: await VisitProgramAsync(program, cancellationToken); break;
            case FunctionDefinitionNode function: await VisitFunctionAsync(function, cancellationToken); break;
            case StatementNode statement: await VisitStatementAsync(statement, cancellationToken); break;
            case ExpressionNode expression: await VisitExpressionAsync(expression, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for node type {node.GetType()}");
        }
    }
    public async Task VisitStatementAsync(StatementNode node, CancellationToken cancellationToken = default)
    {
        switch (node)
        {
            case ReturnStatementNode ret: await VisitReturnAsync(ret, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for statement type {node.GetType()}");
        }
    }
    public async Task VisitExpressionAsync(ExpressionNode node, CancellationToken cancellationToken = default)
    {
        switch (node)
        {
            case ConstantExpressionNode constant: await VisitConstantAsync(constant, cancellationToken); break;
            case UnaryExpressionNode unary: await VisitUnaryAsync(unary, cancellationToken); break;
            default: throw new NotImplementedException($"No visitor for expression type {node.GetType()}");
        }
    }
    public abstract Task VisitProgramAsync(ProgramNode node, CancellationToken cancellationToken = default);
    public abstract Task VisitFunctionAsync(FunctionDefinitionNode node, CancellationToken cancellationToken = default);
    public abstract Task VisitReturnAsync(ReturnStatementNode node, CancellationToken cancellationToken = default);
    public abstract Task VisitConstantAsync(ConstantExpressionNode node, CancellationToken cancellationToken = default);
    public abstract Task VisitUnaryAsync(UnaryExpressionNode node, CancellationToken cancellationToken = default);
}