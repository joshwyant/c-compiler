namespace CSCC.CodeGen.Syntax;

class ProgramAsmNode(FunctionDefinitionAsmNode func) : AssemblyNode
{
    public FunctionDefinitionAsmNode Function { get; } = func;
}