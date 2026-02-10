using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents an if/else statement with condition and optional else clause.
    /// </summary>
    public class IRIfStatement : IRStatement
    {
        public IRExpression Condition { get; set; }
        public IRStatement ThenBranch { get; set; }
        public IRStatement? ElseBranch { get; set; }

        public IRIfStatement(IRExpression condition, IRStatement thenBranch, IRStatement? elseBranch = null)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }
}
