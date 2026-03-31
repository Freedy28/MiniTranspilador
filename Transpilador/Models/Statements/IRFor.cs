using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRFor : IRStatement
    {
        public IRStatement Initializer { get; set; } 
        public IRExpression Condition { get; set; }
        public IRExpression Incrementor { get; set; }
        public List<IRStatement> Body { get; set; }

        public IRFor(IRStatement initializer, IRExpression condition, IRExpression incrementor)
        {
            Initializer = initializer;
            Condition = condition;
            Incrementor = incrementor;
            Body = new List<IRStatement>();
        }
    }
}