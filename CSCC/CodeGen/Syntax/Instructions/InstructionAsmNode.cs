using CSCC.CodeGen.Syntax.Operands;

namespace CSCC.CodeGen.Syntax.Instructions;

abstract class InstructionAsmNode(OperandAsmNode[] operands) : AssemblyNode
{
    internal OperandAsmNode[] Operands { get; } = operands;
}