using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public IRAccessModifier AccessModifier { get; set; }
        public bool IsStatic { get; set; }
        public bool IsEntryPoint { get; set; }
        public List<IRParameter> Parameters { get; set; }
        public List<IRStatement> Body { get; set; }
        public IRExpression ReturnExpression { get; set; }
        public IRMethod(string name, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            AccessModifier = IRAccessModifier.Private;
            IsStatic = false;
            IsEntryPoint = false;
            Parameters = new List<IRParameter>();
            Body = new List<IRStatement>();            
            ReturnExpression = null;
        }
    }
}