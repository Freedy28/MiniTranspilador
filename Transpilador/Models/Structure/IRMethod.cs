using System.Collections.Generic;
using Transpilador.Models.Base;
using Transpilador.Models.Statements;

namespace Transpilador.Models.Structure
{
    public class IRMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<IRVariableDeclaration> Statements { get; set; }
        public List<IRAssignment> Assignments { get; set; }
        public IRExpression? ReturnExpression { get; set; }

        public IRMethod(string name, string returnType)
        {
            Name = name;
            ReturnType = returnType;
            Statements = new List<IRVariableDeclaration>();
            Assignments = new List<IRAssignment>();
        }
    }
}