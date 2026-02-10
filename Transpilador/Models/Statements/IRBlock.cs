using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents a block of statements (e.g., method body, if body, loop body).
    /// </summary>
    public class IRBlock : IRStatement
    {
        public List<IRStatement> Statements { get; set; }

        public IRBlock()
        {
            Statements = new List<IRStatement>();
        }

        public IRBlock(List<IRStatement> statements)
        {
            Statements = statements;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitBlock(this);
        }
    }
}
