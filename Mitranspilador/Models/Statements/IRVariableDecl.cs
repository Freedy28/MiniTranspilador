
using MiniTranspilador.Models.Base;
using MiniTranspilador.Models.Expression;

namespace MiniTranspilador.Models.IR.Statements
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