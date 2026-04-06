using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRTypeCheck : IRExpression
    {
        public IRExpression Expression { get; set; }
        public string TargetType { get; set; }
        public override string Type => "bool";

        public IRTypeCheck(IRExpression expression, string targetType)
        {
            Expression = expression;
            TargetType = targetType;
        }
    }
}
