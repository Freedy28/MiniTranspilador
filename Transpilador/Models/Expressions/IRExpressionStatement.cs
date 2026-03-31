using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRExpressionStatement : IRStatement
    {
        public IRExpression Expression { get; set; }

        public IRExpressionStatement(IRExpression expression)
        {
            Expression = expression;
        }
    }
}