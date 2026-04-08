using System.Collections.Generic;

namespace Transpilador.Models.Structure
{
    public class IRProgram
    {
        public List<IRInterface> Interfaces { get; set; }
        public List<IRClass> Classes { get; set; }
        public string Namespace { get; set; }
        public IRProgram()
        {
            Interfaces = new List<IRInterface>();
            Classes = new List<IRClass>();
            Namespace = "";
        }
    }
}