namespace CSCC.TAC.Syntax;

class TACNode
{
    public override string ToString() => TACTreePrinter.Print(this);
}