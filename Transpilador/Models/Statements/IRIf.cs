using Transpilador.Models.Base;

namespace Transpilador.Models.Statements
{
    public class IRIf : IRStatement
    {
        public IRExpression Condition { get; set; }
        public List<IRStatement> ThenBranch { get; set; }
        public List<IRStatement> ElseBranch { get; set; }

        public IRIf(IRExpression condition)
        {
            Condition = condition;
            ThenBranch = [];
            ElseBranch = [];
        }
    }
}







