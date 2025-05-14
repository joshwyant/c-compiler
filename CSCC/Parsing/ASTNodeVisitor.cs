using CSCC.Parsing.Syntax;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;

namespace CSCC.Parsing;

abstract class ASTNodeVisitor
{
    public void Visit(ASTNode node)
    {
        switch (node)
        {
            case ProgramNode program: VisitProgram(program); break;
            case FunctionDefinitionNode function: VisitFunction(function); break;
            case StatementNode statement: VisitStatement(statement); break;
            case ExpressionNode expression: VisitExpression(expression); break;
        }
    }
    public void VisitStatement(StatementNode node)
    {
        switch (node)
        {
            case ReturnStatementNode ret: VisitReturn(ret); break;
        }
    }
    public void VisitExpression(ExpressionNode node)
    {
        switch (node)
        {
            case ConstantExpressionNode constant: VisitConstant(constant); break;
        }
    }
    public abstract void VisitProgram(ProgramNode node);
    public abstract void VisitFunction(FunctionDefinitionNode node);
    public abstract void VisitReturn(ReturnStatementNode node);
    public abstract void VisitConstant(ConstantExpressionNode node);
}