using System.CodeDom.Compiler;
using System.Text;
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

    string Print() => visitor.Print(node);

    public static string Print(ASTNode node) => new TreePrinter(node).Print();

    class TreePrinterVisitor : ASTNodeVisitor
    {
        readonly StringBuilder sb = new();
        readonly IndentedTextWriter indentedWriter;

        public TreePrinterVisitor()
        {
            indentedWriter = new(new StringWriter(sb), "    ");
        }

        public string Print(ASTNode node)
        {
            Visit(node);
            var str = sb.ToString();
            sb.Clear();
            return str;
        }

        public override void VisitConstant(ConstantExpressionNode node)
        {
            indentedWriter.WriteLine($"Constant({node.Constant})");
        }

        public override void VisitFunction(FunctionDefinitionNode node)
        {
            indentedWriter.WriteLine("Function(");
            indentedWriter.Indent++;
            indentedWriter.WriteLine($"name=\"{node.Name}\",");
            indentedWriter.Write("body=");
            Visit(node.Statement);
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
        }

        public override void VisitProgram(ProgramNode node)
        {
            indentedWriter.WriteLine("Program(");
            indentedWriter.Indent++;
            Visit(node.Function);
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
        }

        public override void VisitReturn(ReturnStatementNode node)
        {
            indentedWriter.WriteLine("Return(");
            indentedWriter.Indent++;
            Visit(node.Expression);
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
        }
    }
}