namespace Transpilador.Models
{
    public enum IROperationType
    {
        // Arithmetic operations
        Add,        // +
        Subtract,   // -
        Multiply,   // *
        Divide,     // /
        Modulo,     // %
        
        // Comparison operations
        Equals,              // ==
        NotEquals,           // !=
        GreaterThan,         // >
        LessThan,            // <
        GreaterThanOrEqual,  // >=
        LessThanOrEqual,     // <=
        
        // Logical operations
        LogicalAnd,  // &&
        LogicalOr,   // ||
        
        // Unary operations
        UnaryPlus,       // +x
        UnaryMinus,      // -x
        LogicalNot,      // !x
        PreIncrement,    // ++x
        PostIncrement,   // x++
        PreDecrement,    // --x
        PostDecrement    // x--
    }

    public static class IROperationExtensions
    {
        public static string ToSymbol(this IROperationType operation)
        {
            return operation switch
            {
                IROperationType.Add => "+",
                IROperationType.Subtract => "-",
                IROperationType.Multiply => "*",
                IROperationType.Divide => "/",
                IROperationType.Modulo => "%",
                IROperationType.Equals => "==",
                IROperationType.NotEquals => "!=",
                IROperationType.GreaterThan => ">",
                IROperationType.LessThan => "<",
                IROperationType.GreaterThanOrEqual => ">=",
                IROperationType.LessThanOrEqual => "<=",
                IROperationType.LogicalAnd => "&&",
                IROperationType.LogicalOr => "||",
                IROperationType.UnaryPlus => "+",
                IROperationType.UnaryMinus => "-",
                IROperationType.LogicalNot => "!",
                IROperationType.PreIncrement => "++",
                IROperationType.PostIncrement => "++",
                IROperationType.PreDecrement => "--",
                IROperationType.PostDecrement => "--",
                _ => "?"
            };
        }

        public static string ToJavaSymbol(this IROperationType operation)
        {
            // In this simple case, the symbols are the same for Java
            return operation.ToSymbol();
        }
    }
}