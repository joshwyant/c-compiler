namespace CSCC.Parsing.Syntax;

abstract class ASTNode
{
    public override string ToString() => ToStringAsync().Result;
    public async Task<string> ToStringAsync(CancellationToken cancellationToken = default) => await TreePrinter.PrintAsync(this, cancellationToken);
}