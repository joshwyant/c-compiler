using CSCC.CodeGen.Syntax.Instructions;

namespace CSCC.CodeGen.Syntax;

class FunctionDefinitionAsmNode(string name, List<InstructionAsmNode> instructions) : AssemblyNode
{
    public string Name { get; } = name;
    public List<InstructionAsmNode> Instructions { get; } = instructions;
}