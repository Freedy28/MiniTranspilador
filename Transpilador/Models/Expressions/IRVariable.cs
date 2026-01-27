using Transpilador.Models.Base;

namespace Transpilador.Models.Expressions
{
    public class IRVariable : IRExpression
    {
        public string Name { get; set; }
        public override string Type { get; }

        public IRVariable(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}