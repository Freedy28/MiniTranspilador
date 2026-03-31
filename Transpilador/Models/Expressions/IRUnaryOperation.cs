using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public enum IRUnaryOperationType
    {
        Negate,     // -  (negación aritmética)
        LogicalNot,  // !  (negación lógica)
        Increment,   // ++ (incremento)
        Decrement    // -- (decremento)
    }

    public class IRUnaryOperation : IRExpression
    {
        public IRExpression Operand { get; set; }
        public IRUnaryOperationType Operation { get; set; }
        public override string Type { get; }

        public IRUnaryOperation(IRExpression operand, IRUnaryOperationType operation, string type)
        {
            Operand = operand;
            Operation = operation;
            Type = type;
        }

        public string ToSymbol()
        {
            return Operation switch
            {
                IRUnaryOperationType.Negate => "-",
                IRUnaryOperationType.LogicalNot => "!",
                IRUnaryOperationType.Increment => "++",
                IRUnaryOperationType.Decrement => "--",
                _ => "?"
            };
        }
    }
}