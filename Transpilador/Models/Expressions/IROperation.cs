namespace Transpilador.Models
{
    public enum IROperationType
    {
        // Aritméticos
        Add,        // +
        Subtract,   // -
        Multiply,   // *
        Divide,     // /
        Modulo,     // %

        // Comparación
        Equal,              // ==
        NotEqual,           // !=
        LessThan,           // <
        GreaterThan,        // >
        LessThanOrEqual,    // <=
        GreaterThanOrEqual, // >=

        // Lógicos binarios
        LogicalAnd,  // &&
        LogicalOr,   // ||
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
                IROperationType.Equal => "==",
                IROperationType.NotEqual => "!=",
                IROperationType.LessThan => "<",
                IROperationType.GreaterThan => ">",
                IROperationType.LessThanOrEqual => "<=",
                IROperationType.GreaterThanOrEqual => ">=",
                IROperationType.LogicalAnd => "&&",
                IROperationType.LogicalOr => "||",
                _ => "?"
            };
        }

        public static string ToJavaSymbol(this IROperationType operation)
        {
            // En Java los símbolos son iguales que en C#
            return operation.ToSymbol();
        }
    }
}