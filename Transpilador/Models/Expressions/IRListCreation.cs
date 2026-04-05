using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRListCreation : IRExpression
    {
        public string ElementType { get; set; }
        public override string Type => $"List<{ElementType}>";

        public IRListCreation(string elementType)
        {
            ElementType = elementType;
        }
    }
}
