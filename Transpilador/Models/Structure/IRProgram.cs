using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRProgram : IRNode
    {
        public List<IRClass> Classes { get; set; }
        public string Namespace { get; set; }

        public IRProgram()
        {
            Classes = new List<IRClass>();
            Namespace = "";
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitProgram(this);
        }
    }
}