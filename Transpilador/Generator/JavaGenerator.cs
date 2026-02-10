using System;
using System.Linq;
using System.Text;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Generator
{
    /// <summary>
    /// Generates Java code from IR using the Visitor pattern.
    /// Implements IIRVisitor to traverse and transform IR nodes into Java code.
    /// </summary>
    public class JavaGenerator : IIRVisitor<string>
    {
        private StringBuilder _sb = new StringBuilder();
        private int _indentLevel;

        /// <summary>
        /// Main entry point: Generates Java code from an IR program.
        /// </summary>
        public string Generate(IRProgram program)
        {
            _sb = new StringBuilder();
            _indentLevel = 0;
            return program.Accept(this);
        }

        #region Structure Visitors

        public string VisitProgram(IRProgram program)
        {
            // Package (if namespace exists)
            if (!string.IsNullOrEmpty(program.Namespace))
            {
                WriteLine($"package {program.Namespace.ToLower()};");
                WriteLine();
            }

            // Generate classes
            foreach (var irClass in program.Classes)
            {
                irClass.Accept(this);
            }

            return _sb.ToString();
        }

        public string VisitClass(IRClass irClass)
        {
            WriteLine($"public class {irClass.Name} {{");
            Indent();

            foreach (var method in irClass.Methods)
            {
                method.Accept(this);
                WriteLine();
            }

            Unindent();
            WriteLine("}");

            return "";
        }

        public string VisitMethod(IRMethod method)
        {
            var returnType = MapTypeToJava(method.ReturnType);
            
            // Build parameter list
            var parameters = string.Join(", ", method.Parameters
                .Select(p => $"{MapTypeToJava(p.Type)} {p.Name}"));

            WriteLine($"public {returnType} {method.Name}({parameters}) {{");
            Indent();

            // Visit method body
            method.Body.Accept(this);

            Unindent();
            WriteLine("}");

            return "";
        }

        public string VisitParameter(IRParameter parameter)
        {
            return $"{MapTypeToJava(parameter.Type)} {parameter.Name}";
        }

        #endregion

        #region Statement Visitors

        public string VisitBlock(IRBlock block)
        {
            foreach (var statement in block.Statements)
            {
                statement.Accept(this);
            }
            return "";
        }

        public string VisitVariableDeclaration(IRVariableDeclaration declaration)
        {
            var type = MapTypeToJava(declaration.Type);
            Write($"{type} {declaration.Name}");
            
            if (declaration.InitialValue != null)
            {
                Write(" = ");
                Write(declaration.InitialValue.Accept(this));
            }
            
            WriteLine(";");
            return "";
        }

        public string VisitAssignment(IRAssignment assignment)
        {
            Write($"{assignment.VariableName} = ");
            Write(assignment.Value.Accept(this));
            WriteLine(";");
            return "";
        }

        public string VisitReturnStatement(IRReturnStatement returnStatement)
        {
            Write("return");
            if (returnStatement.Expression != null)
            {
                Write(" ");
                Write(returnStatement.Expression.Accept(this));
            }
            WriteLine(";");
            return "";
        }

        public string VisitExpressionStatement(IRExpressionStatement expressionStatement)
        {
            Write(expressionStatement.Expression.Accept(this));
            WriteLine(";");
            return "";
        }

        public string VisitIfStatement(IRIfStatement ifStatement)
        {
            Write("if (");
            Write(ifStatement.Condition.Accept(this));
            Write(") ");

            // Handle block vs single statement
            if (ifStatement.ThenBranch is IRBlock)
            {
                WriteLine("{");
                Indent();
                ifStatement.ThenBranch.Accept(this);
                Unindent();
                Write("}");
            }
            else
            {
                WriteLine();
                Indent();
                ifStatement.ThenBranch.Accept(this);
                Unindent();
            }

            // Handle else clause
            if (ifStatement.ElseBranch != null)
            {
                WriteLine();
                Write("else ");

                if (ifStatement.ElseBranch is IRBlock)
                {
                    WriteLine("{");
                    Indent();
                    ifStatement.ElseBranch.Accept(this);
                    Unindent();
                    WriteLine("}");
                }
                else if (ifStatement.ElseBranch is IRIfStatement)
                {
                    // else if case
                    ifStatement.ElseBranch.Accept(this);
                }
                else
                {
                    WriteLine();
                    Indent();
                    ifStatement.ElseBranch.Accept(this);
                    Unindent();
                }
            }
            else
            {
                WriteLine();
            }

            return "";
        }

        public string VisitWhileLoop(IRWhileLoop whileLoop)
        {
            Write("while (");
            Write(whileLoop.Condition.Accept(this));
            Write(") ");

            if (whileLoop.Body is IRBlock)
            {
                WriteLine("{");
                Indent();
                whileLoop.Body.Accept(this);
                Unindent();
                WriteLine("}");
            }
            else
            {
                WriteLine();
                Indent();
                whileLoop.Body.Accept(this);
                Unindent();
            }

            return "";
        }

        public string VisitForLoop(IRForLoop forLoop)
        {
            Write("for (");

            // Initializers
            for (int i = 0; i < forLoop.Initializers.Count; i++)
            {
                var init = forLoop.Initializers[i];
                if (init is IRVariableDeclaration varDecl)
                {
                    var type = MapTypeToJava(varDecl.Type);
                    Write($"{type} {varDecl.Name}");
                    if (varDecl.InitialValue != null)
                    {
                        Write(" = ");
                        Write(varDecl.InitialValue.Accept(this));
                    }
                }
                else if (init is IRAssignment assignment)
                {
                    Write($"{assignment.VariableName} = ");
                    Write(assignment.Value.Accept(this));
                }

                if (i < forLoop.Initializers.Count - 1)
                {
                    Write(", ");
                }
            }

            Write("; ");

            // Condition
            if (forLoop.Condition != null)
            {
                Write(forLoop.Condition.Accept(this));
            }

            Write("; ");

            // Incrementors
            for (int i = 0; i < forLoop.Incrementors.Count; i++)
            {
                Write(forLoop.Incrementors[i].Accept(this));
                if (i < forLoop.Incrementors.Count - 1)
                {
                    Write(", ");
                }
            }

            Write(") ");

            if (forLoop.Body is IRBlock)
            {
                WriteLine("{");
                Indent();
                forLoop.Body.Accept(this);
                Unindent();
                WriteLine("}");
            }
            else
            {
                WriteLine();
                Indent();
                forLoop.Body.Accept(this);
                Unindent();
            }

            return "";
        }

        #endregion

        #region Expression Visitors

        public string VisitLiteral(IRLiteral literal)
        {
            return literal.Value.ToString() ?? "";
        }

        public string VisitVariable(IRVariable variable)
        {
            return variable.Name;
        }

        public string VisitBinaryOperation(IRBinaryOperation binaryOperation)
        {
            var left = binaryOperation.Left.Accept(this);
            var right = binaryOperation.Right.Accept(this);
            var op = binaryOperation.Operation.ToJavaSymbol();
            
            return $"({left} {op} {right})";
        }

        public string VisitUnaryOperation(IRUnaryOperation unaryOperation)
        {
            var operand = unaryOperation.Operand.Accept(this);
            var op = unaryOperation.Operation.ToJavaSymbol();

            if (unaryOperation.IsPrefix)
            {
                // Prefix: ++x, --x, -x, !x
                return $"{op}{operand}";
            }
            else
            {
                // Postfix: x++, x--
                return $"{operand}{op}";
            }
        }

        public string VisitMethodCall(IRMethodCall methodCall)
        {
            var result = new StringBuilder();

            if (methodCall.Target != null)
            {
                result.Append(methodCall.Target.Accept(this));
                result.Append(".");
            }

            result.Append(methodCall.MethodName);
            result.Append("(");

            for (int i = 0; i < methodCall.Arguments.Count; i++)
            {
                result.Append(methodCall.Arguments[i].Accept(this));
                if (i < methodCall.Arguments.Count - 1)
                {
                    result.Append(", ");
                }
            }

            result.Append(")");
            return result.ToString();
        }

        #endregion

        #region Utility Methods

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

        #endregion
    }
}
