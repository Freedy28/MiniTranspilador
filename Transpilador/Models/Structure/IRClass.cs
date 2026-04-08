using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRClass
    {
        public string Name { get; set; }
        public IRAccessModifier AccessModifier { get; set; }
        public bool IsAbstract { get; set; }
        public string BaseClass { get; set; }
        public List<string> Interfaces { get; set; }
        public List<IRField> Fields { get; set; }
        public List<IRMethod> Methods { get; set; }

        public IRClass(string name)
        {
            Name = name;
            AccessModifier = IRAccessModifier.Internal;
            IsAbstract = false;
            BaseClass = null;
            Interfaces = [];
            Fields = [];
            Methods = [];
        }
    }
}
