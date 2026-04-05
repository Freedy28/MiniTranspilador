using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRMethodCall : IRExpression
    {
        public string MethodName { get; set; }
        public List<IRExpression> Arguments { get; set; }
        public override string Type { get; }

        public IRMethodCall(string methodName, List<IRExpression> arguments, string type)
        {
            MethodName = methodName;
            Arguments = arguments;
            Type = type;
        }
    }
}
