using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents a return statement with an optional expression.
    /// </summary>
    public class IRReturnStatement : IRStatement
    {
        public IRExpression? Expression { get; set; }

        public IRReturnStatement(IRExpression? expression = null)
        {
            Expression = expression;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }
}
