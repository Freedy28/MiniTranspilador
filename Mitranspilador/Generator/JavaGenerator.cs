using System;
using System.Text;
using MiniTranspilador.Models;

namespace MiniTranspilador.Generator
{
    public class JavaGenerator
    {
        private StringBuilder _sb;
        private int _indentLevel;

        public string GenerateJava(IRProgram program)
        {
            _sb = new StringBuilder();
            _indentLevel = 0;

            // Package (si hay namespace)
            if (!string.IsNullOrEmpty(program.Namespace))
            {
                WriteLine($"package {program.Namespace.ToLower()};");
                WriteLine();
            }

            // Clases
            foreach (var irClass in program.Classes)
            {
                GenerateClass(irClass);
            }

            return _sb.ToString();
        }

        private void GenerateClass(IRClass irClass)
        {
            WriteLine($"public class {irClass.Name} {{");
            Indent();

            foreach (var method in irClass.Methods)
            {
                GenerateMethod(method);
                WriteLine();
            }

            Unindent();
            WriteLine("}");
        }

        private void GenerateMethod(IRMethod method)
        {
            var returnType = MapTypeToJava(method.ReturnType);
            WriteLine($"public {returnType} {method.Name}() {{");
            Indent();

            // Declaraciones de variables
            foreach (var stmt in method.Statements)
            {
                GenerateVariableDeclaration(stmt);
            }

            // Asignaciones
            foreach (var assignment in method.Assignments)
            {
                GenerateAssignment(assignment);
            }

            // Return
            if (method.ReturnExpression != null)
            {
                Write("return ");
                GenerateExpression(method.ReturnExpression);
                WriteLine(";");
            }

            Unindent();
            WriteLine("}");
        }

        private void GenerateVariableDeclaration(IRVariableDeclaration decl)
        {
            var type = MapTypeToJava(decl.Type);
            Write($"{type} {decl.Name}");
            
            if (decl.InitialValue != null)
            {
                Write(" = ");
                GenerateExpression(decl.InitialValue);
            }
            
            WriteLine(";");
        }

        private void GenerateAssignment(IRAssignment assignment)
        {
            Write($"{assignment.VariableName} = ");
            GenerateExpression(assignment.Value);
            WriteLine(";");
        }

        private void GenerateExpression(IRExpression expression)
        {
            switch (expression)
            {
                case IRLiteral literal:
                    Write(literal.Value.ToString());
                    break;
                
                case IRVariable variable:
                    Write(variable.Name);
                    break;
                
                case IRBinaryOperation binary:
                    GenerateBinaryOperation(binary);
                    break;
            }
        }

        private void GenerateBinaryOperation(IRBinaryOperation binary)
        {
            Write("(");
            GenerateExpression(binary.Left);
            Write($" {binary.Operation.ToJavaSymbol()} ");
            GenerateExpression(binary.Right);
            Write(")");
        }

        private string MapTypeToJava(string csharpType)
        {
            return csharpType switch
            {
                "int" => "int",
                "double" => "double",
                "float" => "float",
                "long" => "long",
                "void" => "void",
                "string" => "String",
                "bool" => "boolean",
                _ => csharpType
            };
        }

        // Métodos de utilidad para formatear
        private void WriteLine(string text = "")
        {
            if (!string.IsNullOrEmpty(text))
            {
                Write(text);
            }
            _sb.AppendLine();
        }

        private void Write(string text)
        {
            if (_sb.Length == 0 || _sb[_sb.Length - 1] == '\n')
            {
                _sb.Append(new string(' ', _indentLevel * 4));
            }
            _sb.Append(text);
        }

        private void Indent() => _indentLevel++;
        private void Unindent() => _indentLevel = Math.Max(0, _indentLevel - 1);
    }
}