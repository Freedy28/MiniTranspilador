using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Parser
{
    public class CSharpParser
    {
        private SemanticModel _semanticModel;

        public IRProgram ParseToIR(string csharpCode)
        {
            // 1. Parsear el código C#
            var tree = CSharpSyntaxTree.ParseText(csharpCode);
            
            // 2. Crear compilación para resolver símbolos
            var compilation = CSharpCompilation.Create("TempAssembly")
                .AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            _semanticModel = compilation.GetSemanticModel(tree);

            // 3. Convertir a IR
            var program = new IRProgram();
            var root = tree.GetRoot();

            // Buscar namespace
            var namespaceDecl = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            if (namespaceDecl != null)
            {
                program.Namespace = namespaceDecl.Name.ToString();
            }

            // Buscar clases
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var classDecl in classes)
            {
                var irClass = ParseClass(classDecl);
                program.Classes.Add(irClass);
            }

            return program;
        }

        private IRClass ParseClass(ClassDeclarationSyntax classDecl)
        {
            var irClass = new IRClass(classDecl.Identifier.Text);

            // Parsear métodos
            var methods = classDecl.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var irMethod = ParseMethod(method);
                irClass.Methods.Add(irMethod);
            }

            return irClass;
        }

        private IRMethod ParseMethod(MethodDeclarationSyntax methodDecl)
        {
            var returnType = MapType(methodDecl.ReturnType);
            var irMethod = new IRMethod(methodDecl.Identifier.Text, returnType);

            if (methodDecl.Body != null)
            {
                // Parsear sentencias del cuerpo del método
                foreach (var statement in methodDecl.Body.Statements)
                {
                    ParseStatement(statement, irMethod);
                }
            }

            return irMethod;
        }

        private void ParseStatement(StatementSyntax statement, IRMethod method)
        {
            switch (statement)
            {
                case LocalDeclarationStatementSyntax localDecl:
                    ParseVariableDeclaration(localDecl, method);
                    break;
                
                case ExpressionStatementSyntax exprStmt:
                    ParseExpressionStatement(exprStmt, method);
                    break;
                
                case ReturnStatementSyntax returnStmt:
                    ParseReturnStatement(returnStmt, method);
                    break;
            }
        }

        private void ParseVariableDeclaration(LocalDeclarationStatementSyntax localDecl, IRMethod method)
        {
            var type = MapType(localDecl.Declaration.Type);
            
            foreach (var variable in localDecl.Declaration.Variables)
            {
                IRExpression? initialValue = null;
                
                if (variable.Initializer != null)
                {
                    initialValue = ParseExpression(variable.Initializer.Value);
                }

                var declaration = new IRVariableDeclaration(variable.Identifier.Text, type, initialValue);
                method.Statements.Add(declaration);
            }
        }

        private void ParseExpressionStatement(ExpressionStatementSyntax exprStmt, IRMethod method)
        {
            if (exprStmt.Expression is AssignmentExpressionSyntax assignment)
            {
                var variableName = assignment.Left.ToString();
                var value = ParseExpression(assignment.Right);
                
                var irAssignment = new IRAssignment(variableName, value);
                method.Assignments.Add(irAssignment);
            }
        }

        private void ParseReturnStatement(ReturnStatementSyntax returnStmt, IRMethod method)
        {
            if (returnStmt.Expression != null)
            {
                method.ReturnExpression = ParseExpression(returnStmt.Expression);
            }
        }

        private IRExpression ParseExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return ParseLiteral(literal);
                
                case IdentifierNameSyntax identifier:
                    return new IRVariable(identifier.Identifier.Text, "int"); // Simplificado
                
                case BinaryExpressionSyntax binary:
                    return ParseBinaryExpression(binary);
                
                default:
                    throw new NotSupportedException($"Expresión no soportada: {expression.GetType().Name}");
            }
        }

        private IRLiteral ParseLiteral(LiteralExpressionSyntax literal)
        {
            var typeInfo = _semanticModel.GetTypeInfo(literal);
            var type = typeInfo.Type?.SpecialType.ToString() ?? "int";
            
            return new IRLiteral(literal.Token.ValueText, MapSpecialType(type));
        }

        private IRBinaryOperation ParseBinaryExpression(BinaryExpressionSyntax binary)
        {
            var left = ParseExpression(binary.Left);
            var right = ParseExpression(binary.Right);
            var operation = MapBinaryOperation(binary.OperatorToken.Kind());
            
            return new IRBinaryOperation(left, right, operation, "int"); // Simplificado
        }

        private IROperationType MapBinaryOperation(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => IROperationType.Add,
                SyntaxKind.MinusToken => IROperationType.Subtract,
                SyntaxKind.AsteriskToken => IROperationType.Multiply,
                SyntaxKind.SlashToken => IROperationType.Divide,
                _ => throw new NotSupportedException($"Operación no soportada: {kind}")
            };
        }

        private string MapType(TypeSyntax type)
        {
            return type.ToString() switch
            {
                "int" => "int",
                "double" => "double",
                "float" => "float",
                "long" => "long",
                "void" => "void",
                _ => type.ToString()
            };
        }

        private string MapSpecialType(string specialType)
        {
            return specialType switch
            {
                "System_Int32" => "int",
                "System_Double" => "double",
                "System_Single" => "float",
                "System_Int64" => "long",
                _ => "int"
            };
        }
    }
}