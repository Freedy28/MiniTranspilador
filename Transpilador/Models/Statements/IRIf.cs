using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;


public class IRIf : IRStatement
{
    public IRExpression Condition { get; set; }
    public List<IRStatement> ThenBranch { get; set; }
    public List<IRStatement> ElseBranch { get; set; }


    public IRIf(IRExpression condition)
    {
        Condition = condition;
        ThenBranch = new List<IRStatement>();
        ElseBranch = new List<IRStatement>();
    }

}




