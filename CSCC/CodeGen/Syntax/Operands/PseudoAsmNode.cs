namespace CSCC.CodeGen.Syntax.Operands;

class PseudoAsmNode(string identifier) : OperandAsmNode
{
    public string Identifier { get; } = identifier;
}