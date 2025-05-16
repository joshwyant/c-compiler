namespace CSCC.CodeGen.Syntax.Instructions;

class AllocateStackAsmNode(int frameSize) : InstructionAsmNode([])
{
    public int StackFrameSize { get; } = frameSize;
}