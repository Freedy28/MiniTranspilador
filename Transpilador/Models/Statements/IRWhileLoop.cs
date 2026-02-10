using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents a while loop with condition and body.
    /// </summary>
    public class IRWhileLoop : IRStatement
    {
        public IRExpression Condition { get; set; }
        public IRStatement Body { get; set; }

        public IRWhileLoop(IRExpression condition, IRStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitWhileLoop(this);
        }
    }
}
