using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRClass
    {
        public string Name { get; set; }
        public IRAccessModifier AccessModifier { get; set; }
        public List<IRField> Fields { get; set; }
        public List<IRMethod> Methods { get; set; }

        public IRClass(string name)
        {
            Name = name;
            AccessModifier = IRAccessModifier.Internal;
            Fields = new List<IRField>();
            Methods = new List<IRMethod>();
        }
    }
}
