using CSCC.TAC.Syntax.Instructions;

namespace CSCC.TAC.Syntax;

class FunctionDefinitionTACNode(string name, InstructionTACNode[] instructions) : TACNode
{
    public string Name { get; } = name;
    public InstructionTACNode[] Instructions { get; } = instructions;
}