using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRLiteral : IRExpression
    {
        public object Value { get; set; }
        public override string Type { get; }

        public IRLiteral(object value, string type)
        {
            Value = value;
            Type = type;
        }
    }
}