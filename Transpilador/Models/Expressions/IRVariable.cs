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

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitVariable(this);
        }
    }
}