using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRInterface
    {
        public string Name { get; set; }
        public IRAccessModifier AccessModifier { get; set; }
        public List<IRMethod> Methods { get; set; }

        public IRInterface(string name)
        {
            Name = name;
            AccessModifier = IRAccessModifier.Internal;
            Methods = new List<IRMethod>();
        }
    }
}
