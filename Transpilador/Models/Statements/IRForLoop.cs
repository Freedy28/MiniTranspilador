using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    /// <summary>
    /// Represents a for loop with initializers, condition, incrementors, and body.
    /// </summary>
    public class IRForLoop : IRStatement
    {
        public List<IRStatement> Initializers { get; set; }
        public IRExpression? Condition { get; set; }
        public List<IRExpression> Incrementors { get; set; }
        public IRStatement Body { get; set; }

        public IRForLoop(List<IRStatement> initializers, IRExpression? condition, List<IRExpression> incrementors, IRStatement body)
        {
            Initializers = initializers;
            Condition = condition;
            Incrementors = incrementors;
            Body = body;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitForLoop(this);
        }
    }
}
