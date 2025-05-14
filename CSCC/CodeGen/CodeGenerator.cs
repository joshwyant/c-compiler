using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;
using CSCC.Parsing;
using CSCC.Parsing.Syntax;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;

namespace CSCC.CodeGen;

class CodeGenerator(ProgramNode programAstNode)
{
    ProgramNode programAstNode = programAstNode;
    ProgramAsmNode? programNode;

    public ProgramAsmNode Generate()
    {
        return programNode ??= Program();
    }

    ProgramAsmNode Program()
    {
        var astFunc = programAstNode.Function;
        return new ProgramAsmNode(new FunctionDefinitionAsmNode(astFunc.Name, Instructions(astFunc.Statement)));
    }
    InstructionAsmNode[] Instructions(StatementNode stmt)
    {
        var instructions = new List<InstructionAsmNode>();
        switch (stmt)
        {
            case ReturnStatementNode ret:
                var constExpr = (ConstantExpressionNode)ret.Expression;
                instructions.Add(new MovAsmNode(new ImmediateAsmNode(constExpr.Constant), new RegisterAsmNode()));
                instructions.Add(new RetAsmNode());
                break;
        }
        return [.. instructions];
    }
}