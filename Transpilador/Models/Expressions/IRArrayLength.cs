using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRArrayLength : IRExpression
    {
        public IRExpression ArrayExpression { get; set; }

        public override string Type => "int";

        public IRArrayLength(IRExpression arrayExpression)
        {
            ArrayExpression = arrayExpression;
        }
    }
}
