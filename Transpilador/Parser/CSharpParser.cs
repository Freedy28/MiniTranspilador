using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Transpilador.Errors;
using Transpilador.Models;
using Transpilador.Models.Base;
using Transpilador.Models.Expressions;using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Parser
{
    public class CSharpParser
    {
        public IRProgram ParseToIR(string csharpCode)
        {
            var tree = CSharpSyntaxTree.ParseText(csharpCode);

            var syntaxErrors = tree.GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();

            if (syntaxErrors.Count > 0)
            {
                var details = syntaxErrors.Select(d =>
                {
                    var pos = d.Location.GetLineSpan().StartLinePosition;
                    return new ErrorDetail(d.Id, d.GetMessage(), pos.Line + 1, pos.Character + 1);
                }).ToList();

                throw new TranspileException(
                    $"El código C# contiene {syntaxErrors.Count} error(es) de sintaxis.",
                    "parse",
                    details
                );
            }

            var compilation = CSharpCompilation.Create("TempAssembly")
                .AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var semanticModel = compilation.GetSemanticModel(tree);

            var walker = new IRBuilderWalker(semanticModel);
            walker.Visit(tree.GetRoot());

            if (walker.Program.Classes.Count == 0)
            {
                var hasTopLevelStatements = tree.GetRoot()
                    .DescendantNodes()
                    .Any(n => n is GlobalStatementSyntax || n is LocalFunctionStatementSyntax);

                var hint = hasTopLevelStatements
                    ? " Se detectaron sentencias o funciones en el nivel superior, pero el transpilador requiere clases con sus métodos definidos."
                    : " Asegúrate de que el código tenga al menos una declaración de clase (class).";

                throw new TranspileException(
                    "No se encontraron clases en el código." + hint,
                    "no_classes"
                );
            }

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
            _currentClass = new IRClass(node.Identifier.Text)
            {
                AccessModifier = ParseAccessModifier(node.Modifiers)
            };
            Program.Classes.Add(_currentClass);
            base.VisitClassDeclaration(node);
            _currentClass = null;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_currentClass == null) return;

            var returnType = MapType(node.ReturnType);
            var methodName = node.Identifier.Text;
            bool isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            bool isEntryPoint = isStatic && methodName == "Main";

            _currentMethod = new IRMethod(methodName, returnType)
            {
                AccessModifier = ParseAccessModifier(node.Modifiers),
                IsStatic = isStatic,
                IsEntryPoint = isEntryPoint
            };
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
                _currentMethod.Body.Add(new IRAssignment(variableName, value));
            }
            else if (node.Expression is InvocationExpressionSyntax invocation)
            {
                var methodName = invocation.Expression.ToString();
                if (methodName == "Console.WriteLine" || methodName == "Console.Write")
                {
                    bool newLine = methodName == "Console.WriteLine";
                    IRExpression argument = null;
                    if (invocation.ArgumentList.Arguments.Count > 0)
                        argument = ParseExpression(invocation.ArgumentList.Arguments[0].Expression);
                    _currentMethod.Body.Add(new IRConsoleOutput(argument, newLine));
                }
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

                case PrefixUnaryExpressionSyntax unary:
                    return ParseUnaryExpression(unary);

                case ParenthesizedExpressionSyntax paren:
                    return ParseExpression(paren.Expression);

                case InvocationExpressionSyntax invocation:
                    return ParseInvocationExpression(invocation);

                default:
                    throw new NotSupportedException($"Expresión no soportada: {expression.GetType().Name}");
            }
        }

        private IRExpression ParseInvocationExpression(InvocationExpressionSyntax invocation)
        {
            var methodName = invocation.Expression.ToString();
            var args = invocation.ArgumentList.Arguments;

            if (methodName == "Console.ReadLine")
                return new IRConsoleInput("string");

            if (args.Count == 1 &&
                args[0].Expression is InvocationExpressionSyntax inner &&
                inner.Expression.ToString() == "Console.ReadLine")
            {
                return methodName switch
                {
                    "int.Parse"    => new IRConsoleInput("int"),
                    "double.Parse" => new IRConsoleInput("double"),
                    "float.Parse"  => new IRConsoleInput("float"),
                    "long.Parse"   => new IRConsoleInput("long"),
                    _              => new IRConsoleInput("string")
                };
            }

            throw new NotSupportedException($"Invocación no soportada: {methodName}");
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
                // Aritméticos
                SyntaxKind.PlusToken => IROperationType.Add,
                SyntaxKind.MinusToken => IROperationType.Subtract,
                SyntaxKind.AsteriskToken => IROperationType.Multiply,
                SyntaxKind.SlashToken => IROperationType.Divide,
                SyntaxKind.PercentToken => IROperationType.Modulo,

                // Comparación
                SyntaxKind.EqualsEqualsToken => IROperationType.Equal,
                SyntaxKind.ExclamationEqualsToken => IROperationType.NotEqual,
                SyntaxKind.LessThanToken => IROperationType.LessThan,
                SyntaxKind.GreaterThanToken => IROperationType.GreaterThan,
                SyntaxKind.LessThanEqualsToken => IROperationType.LessThanOrEqual,
                SyntaxKind.GreaterThanEqualsToken => IROperationType.GreaterThanOrEqual,

                // Lógicos
                SyntaxKind.AmpersandAmpersandToken => IROperationType.LogicalAnd,
                SyntaxKind.BarBarToken => IROperationType.LogicalOr,

                _ => throw new NotSupportedException($"Operación no soportada: {kind}")
            };
        }




        private IRUnaryOperation ParseUnaryExpression(PrefixUnaryExpressionSyntax unary)
        {
            var operand = ParseExpression(unary.Operand);
            var operation = unary.OperatorToken.Kind() switch
            {
                SyntaxKind.MinusToken => IRUnaryOperationType.Negate,
                SyntaxKind.ExclamationToken => IRUnaryOperationType.LogicalNot,
                _ => throw new NotSupportedException($"Operador unario no soportado: {unary.OperatorToken.Kind()}")
            };

            return new IRUnaryOperation(operand, operation, operand.Type);
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

        private IRAccessModifier ParseAccessModifier(SyntaxTokenList modifiers)
        {
            bool hasProtected = modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));
            bool hasInternal  = modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));
            bool hasPrivate   = modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

            if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) return IRAccessModifier.Public;
            if (hasProtected && hasInternal)                             return IRAccessModifier.ProtectedInternal;
            if (hasPrivate   && hasProtected)                           return IRAccessModifier.PrivateProtected;
            if (hasPrivate)                                             return IRAccessModifier.Private;
            if (hasProtected)                                           return IRAccessModifier.Protected;
            if (hasInternal)                                            return IRAccessModifier.Internal;
            return IRAccessModifier.Internal;
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (_currentClass == null) return;

            var type     = MapType(node.Declaration.Type);
            bool isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            var modifier  = ParseAccessModifier(node.Modifiers);

            foreach (var variable in node.Declaration.Variables)
            {
                IRExpression initialValue = null;
                if (variable.Initializer != null)
                    initialValue = ParseExpression(variable.Initializer.Value);

                var field = new IRField(variable.Identifier.Text, type)
                {
                    AccessModifier = modifier,
                    IsStatic       = isStatic,
                    InitialValue   = initialValue
                };

                _currentClass.Fields.Add(field);
            }
        }
    }
}