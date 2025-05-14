using System.CodeDom.Compiler;
using System.Text;
using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;

namespace CSCC.CodeGen.Emit;

class AssemblyWriter
{
    readonly ProgramAsmNode program;
    public AssemblyWriter(ProgramAsmNode program)
    {
        this.program = program;
    }

    public void Write(string filename)
    {
        using var visitor = new AsmWriterVisitor(filename, program);
        visitor.Write();
    }

    class AsmWriterVisitor : AssemblyNodeVisitor, IDisposable
    {
        readonly StreamWriter writer;
        readonly IndentedTextWriter indentedWriter;
        readonly ProgramAsmNode program;

        public AsmWriterVisitor(string filename, ProgramAsmNode program)
        {
            writer = new StreamWriter(filename);
            indentedWriter = new(writer, "        ");
            this.program = program;
        }

        public void Dispose()
        {
            writer.Flush();
            writer.Close();
            writer.Dispose();
        }

        public void Write()
        {
            Visit(program);
            if (OperatingSystem.IsLinux())
            {
                indentedWriter.WriteLine();
                indentedWriter.WriteLine(".section .note.GNU-stack,\"\",@progbits");
            }
        }

        protected override void VisitFunctionDefinition(FunctionDefinitionAsmNode func)
        {
            var funcName = OperatingSystem.IsMacOS() ? $"_{func.Name}" : func.Name;

            indentedWriter.WriteLine($"# Function {func.Name}");
            indentedWriter.Indent++;
            indentedWriter.WriteLine($".globl {funcName}");
            indentedWriter.Indent--;
            indentedWriter.WriteLine($"{funcName}:");
            indentedWriter.Indent++;
            foreach (var instruction in func.Instructions)
            {
                Visit(instruction);
            }
            indentedWriter.Indent--;
        }

        protected override void VisitImmediate(ImmediateAsmNode imm)
        {
            indentedWriter.Write($"${imm.Value}");
        }

        protected override void VisitMov(MovAsmNode mov)
        {
            indentedWriter.Write($"movl ");
            Visit(mov.Source);
            indentedWriter.Write(", ");
            Visit(mov.Destination);
            indentedWriter.WriteLine();
        }

        protected override void VisitProgram(ProgramAsmNode program)
        {
            Visit(program.Function);
        }

        protected override void VisitRegister(RegisterAsmNode register)
        {
            indentedWriter.Write("%eax");
        }

        protected override void VisitRet(RetAsmNode ret)
        {
            indentedWriter.WriteLine("ret");
        }
    }
}