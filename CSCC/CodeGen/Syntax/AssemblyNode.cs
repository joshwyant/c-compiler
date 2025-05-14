namespace CSCC.CodeGen.Syntax;

abstract class AssemblyNode
{
    public override string ToString() => ToStringAsync().Result;
    public async Task<string> ToStringAsync(CancellationToken cancellationToken = default) => await AsmTreePrinter.PrintAsync(this, cancellationToken);
}