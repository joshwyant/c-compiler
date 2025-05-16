namespace CSCC.CodeGen.Syntax.Operands;

class RegisterAsmNode(AssemblyRegister register) : OperandAsmNode
{
    public AssemblyRegister Register { get; } = register;
}