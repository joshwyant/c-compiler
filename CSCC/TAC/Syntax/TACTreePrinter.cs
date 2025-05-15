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
            writer.Write($"Constant({constant.Value})");
        }

        public override void VisitFunctionTACNode(FunctionDefinitionTACNode function)
        {
            writer.WriteLine("FunctionDefinition(");
            writer.Indent++;
            writer.WriteLine($"name=\"{function.Name}\",");
            writer.WriteLine("instructions=[");
            writer.Indent++;
            foreach (var instruction in function.Instructions)
            {
                Visit(instruction);
            }
            writer.Indent--;
            writer.WriteLine("]");
            writer.Indent--;
            writer.WriteLine("),");
        }

        public override void VisitProgramTACNode(ProgramTACNode program)
        {
            writer.WriteLine("Program(");
            writer.Indent++;
            writer.Write("function=");
            Visit(program.Function);
            writer.Indent--;
            writer.WriteLine(")");
        }

        public override void VisitReturnTACNode(ReturnInstructionTACNode instruction)
        {
            writer.Write("Return(");
            Visit(instruction.Value);
            writer.WriteLine("),");
        }

        public override void VisitUnaryTACNode(UnaryInstructionTACNode instruction)
        {
            writer.Write($"Unary(\"{TokenPrinter.Print(instruction.Operator)}\", ");
            Visit(instruction.Source);
            writer.Write(", ");
            Visit(instruction.Destination);
            writer.WriteLine("),");
        }

        public override void VisitVarTACNode(VarTACNode variable)
        {
            writer.Write($"Var(\"{variable.Name}\")");
        }
    }
}