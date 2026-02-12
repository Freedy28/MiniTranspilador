using System.Collections.Generic;
using Transpilador.Models.Base;

namespace Transpilador.Models.Structure
{
    public class IRMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<IRStatement> Body { get; set; }
        public IRExpression? ReturnExpression { get; set; }
        public IRMethod(string name, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Body = new List<IRStatement>();            
        }
    }
}