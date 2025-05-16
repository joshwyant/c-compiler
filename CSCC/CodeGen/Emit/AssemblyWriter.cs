using System.CodeDom.Compiler;
using CSCC.CodeGen.Syntax;
using CSCC.CodeGen.Syntax.Instructions;
using CSCC.CodeGen.Syntax.Operands;

namespace CSCC.CodeGen.Emit;

class AssemblyWriter(ProgramAsmNode program)
{
    readonly ProgramAsmNode program = program;

    public async Task WriteAsync(string filename, CancellationToken token = default)
    {
        using var visitor = new AsmWriterVisitor(filename, program);
        await visitor.WriteAsync(token);
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

        public async Task WriteAsync(CancellationToken cancellationToken = default)
        {
            await VisitAsync(program, cancellationToken);
            if (OperatingSystem.IsLinux())
            {
                await indentedWriter.WriteLineAsync();
                await indentedWriter.WriteLineAsync(".section .note.GNU-stack,\"\",@progbits".AsMemory(), cancellationToken);
            }
        }

        protected override Task VisitAllocateStackAsync(AllocateStackAsmNode alloc, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task VisitFunctionDefinitionAsync(FunctionDefinitionAsmNode func, CancellationToken cancellationToken = default)
        {
            var funcName = OperatingSystem.IsMacOS() ? $"_{func.Name}" : func.Name;

            await indentedWriter.WriteLineAsync($"# Function {func.Name}".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            await indentedWriter.WriteLineAsync($".globl {funcName}".AsMemory(), cancellationToken);
            indentedWriter.Indent--;
            await indentedWriter.WriteLineAsync($"{funcName}:".AsMemory(), cancellationToken);
            indentedWriter.Indent++;
            foreach (var instruction in func.Instructions)
            {
                await VisitAsync(instruction, cancellationToken);
            }
            indentedWriter.Indent--;
        }

        protected override async Task VisitImmediateAsync(ImmediateAsmNode imm, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync($"${imm.Value}".AsMemory(), cancellationToken);
        }

        protected override async Task VisitMovAsync(MovAsmNode mov, CancellationToken cancellationToken = default)
        {
            indentedWriter.Write($"movl ".AsMemory());
            await VisitAsync(mov.Source, cancellationToken);
            indentedWriter.Write(", ".AsMemory());
            await VisitAsync(mov.Destination, cancellationToken);
            indentedWriter.WriteLine();
        }

        protected override async Task VisitProgramAsync(ProgramAsmNode program, CancellationToken cancellationToken = default)
        {
            await VisitAsync(program.Function, cancellationToken);
        }

        protected override Task VisitPseudoOperandAsync(PseudoAsmNode ps, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task VisitRegisterAsync(RegisterAsmNode register, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync("%eax".AsMemory(), cancellationToken);
        }

        protected override async Task VisitRetAsync(RetAsmNode ret, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("ret".AsMemory(), cancellationToken);
        }

        protected override Task VisitStackOperandAsync(StackAsmNode stack, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task VisitUnaryAsync(UnaryAsmNode unary, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}