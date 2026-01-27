using Transpilador.Models.Base;
using Transpilador.Models.Expressions;

namespace Transpilador.Models.Statements
{
public class IRAssignment
    {
        public string VariableName { get; set; }
        public IRExpression Value { get; set; }

        public IRAssignment(string variableName, IRExpression value)
        {
            VariableName = variableName;
            Value = value;
        }
    }
}