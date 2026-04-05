using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRArrayCreation : IRExpression
    {
        public string ElementType { get; set; }
        public IRExpression SizeExpression { get; set; }
        public List<IRExpression> InitialValues { get; set; }

        public override string Type => $"{ElementType}[]";

        public IRArrayCreation(string elementType, IRExpression sizeExpression = null, List<IRExpression> initialValues = null)
        {
            ElementType = elementType;
            SizeExpression = sizeExpression;
            InitialValues = initialValues ?? new List<IRExpression>();
        }
    }
}
