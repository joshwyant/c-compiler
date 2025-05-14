using System.CodeDom.Compiler;
using System.Text;
using CSCC.CodeGen.Syntax.Instructions;

namespace CSCC.CodeGen.Syntax;

class AsmTreePrinter
{
    readonly AssemblyNode node;
    readonly AsmTreePrinterVisitor visitor;
    AsmTreePrinter(AssemblyNode node)
    {
        this.node = node;
        this.visitor = new();
    }

    string Print() => visitor.Print(node);

    public static string Print(AssemblyNode node) => new AsmTreePrinter(node).Print();

    class AsmTreePrinterVisitor : AssemblyNodeVisitor
    {
        readonly StringBuilder sb = new();
        readonly IndentedTextWriter indentedWriter;

        public AsmTreePrinterVisitor()
        {
            indentedWriter = new(new StringWriter(sb), "    ");
        }

        public string Print(AssemblyNode node)
        {
            Visit(node);
            var str = sb.ToString();
            sb.Clear();
            return str;
        }

        protected override void VisitFunctionDefinition(FunctionDefinitionAsmNode func)
        {
            indentedWriter.WriteLine("Function(");
            indentedWriter.Indent++;
            indentedWriter.WriteLine($"name=\"{func.Name}\",");
            indentedWriter.WriteLine("body=(");
            indentedWriter.Indent++;
            foreach (var instruction in func.Instructions)
            {
                Visit(instruction);
            }
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
        }

        protected override void VisitImmediate(ImmediateAsmNode imm)
        {
            indentedWriter.Write($"Imm({imm.Value})");
        }

        protected override void VisitMov(MovAsmNode mov)
        {
            indentedWriter.WriteLine($"Mov({mov.Source}, {mov.Destination})");
        }

        protected override void VisitProgram(ProgramAsmNode program)
        {
            indentedWriter.WriteLine("Program(");
            indentedWriter.Indent++;
            Visit(program.Function);
            indentedWriter.Indent--;
            indentedWriter.WriteLine(")");
        }

        protected override void VisitRegister(RegisterAsmNode register)
        {
            indentedWriter.Write("Register");
        }

        protected override void VisitRet(RetAsmNode ret)
        {
            indentedWriter.WriteLine("Ret");
        }
    }
}