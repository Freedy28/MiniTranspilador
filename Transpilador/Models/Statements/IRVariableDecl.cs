
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;

namespace Transpilador.Models.Statements
{
    public class IRVariableDeclaration : IRStatement
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IRExpression? InitialValue { get; set; }

        public IRVariableDeclaration(string name, string type, IRExpression? initialValue = null)
        {
            Name = name;
            Type = type;
            InitialValue = initialValue;
        }
    }
}