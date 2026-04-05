using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRDoWhile : IRStatement
    {
        public IRExpression Condition { get; set; }
        public List<IRStatement> Body { get; set; }

        public IRDoWhile(IRExpression condition)
        {
            Condition = condition;
            Body = new List<IRStatement>();
        }
    }
}