using System;
using System.Text;
using Transpilador.Generator.Base;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Generator
{

    public class JavaGenerator : IRWalker
    {
        private StringBuilder _sb = new StringBuilder();
        private int _indentLevel;

        public string GenerateJava(IRProgram program)
        {
            _sb.Clear();
            _indentLevel = 0;

            if (!string.IsNullOrEmpty(program.Namespace))
            {
                WriteLine($"package {program.Namespace.ToLower()};");
                WriteLine();
            }

            VisitProgram(program);

            return _sb.ToString();
        }

        public override void VisitClass(IRClass irClass)
        {

            WriteLine($"public class {irClass.Name} {{");
            Indent();

            base.VisitClass(irClass);

            Unindent();
            WriteLine("}");
            WriteLine();
        }

        public override void VisitMethod(IRMethod method)
        {
            var returnType = MapTypeToJava(method.ReturnType);
            WriteLine($"public {returnType} {method.Name}() {{");
            Indent();

            base.VisitMethod(method);

            if (method.ReturnExpression != null)
            {
                Write("return ");
                GenerateExpression(method.ReturnExpression); 
                WriteLine(";");
            }
            
            Unindent();
            WriteLine("}"); 
        }

        protected override void VisitVariableDeclaration(IRVariableDeclaration decl)
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

        protected override void VisitAssignment(IRAssignment assignment)
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

        private void WriteLine(string text = "")
        {
            if (!string.IsNullOrEmpty(text)) Write(text);
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