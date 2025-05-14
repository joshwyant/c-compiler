namespace CSCC.Parsing.Syntax;

abstract class ASTNode
{
    public override string ToString() => TreePrinter.Print(this);
}