using System.CodeDom.Compiler;
using System.Text;
using CSCC.Lexing;
using CSCC.TAC.Syntax.Instructions;

namespace CSCC.TAC.Syntax;

class TACTreePrinter
{
    TACNode node;
    TreePrinterVisitor visitor;
    TACTreePrinter(TACNode node)
    {
        this.node = node;
        visitor = new();
    }

    public static string Print(TACNode node) =>
        new TACTreePrinter(node).Print();
    string Print()
    {
        return visitor.Print(node);
    }

    class TreePrinterVisitor : TACNodeVisitor
    {
        StringBuilder sb;
        IndentedTextWriter writer;
        public TreePrinterVisitor()
        {
            sb = new();
            writer = new(new StringWriter(sb), "    ");
        }
        public string Print(TACNode node)
        {
            Visit(node);
            var result = sb.ToString();
            sb.Clear(); // For next time
            return result;
        }
        public override void VisitConstantTACNode(ConstantTACNode constant)
        {
            writer.WriteLine($"Constant({constant.Value}),".AsMemory());
        }

        public override void VisitFunctionTACNode(FunctionDefinitionTACNode function)
        {
            writer.WriteLine("FunctionDefinition(".AsMemory());
            writer.Indent++;
            writer.WriteLine($"name=\"{function.Name}\",");
            writer.WriteLine("instructions=[".AsMemory());
            writer.Indent++;
            foreach (var instruction in function.Instructions)
            {
                Visit(instruction);
            }
            writer.Indent--;
            writer.WriteLine("]".AsMemory());
            writer.Indent--;
            writer.WriteLine("),".AsMemory());
        }

        public override void VisitProgramTACNode(ProgramTACNode program)
        {
            writer.WriteLine("Program(".AsMemory());
            writer.Indent++;
            writer.Write("function=".AsMemory());
            Visit(program.Function);
            writer.Indent--;
            writer.WriteLine(")".AsMemory());
        }

        public override void VisitReturnTACNode(ReturnInstructionTACNode instruction)
        {
            writer.WriteLine("Return(".AsMemory());
            writer.Indent++;
            writer.Write("value=".AsMemory());
            Visit(instruction.Value);
            writer.Indent--;
            writer.WriteLine("),".AsMemory());
        }

        public override void VisitUnaryTACNode(UnaryInstructionTACNode instruction)
        {
            writer.WriteLine("Unary(".AsMemory());
            writer.Indent++;
            writer.WriteLine($"operator=\"{TokenPrinter.Print(instruction.Operator)}\"".AsMemory());
            writer.Write("source=".AsMemory());
            Visit(instruction.Source);
            writer.Write("destination=".AsMemory());
            Visit(instruction.Destination);
            writer.Indent--;
            writer.WriteLine("),".AsMemory());
        }

        public override void VisitVarTACNode(VarTACNode variable)
        {
            writer.WriteLine("Var(".AsMemory());
            writer.Indent++;
            writer.WriteLine($"name=\"{variable.Name}\"".AsMemory());
            writer.Indent--;
            writer.WriteLine("),".AsMemory());
        }
    }
}