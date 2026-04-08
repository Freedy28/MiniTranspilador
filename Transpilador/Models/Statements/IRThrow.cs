using Transpilador.Models.Base;

namespace Transpilador.Models.Statements;

public class IRThrow : IRStatement
{
    public IRExpression? Expression { get; set; }

    public IRThrow(IRExpression? expression = null)
    {
        Expression = expression;
    }
}
