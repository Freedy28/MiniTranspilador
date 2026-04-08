using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRObjectCreation : IRExpression
    {
        public string ClassName { get; set; }
        public List<IRExpression> Arguments { get; set; }
        public override string Type => ClassName;

        public IRObjectCreation(string className, List<IRExpression> arguments)
        {
            ClassName = className;
            Arguments = arguments ?? [];
        }
    }
}
