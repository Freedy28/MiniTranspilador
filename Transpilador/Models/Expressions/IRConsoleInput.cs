using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRConsoleInput : IRExpression
    {
        public override string Type { get; }

        // "string" → scanner.nextLine()
        // "int"    → Integer.parseInt(scanner.nextLine())
        // "double" → Double.parseDouble(scanner.nextLine())
        // "float"  → Float.parseFloat(scanner.nextLine())
        // "long"   → Long.parseLong(scanner.nextLine())
        public IRConsoleInput(string type)
        {
            Type = type;
        }
    }
}
