using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRWhile : IRStatement
    {
        public IRExpression Condition { get; set; }
        public List<IRStatement> Body { get; set; }

        public IRWhile(IRExpression condition)
        {
            Condition = condition;
            Body = new List<IRStatement>();
        }
    }
}