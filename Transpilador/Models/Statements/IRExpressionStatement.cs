using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents an expression used as a statement (e.g., method call, increment).
    /// </summary>
    public class IRExpressionStatement : IRStatement
    {
        public IRExpression Expression { get; set; }

        public IRExpressionStatement(IRExpression expression)
        {
            Expression = expression;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}
