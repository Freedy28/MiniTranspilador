using System.Collections.Generic;

namespace MiniTranspilador.Models
{
    // Representación intermedia para expresiones
    public abstract class IRExpression
    {
        public abstract string Type { get; }
    }

    // Literales numéricos (5, 10, etc.)
    public class IRLiteral : IRExpression
    {
        public object Value { get; set; }
        public override string Type { get; }

        public IRLiteral(object value, string type)
        {
            Value = value;
            Type = type;
        }
    }

    // Variables (a, b, suma, etc.)
    public class IRVariable : IRExpression
    {
        public string Name { get; set; }
        public override string Type { get; }

        public IRVariable(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    // Operaciones binarias (a + b, x * y, etc.)
    public class IRBinaryOperation : IRExpression
    {
        public IRExpression Left { get; set; }
        public IRExpression Right { get; set; }
        public IROperationType Operation { get; set; }
        public override string Type { get; }

        public IRBinaryOperation(IRExpression left, IRExpression right, IROperationType operation, string type)
        {
            Left = left;
            Right = right;
            Operation = operation;
            Type = type;
        }
    }

    // Declaración de variable (int a = 5;)
    public class IRVariableDeclaration
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public IRExpression? InitialValue { get; set; }

        public IRVariableDeclaration(string name, string type, IRExpression? initialValue = null)
        {
            Name = name;
            Type = type;
            InitialValue = initialValue;
        }
    }

    // Asignación (a = b + c;)
    public class IRAssignment
    {
        public string VariableName { get; set; }
        public IRExpression Value { get; set; }

        public IRAssignment(string variableName, IRExpression value)
        {
            VariableName = variableName;
            Value = value;
        }
    }

    // Método
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

    // Clase
    public class IRClass
    {
        public string Name { get; set; }
        public List<IRMethod> Methods { get; set; }

        public IRClass(string name)
        {
            Name = name;
            Methods = new List<IRMethod>();
        }
    }

    // Programa completo
    public class IRProgram
    {
        public List<IRClass> Classes { get; set; }
        public string Namespace { get; set; }

        public IRProgram()
        {
            Classes = new List<IRClass>();
            Namespace = "";
        }
    }
}