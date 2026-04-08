using Transpilador.Models.Base;

namespace Transpilador.Models.Statements;

public class IRReturn : IRStatement
{
    public IRExpression? Expression { get; set; }

    public IRReturn(IRExpression? expression = null)
    {
        Expression = expression;
    }
}
