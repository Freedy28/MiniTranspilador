using System.Collections.Generic;
using Transpilador.Models.Base;
using Transpilador.Models.Statements;

namespace Transpilador.Models.Structure
{
    public class IRMethod : IRNode
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<IRParameter> Parameters { get; set; }
        public IRBlock Body { get; set; }

        public IRMethod(string name, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = new List<IRParameter>();
            Body = new IRBlock();
        }

        public override T Accept<T>(IIRVisitor<T> visitor)
        {
            return visitor.VisitMethod(this);
        }
    }
}
