using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRClass : IRNode
    {
        public string Name { get; set; }
        public List<IRMethod> Methods { get; set; }

        public IRClass(string name)
        {
            Name = name;
            Methods = new List<IRMethod>();
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitClass(this);
        }
    }
}