namespace MiniTranspilador.Models
{
    public enum IROperationType
    {
        Add,        // +
        Subtract,   // -
        Multiply,   // *
        Divide      // /
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
                _ => "?"
            };
        }

        public static string ToJavaSymbol(this IROperationType operation)
        {
            // En este caso simple, los símbolos son iguales
            return operation.ToSymbol();
        }
    }
}