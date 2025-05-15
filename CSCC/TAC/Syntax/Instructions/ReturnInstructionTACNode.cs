namespace CSCC.TAC.Syntax.Instructions;

class ReturnInstructionTACNode(ValueTACNode value) : InstructionTACNode
{
    public ValueTACNode Value { get; } = value;
}