
using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRBinaryOperation : IRExpression
    {
        public IRExpression Left { get; set; }
        public IRExpression Right { get; set; }
        public IROperationType Operation { get; set; }
        public override string Type { get; }

        public IRBinaryOperation(IRExpression left, IRExpression right, IROperationType operation, string type)
        {
            Left = left;
            Right = right;
            Operation = operation;
            Type = type;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitBinaryOperation(this);
        }
    }
}