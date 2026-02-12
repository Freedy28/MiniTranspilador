using System;
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
        public IRProgram ParseToIR(string csharpCode)
        {
            var tree = CSharpSyntaxTree.ParseText(csharpCode);
            
            var compilation = CSharpCompilation.Create("TempAssembly")
                .AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var semanticModel = compilation.GetSemanticModel(tree);

            var walker = new IRBuilderWalker(semanticModel);
            walker.Visit(tree.GetRoot());

            return walker.Program;
        }
    }

    internal class IRBuilderWalker : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private IRClass _currentClass;
        private IRMethod _currentMethod;

        public IRProgram Program { get; }

        public IRBuilderWalker(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
            Program = new IRProgram();
        }

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            Program.Namespace = node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            _currentClass = new IRClass(node.Identifier.Text);
            Program.Classes.Add(_currentClass);
            base.VisitClassDeclaration(node);
            _currentClass = null;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_currentClass == null) return;

            var returnType = MapType(node.ReturnType);
            _currentMethod = new IRMethod(node.Identifier.Text, returnType);
            _currentClass.Methods.Add(_currentMethod);

            if (node.Body != null)
            {
                base.VisitMethodDeclaration(node);
            }

            _currentMethod = null;
        }

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (_currentMethod == null) return;

            var type = MapType(node.Declaration.Type);

            foreach (var variable in node.Declaration.Variables)
            {
                IRExpression initialValue = null;

                if (variable.Initializer != null)
                {
                    initialValue = ParseExpression(variable.Initializer.Value);
                }

                var declaration = new IRVariableDeclaration(
                    variable.Identifier.Text,
                    type,
                    initialValue
                );
    
                _currentMethod.Body.Add(declaration);
            }

            base.VisitLocalDeclarationStatement(node);
        }

        public override void VisitExpressionStatement(ExpressionStatementSyntax node)
        {
            if (_currentMethod == null) return;

            if (node.Expression is AssignmentExpressionSyntax assignment)
            {
                var variableName = assignment.Left.ToString();
                var value = ParseExpression(assignment.Right);

                var irAssignment = new IRAssignment(variableName, value);
                _currentMethod.Body.Add(irAssignment);
            }

            base.VisitExpressionStatement(node);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            if (_currentMethod == null) return;

            if (node.Expression != null)
            {
                _currentMethod.ReturnExpression = ParseExpression(node.Expression);
            }

            base.VisitReturnStatement(node);
        }

        private IRExpression ParseExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return ParseLiteral(literal);

                case IdentifierNameSyntax identifier:
                    return new IRVariable(identifier.Identifier.Text, "int");

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

            return new IRBinaryOperation(left, right, operation, "int");
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
                "string" => "string",
                "bool" => "bool",
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
                "System_String" => "string",
                "System_Boolean" => "bool",
                _ => "int"
            };
        }
    }
}