using CSCC.CodeGen.Syntax.Operands;

namespace CSCC.CodeGen.Syntax.Instructions;

class MovAsmNode(OperandAsmNode source, OperandAsmNode dest) : InstructionAsmNode([source, dest])
{
    public OperandAsmNode Source { get => Operands[0]; set => Operands[0] = value; }
    public OperandAsmNode Destination { get => Operands[1]; set => Operands[1] = value; }
}