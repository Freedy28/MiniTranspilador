using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRCastExpression : IRExpression
    {
        public string TargetType { get; set; }
        public IRExpression Expression { get; set; }
        public override string Type => TargetType;

        public IRCastExpression(string targetType, IRExpression expression)
        {
            TargetType = targetType;
            Expression = expression;
        }
    }
}
