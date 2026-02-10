using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    /// <summary>
    /// Represents a method call with optional target object and arguments.
    /// </summary>
    public class IRMethodCall : IRExpression
    {
        public IRExpression? Target { get; set; } // null for static calls or calls in the same class
        public string MethodName { get; set; }
        public List<IRExpression> Arguments { get; set; }
        public override string Type { get; }

        public IRMethodCall(string methodName, List<IRExpression> arguments, string type, IRExpression? target = null)
        {
            MethodName = methodName;
            Arguments = arguments;
            Type = type;
            Target = target;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitMethodCall(this);
        }
    }
}
