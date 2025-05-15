using CSCC.Parsing.Syntax;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;
using CSCC.TAC.Syntax;
using CSCC.TAC.Syntax.Instructions;

namespace CSCC.TAC;

class TACGenerator(ProgramNode programNode)
{
    public ProgramNode Program { get; } = programNode;
    private int nextVariableId = 0;

    public ProgramTACNode? Generate(ProgramNode programAst)
    {
        var instructions = new List<InstructionTACNode>();
        Statement(programAst.Function.Statement, instructions);
        if (instructions.Count == 0)
        {
            return null;
        }
        var fn = new FunctionDefinitionTACNode(programAst.Function.Name, [.. instructions]);
        return new ProgramTACNode(fn);
    }

    VarTACNode MakeTemporary() => new($"tmp.{nextVariableId++}");

    void Statement(StatementNode statement, List<InstructionTACNode> instructions)
    {
        switch (statement)
        {
            case ReturnStatementNode ret:
                Return(ret, instructions);
                break;
            default:
                throw new NotImplementedException($"No procedure for statement type {statement.GetType()}");
        }
    }

    ValueTACNode Expression(ExpressionNode expression, List<InstructionTACNode> instructions) =>
        expression switch
        {
            UnaryExpressionNode unary => Unary(unary, instructions),
            ConstantExpressionNode constant => Constant(constant, instructions),
            _ => throw new NotImplementedException($"No procedure for expression type {expression.GetType()}"),
        };

    void Return(ReturnStatementNode ret, List<InstructionTACNode> instructions)
    {
        instructions.Add(
            new ReturnInstructionTACNode(
                Expression(ret.Expression, instructions)));
    }

    ConstantTACNode Constant(ConstantExpressionNode constant, List<InstructionTACNode> instructions) =>
        new(constant.Constant);

    VarTACNode Unary(UnaryExpressionNode unary, List<InstructionTACNode> instructions)
    {
        var dest = MakeTemporary();

        instructions.Add(
            new UnaryInstructionTACNode(
                unary.Operator,
                Expression(unary.Expression, instructions),
                dest));

        return dest;
    }
}