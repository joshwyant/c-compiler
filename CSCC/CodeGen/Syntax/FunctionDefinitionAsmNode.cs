using CSCC.CodeGen.Syntax.Instructions;

namespace CSCC.CodeGen.Syntax;

class FunctionDefinitionAsmNode(string name, InstructionAsmNode[] instructions) : AssemblyNode
{
    public string Name { get; } = name;
    public InstructionAsmNode[] Instructions { get; } = instructions;
}