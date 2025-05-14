using System.CodeDom.Compiler;
using System.Text;
using System.Threading.Tasks;
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

    Task<string> PrintAsync(CancellationToken token = default) => visitor.PrintAsync(node, token);

    public static async Task<string> PrintAsync(AssemblyNode node, CancellationToken token = default) => await new AsmTreePrinter(node).PrintAsync(token);

    class AsmTreePrinterVisitor : AssemblyNodeVisitor
    {
        readonly StringBuilder sb = new();
        readonly IndentedTextWriter indentedWriter;

        public AsmTreePrinterVisitor()
        {
            indentedWriter = new(new StringWriter(sb), "    ");
        }

        public async Task<string> PrintAsync(AssemblyNode node, CancellationToken cancellationToken = default)
        {
            await VisitAsync(node, cancellationToken);
            var str = sb.ToString();
            sb.Clear();
            return str;
        }

        protected override async Task VisitFunctionDefinitionAsync(FunctionDefinitionAsmNode func, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("Function(".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            await indentedWriter.WriteLineAsync($"name=\"{func.Name}\",".AsMemory(), cancellationToken);
            await indentedWriter.WriteLineAsync("body=(".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            foreach (var instruction in func.Instructions)
            {
                await VisitAsync(instruction, cancellationToken);
            }
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
        }

        protected override async Task VisitImmediateAsync(ImmediateAsmNode imm, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync($"Imm({imm.Value})".AsMemory(), cancellationToken);
        }

        protected override async Task VisitMovAsync(MovAsmNode mov, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync($"Mov({mov.Source}, {mov.Destination})".AsMemory(), cancellationToken);
        }

        protected override async Task VisitProgramAsync(ProgramAsmNode program, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("Program(".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            await VisitAsync(program.Function, cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync(")".AsMemory(), cancellationToken);
        }

        protected override async Task VisitRegisterAsync(RegisterAsmNode register, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync("Register".AsMemory(), cancellationToken);
        }

        protected override async Task VisitRetAsync(RetAsmNode ret, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("Ret".AsMemory(), cancellationToken);
        }
    }
}