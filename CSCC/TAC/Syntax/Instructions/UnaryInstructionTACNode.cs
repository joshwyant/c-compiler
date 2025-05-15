using CSCC.Lexing;

namespace CSCC.TAC.Syntax.Instructions;

class UnaryInstructionTACNode(TokenType @operator, ValueTACNode source, VarTACNode destination) : InstructionTACNode
{
    public TokenType Operator { get; } = @operator;
    public ValueTACNode Source { get; } = source;
    public VarTACNode Destination { get; } = destination;
}