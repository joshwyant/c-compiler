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

        protected override async Task VisitAllocateStackAsync(AllocateStackAsmNode alloc, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync($"subq    ${alloc.StackFrameSize}, %rsp".AsMemory(), cancellationToken);
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

            await indentedWriter.WriteLineAsync("pushq   %rbp".AsMemory(), cancellationToken);
            await indentedWriter.WriteLineAsync("movq    %rsp, %rbp".AsMemory(), cancellationToken);
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
            await indentedWriter.WriteAsync($"movl    ".AsMemory(), cancellationToken);
            await VisitAsync(mov.Source, cancellationToken);
            await indentedWriter.WriteAsync(", ");
            await VisitAsync(mov.Destination, cancellationToken);
            await indentedWriter.WriteLineAsync();
        }

        protected override async Task VisitProgramAsync(ProgramAsmNode program, CancellationToken cancellationToken = default)
        {
            await VisitAsync(program.Function, cancellationToken);
        }

        protected override Task VisitPseudoOperandAsync(PseudoAsmNode ps, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("No pseudo operands in final assembly");
        }

        protected override async Task VisitRegisterAsync(RegisterAsmNode register, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync(
                (register.Register switch
                {
                    AssemblyRegister.AX => "%eax",
                    AssemblyRegister.R10 => "%r10d",
                    _ => throw new NotImplementedException($"No register mapping defined for register {register.Register}")
                }).AsMemory(), cancellationToken);
        }

        protected override async Task VisitRetAsync(RetAsmNode ret, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteLineAsync("movq    %rbp, %rsp".AsMemory(), cancellationToken);
            await indentedWriter.WriteLineAsync("popq    %rbp".AsMemory(), cancellationToken);
            await indentedWriter.WriteLineAsync("ret".AsMemory(), cancellationToken);
        }

        protected override async Task VisitStackOperandAsync(StackAsmNode stack, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync($"{stack.Position}(%rbp)".AsMemory(), cancellationToken);
        }

        protected override async Task VisitUnaryAsync(UnaryAsmNode unary, CancellationToken cancellationToken = default)
        {
            await indentedWriter.WriteAsync($"{(unary.Operator switch
            {
                AssemblyOperator.Neg => "negl",
                AssemblyOperator.Not => "notl",
                _ => throw new NotImplementedException($"No instruction mnemonic defined for operator {unary.Operator}")
            }).PadRight(8)}".AsMemory(), cancellationToken);
            await VisitAsync(unary.Operand, cancellationToken);
            await indentedWriter.WriteLineAsync();
        }
    }
}