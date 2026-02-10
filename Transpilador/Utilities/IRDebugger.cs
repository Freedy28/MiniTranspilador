using System;
using System.Text;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Utilities
{
    /// <summary>
    /// Visitor that generates a visual representation of the IR tree for debugging.
    /// Useful for understanding the structure of the IR and debugging parsing issues.
    /// </summary>
    public class IRDebugger : IIRVisitor<string>
    {
        private StringBuilder _sb = new StringBuilder();
        private int _indentLevel = 0;
        private const string IndentString = "  ";

        /// <summary>
        /// Generates a tree view of the IR for debugging purposes.
        /// </summary>
        public string Debug(IRProgram program)
        {
            _sb = new StringBuilder();
            _indentLevel = 0;
            program.Accept(this);
            return _sb.ToString();
        }

        #region Structure Visitors

        public string VisitProgram(IRProgram program)
        {
            WriteLine($"IRProgram");
            Indent();
            
            if (!string.IsNullOrEmpty(program.Namespace))
            {
                WriteLine($"Namespace: {program.Namespace}");
            }
            
            WriteLine($"Classes: {program.Classes.Count}");
            foreach (var irClass in program.Classes)
            {
                irClass.Accept(this);
            }
            
            Unindent();
            return _sb.ToString();
        }

        public string VisitClass(IRClass irClass)
        {
            WriteLine($"IRClass: {irClass.Name}");
            Indent();
            
            WriteLine($"Methods: {irClass.Methods.Count}");
            foreach (var method in irClass.Methods)
            {
                method.Accept(this);
            }
            
            Unindent();
            return "";
        }

        public string VisitMethod(IRMethod method)
        {
            WriteLine($"IRMethod: {method.Name}");
            Indent();
            
            WriteLine($"ReturnType: {method.ReturnType}");
            
            if (method.Parameters.Count > 0)
            {
                WriteLine($"Parameters: {method.Parameters.Count}");
                Indent();
                foreach (var param in method.Parameters)
                {
                    param.Accept(this);
                }
                Unindent();
            }
            
            WriteLine("Body:");
            Indent();
            method.Body.Accept(this);
            Unindent();
            
            Unindent();
            return "";
        }

        public string VisitParameter(IRParameter parameter)
        {
            WriteLine($"IRParameter: {parameter.Type} {parameter.Name}");
            return "";
        }

        #endregion

        #region Statement Visitors

        public string VisitBlock(IRBlock block)
        {
            WriteLine($"IRBlock ({block.Statements.Count} statements)");
            Indent();
            
            foreach (var statement in block.Statements)
            {
                statement.Accept(this);
            }
            
            Unindent();
            return "";
        }

        public string VisitVariableDeclaration(IRVariableDeclaration declaration)
        {
            WriteLine($"IRVariableDeclaration: {declaration.Type} {declaration.Name}");
            if (declaration.InitialValue != null)
            {
                Indent();
                WriteLine("InitialValue:");
                Indent();
                declaration.InitialValue.Accept(this);
                Unindent();
                Unindent();
            }
            return "";
        }

        public string VisitAssignment(IRAssignment assignment)
        {
            WriteLine($"IRAssignment: {assignment.VariableName} =");
            Indent();
            assignment.Value.Accept(this);
            Unindent();
            return "";
        }

        public string VisitReturnStatement(IRReturnStatement returnStatement)
        {
            WriteLine("IRReturnStatement");
            if (returnStatement.Expression != null)
            {
                Indent();
                returnStatement.Expression.Accept(this);
                Unindent();
            }
            return "";
        }

        public string VisitExpressionStatement(IRExpressionStatement expressionStatement)
        {
            WriteLine("IRExpressionStatement");
            Indent();
            expressionStatement.Expression.Accept(this);
            Unindent();
            return "";
        }

        public string VisitIfStatement(IRIfStatement ifStatement)
        {
            WriteLine("IRIfStatement");
            Indent();
            
            WriteLine("Condition:");
            Indent();
            ifStatement.Condition.Accept(this);
            Unindent();
            
            WriteLine("Then:");
            Indent();
            ifStatement.ThenBranch.Accept(this);
            Unindent();
            
            if (ifStatement.ElseBranch != null)
            {
                WriteLine("Else:");
                Indent();
                ifStatement.ElseBranch.Accept(this);
                Unindent();
            }
            
            Unindent();
            return "";
        }

        public string VisitWhileLoop(IRWhileLoop whileLoop)
        {
            WriteLine("IRWhileLoop");
            Indent();
            
            WriteLine("Condition:");
            Indent();
            whileLoop.Condition.Accept(this);
            Unindent();
            
            WriteLine("Body:");
            Indent();
            whileLoop.Body.Accept(this);
            Unindent();
            
            Unindent();
            return "";
        }

        public string VisitForLoop(IRForLoop forLoop)
        {
            WriteLine("IRForLoop");
            Indent();
            
            if (forLoop.Initializers.Count > 0)
            {
                WriteLine($"Initializers: {forLoop.Initializers.Count}");
                Indent();
                foreach (var init in forLoop.Initializers)
                {
                    init.Accept(this);
                }
                Unindent();
            }
            
            if (forLoop.Condition != null)
            {
                WriteLine("Condition:");
                Indent();
                forLoop.Condition.Accept(this);
                Unindent();
            }
            
            if (forLoop.Incrementors.Count > 0)
            {
                WriteLine($"Incrementors: {forLoop.Incrementors.Count}");
                Indent();
                foreach (var inc in forLoop.Incrementors)
                {
                    inc.Accept(this);
                }
                Unindent();
            }
            
            WriteLine("Body:");
            Indent();
            forLoop.Body.Accept(this);
            Unindent();
            
            Unindent();
            return "";
        }

        #endregion

        #region Expression Visitors

        public string VisitLiteral(IRLiteral literal)
        {
            WriteLine($"IRLiteral: {literal.Value} (Type: {literal.Type})");
            return "";
        }

        public string VisitVariable(IRVariable variable)
        {
            WriteLine($"IRVariable: {variable.Name} (Type: {variable.Type})");
            return "";
        }

        public string VisitBinaryOperation(IRBinaryOperation binaryOperation)
        {
            WriteLine($"IRBinaryOperation: {binaryOperation.Operation.ToSymbol()} (Type: {binaryOperation.Type})");
            Indent();
            
            WriteLine("Left:");
            Indent();
            binaryOperation.Left.Accept(this);
            Unindent();
            
            WriteLine("Right:");
            Indent();
            binaryOperation.Right.Accept(this);
            Unindent();
            
            Unindent();
            return "";
        }

        public string VisitUnaryOperation(IRUnaryOperation unaryOperation)
        {
            var prefix = unaryOperation.IsPrefix ? "Prefix" : "Postfix";
            WriteLine($"IRUnaryOperation: {prefix} {unaryOperation.Operation.ToSymbol()} (Type: {unaryOperation.Type})");
            Indent();
            unaryOperation.Operand.Accept(this);
            Unindent();
            return "";
        }

        public string VisitMethodCall(IRMethodCall methodCall)
        {
            WriteLine($"IRMethodCall: {methodCall.MethodName} (Type: {methodCall.Type})");
            Indent();
            
            if (methodCall.Target != null)
            {
                WriteLine("Target:");
                Indent();
                methodCall.Target.Accept(this);
                Unindent();
            }
            
            if (methodCall.Arguments.Count > 0)
            {
                WriteLine($"Arguments: {methodCall.Arguments.Count}");
                Indent();
                foreach (var arg in methodCall.Arguments)
                {
                    arg.Accept(this);
                }
                Unindent();
            }
            
            Unindent();
            return "";
        }

        #endregion

        #region Helper Methods

        private void WriteLine(string text)
        {
            _sb.AppendLine(GetIndent() + text);
        }

        private string GetIndent()
        {
            return new string(' ', _indentLevel * IndentString.Length);
        }

        private void Indent()
        {
            _indentLevel++;
        }

        private void Unindent()
        {
            _indentLevel = Math.Max(0, _indentLevel - 1);
        }

        #endregion
    }
}
