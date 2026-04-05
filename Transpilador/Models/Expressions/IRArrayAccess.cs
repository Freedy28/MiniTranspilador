using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRArrayAccess : IRExpression
    {
        public IRExpression ArrayExpression { get; set; }
        public IRExpression IndexExpression { get; set; }
        public override string Type { get; }

        public IRArrayAccess(IRExpression arrayExpression, IRExpression indexExpression, string type)
        {
            ArrayExpression = arrayExpression;
            IndexExpression = indexExpression;
            Type = type;
        }
    }
}
