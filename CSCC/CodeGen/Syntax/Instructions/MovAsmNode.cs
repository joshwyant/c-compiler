namespace CSCC.CodeGen.Syntax.Instructions;

class MovAsmNode(OperandAsmNode source, OperandAsmNode dest) : InstructionAsmNode
{
    public OperandAsmNode Source { get; } = source;
    public OperandAsmNode Destination { get; } = dest;
}