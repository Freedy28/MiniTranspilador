using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRSwitch : IRStatement
    {
        public IRExpression Expression { get; set; }
        public List<IRSwitchCase> Cases { get; set; }

        public IRSwitch(IRExpression expression)
        {
            Expression = expression;
            Cases = new List<IRSwitchCase>();
        }
    }

    public class IRSwitchCase
    {
        public bool IsDefault { get; set; }
        public IRExpression Label { get; set; }
        public List<IRStatement> Body { get; set; }

        public IRSwitchCase(IRExpression label, bool isDefault = false)
        {
            IsDefault = isDefault;
            Label = label;
            Body = new List<IRStatement>();
        }
    }
}