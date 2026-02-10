using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    /// <summary>
    /// Represents a unary operation (e.g., -x, !flag, ++counter).
    /// </summary>
    public class IRUnaryOperation : IRExpression
    {
        public IRExpression Operand { get; set; }
        public IROperationType Operation { get; set; }
        public override string Type { get; }
        public bool IsPrefix { get; set; } // true for ++x, false for x++

        public IRUnaryOperation(IRExpression operand, IROperationType operation, string type, bool isPrefix = true)
        {
            Operand = operand;
            Operation = operation;
            Type = type;
            IsPrefix = isPrefix;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitUnaryOperation(this);
        }
    }
}
