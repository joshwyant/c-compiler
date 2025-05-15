using System.CodeDom.Compiler;
using System.Text;
using CSCC.Lexing;
using CSCC.Parsing.Syntax.Expressions;
using CSCC.Parsing.Syntax.Statements;

namespace CSCC.Parsing.Syntax;

class TreePrinter
{
    readonly ASTNode node;
    readonly TreePrinterVisitor visitor;
    TreePrinter(ASTNode node)
    {
        this.node = node;
        this.visitor = new();
    }

    async Task<string> PrintAsync(CancellationToken cancellationToken = default) => await visitor.PrintAsync(node, cancellationToken);

    public static async Task<string> PrintAsync(ASTNode node, CancellationToken cancellationToken = default) => await new TreePrinter(node).PrintAsync(cancellationToken);

    class TreePrinterVisitor : ASTNodeVisitor
    {
        readonly StringBuilder sb = new();
        readonly IndentedTextWriter indentedWriter;

        public TreePrinterVisitor()
        {
            indentedWriter = new(new StringWriter(sb), "    ");
        }

        public async Task<string> PrintAsync(ASTNode node, CancellationToken cancellationToken = default)
        {
            await VisitAsync(node, cancellationToken);
            var str = sb.ToString();
            sb.Clear();
            return str;
        }

        public override async Task VisitConstantAsync(ConstantExpressionNode node, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync($"Constant({node.Constant})".AsMemory(), cancellationToken);
        }

        public override async Task VisitFunctionAsync(FunctionDefinitionNode node, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("Function(".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            await indentedWriter.WriteLineAsync($"name=\"{node.Name}\",".AsMemory(), cancellationToken);
            await indentedWriter.WriteAsync("body=".AsMemory(), cancellationToken);
            await VisitAsync(node.Statement, cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
        }

        public override async Task VisitProgramAsync(ProgramNode node, CancellationToken cancellationToken = default)
        {
            indentedWriter.WriteLine("Program(".AsMemory());
            indentedWriter.Indent++;
            await VisitAsync(node.Function, cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
        }

        public override async Task VisitReturnAsync(ReturnStatementNode node, CancellationToken cancellationToken = default)
        {
            indentedWriter.WriteLine("Return(".AsMemory());
            indentedWriter.Indent++;
            await VisitAsync(node.Expression, cancellationToken);
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")".AsMemory());
        }

        public override async Task VisitUnaryAsync(UnaryExpressionNode node, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("Unary(".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            await indentedWriter.WriteLineAsync($"operator=\"{TokenPrinter.Print(node.Operator)}\",".AsMemory(), cancellationToken);
            await indentedWriter.WriteAsync("expression=".AsMemory(), cancellationToken);
            await VisitAsync(node.Expression, cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
        }
    }
}