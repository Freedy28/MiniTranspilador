using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    /// <summary>
    /// Represents a method parameter with name and type.
    /// </summary>
    public class IRParameter : IRNode
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public IRParameter(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitParameter(this);
        }
    }
}
