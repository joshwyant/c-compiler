namespace CSCC.CodeGen.Syntax;

abstract class AssemblyNode
{
    public override string ToString() => AsmTreePrinter.Print(this);
}