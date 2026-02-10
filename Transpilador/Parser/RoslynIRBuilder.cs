using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Builds an IR (Intermediate Representation) from C# source code using Roslyn's CSharpSyntaxWalker.
    /// This class walks through the C# syntax tree and creates corresponding IR nodes.
    /// </summary>
    public class RoslynIRBuilder : CSharpSyntaxWalker
    {
        private SemanticModel _semanticModel = null!;
        private IRProgram _program = null!;
        private IRClass? _currentClass;
        private IRMethod? _currentMethod;

        /// <summary>
        /// Main entry point: Parses C# code and builds an IR representation.
        /// </summary>
        public static IRProgram BuildIR(string csharpCode)
        {
            var tree = CSharpSyntaxTree.ParseText(csharpCode);
            
            // Create compilation for semantic analysis
            var compilation = CSharpCompilation.Create("TempAssembly")
                .AddSyntaxTrees(tree)
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var semanticModel = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var builder = new RoslynIRBuilder();
            builder._semanticModel = semanticModel;
            builder._program = new IRProgram();

            // Walk the syntax tree
            builder.Visit(root);

            return builder._program;
        }

        #region Structure Visitors

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            _program.Namespace = node.Name.ToString();
            base.VisitNamespaceDeclaration(node);
        }

        public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        {
            _program.Namespace = node.Name.ToString();
            base.VisitFileScopedNamespaceDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var irClass = new IRClass(node.Identifier.Text);
            _program.Classes.Add(irClass);
            
            var previousClass = _currentClass;
            _currentClass = irClass;
            
            base.VisitClassDeclaration(node);
            
            _currentClass = previousClass;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_currentClass == null)
                return;

            var returnType = GetTypeName(node.ReturnType);
            var method = new IRMethod(node.Identifier.Text, returnType);

            // Parse parameters
            foreach (var param in node.ParameterList.Parameters)
            {
                var paramType = GetTypeName(param.Type!);
                var parameter = new IRParameter(param.Identifier.Text, paramType);
                method.Parameters.Add(parameter);
            }

            _currentClass.Methods.Add(method);
            
            var previousMethod = _currentMethod;
            _currentMethod = method;

            // Visit method body
            if (node.Body != null)
            {
                VisitBlock(node.Body);
            }
            
            _currentMethod = previousMethod;
        }

        #endregion

        #region Statement Visitors

        public override void VisitBlock(BlockSyntax node)
        {
            if (_currentMethod == null)
                return;

            var block = new IRBlock();
            
            foreach (var statement in node.Statements)
            {
                var irStatement = BuildStatement(statement);
                if (irStatement != null)
                {
                    block.Statements.Add(irStatement);
                }
            }

            // If this is the top-level method body, set it directly
            if (_currentMethod.Body.Statements.Count == 0)
            {
                _currentMethod.Body = block;
            }
        }

        private IRStatement? BuildStatement(StatementSyntax statement)
        {
            return statement switch
            {
                LocalDeclarationStatementSyntax localDecl => BuildVariableDeclaration(localDecl),
                ExpressionStatementSyntax exprStmt => BuildExpressionStatement(exprStmt),
                ReturnStatementSyntax returnStmt => BuildReturnStatement(returnStmt),
                IfStatementSyntax ifStmt => BuildIfStatement(ifStmt),
                WhileStatementSyntax whileStmt => BuildWhileStatement(whileStmt),
                ForStatementSyntax forStmt => BuildForStatement(forStmt),
                BlockSyntax blockStmt => BuildBlockStatement(blockStmt),
                _ => null
            };
        }

        private IRStatement BuildBlockStatement(BlockSyntax node)
        {
            var block = new IRBlock();
            
            foreach (var statement in node.Statements)
            {
                var irStatement = BuildStatement(statement);
                if (irStatement != null)
                {
                    block.Statements.Add(irStatement);
                }
            }
            
            return block;
        }

        private IRStatement BuildVariableDeclaration(LocalDeclarationStatementSyntax node)
        {
            var type = GetTypeName(node.Declaration.Type);
            var variable = node.Declaration.Variables.First();
            
            IRExpression? initialValue = null;
            if (variable.Initializer != null)
            {
                initialValue = BuildExpression(variable.Initializer.Value);
            }
            
            return new IRVariableDeclaration(variable.Identifier.Text, type, initialValue);
        }

        private IRStatement BuildExpressionStatement(ExpressionStatementSyntax node)
        {
            // Check if this is an assignment
            if (node.Expression is AssignmentExpressionSyntax assignment)
            {
                var variableName = assignment.Left.ToString();
                var value = BuildExpression(assignment.Right);
                return new IRAssignment(variableName, value);
            }
            
            // Otherwise, wrap the expression
            var expr = BuildExpression(node.Expression);
            return new IRExpressionStatement(expr);
        }

        private IRStatement BuildReturnStatement(ReturnStatementSyntax node)
        {
            IRExpression? expression = null;
            if (node.Expression != null)
            {
                expression = BuildExpression(node.Expression);
            }
            
            return new IRReturnStatement(expression);
        }

        private IRStatement BuildIfStatement(IfStatementSyntax node)
        {
            var condition = BuildExpression(node.Condition);
            var thenBranch = BuildStatement(node.Statement)!;
            IRStatement? elseBranch = null;
            
            if (node.Else != null)
            {
                elseBranch = BuildStatement(node.Else.Statement);
            }
            
            return new IRIfStatement(condition, thenBranch, elseBranch);
        }

        private IRStatement BuildWhileStatement(WhileStatementSyntax node)
        {
            var condition = BuildExpression(node.Condition);
            var body = BuildStatement(node.Statement)!;
            
            return new IRWhileLoop(condition, body);
        }

        private IRStatement BuildForStatement(ForStatementSyntax node)
        {
            var initializers = new List<IRStatement>();
            if (node.Declaration != null)
            {
                // For with variable declaration: for (int i = 0; ...)
                var type = GetTypeName(node.Declaration.Type);
                foreach (var variable in node.Declaration.Variables)
                {
                    IRExpression? initialValue = null;
                    if (variable.Initializer != null)
                    {
                        initialValue = BuildExpression(variable.Initializer.Value);
                    }
                    initializers.Add(new IRVariableDeclaration(variable.Identifier.Text, type, initialValue));
                }
            }
            else
            {
                // For with initializers: for (i = 0; ...)
                foreach (var init in node.Initializers)
                {
                    if (init is AssignmentExpressionSyntax assignment)
                    {
                        var variableName = assignment.Left.ToString();
                        var value = BuildExpression(assignment.Right);
                        initializers.Add(new IRAssignment(variableName, value));
                    }
                }
            }

            IRExpression? condition = null;
            if (node.Condition != null)
            {
                condition = BuildExpression(node.Condition);
            }

            var incrementors = new List<IRExpression>();
            foreach (var incrementor in node.Incrementors)
            {
                incrementors.Add(BuildExpression(incrementor));
            }

            var body = BuildStatement(node.Statement)!;

            return new IRForLoop(initializers, condition, incrementors, body);
        }

        #endregion

        #region Expression Builders

        private IRExpression BuildExpression(ExpressionSyntax expression)
        {
            return expression switch
            {
                LiteralExpressionSyntax literal => BuildLiteral(literal),
                IdentifierNameSyntax identifier => BuildIdentifier(identifier),
                BinaryExpressionSyntax binary => BuildBinaryExpression(binary),
                PrefixUnaryExpressionSyntax prefixUnary => BuildPrefixUnary(prefixUnary),
                PostfixUnaryExpressionSyntax postfixUnary => BuildPostfixUnary(postfixUnary),
                InvocationExpressionSyntax invocation => BuildMethodCall(invocation),
                ParenthesizedExpressionSyntax paren => BuildExpression(paren.Expression),
                _ => throw new NotSupportedException($"Expression type not supported: {expression.GetType().Name}")
            };
        }

        private IRExpression BuildLiteral(LiteralExpressionSyntax literal)
        {
            var typeInfo = _semanticModel.GetTypeInfo(literal);
            var type = GetTypeName(typeInfo.Type);
            
            return new IRLiteral(literal.Token.ValueText, type);
        }

        private IRExpression BuildIdentifier(IdentifierNameSyntax identifier)
        {
            var typeInfo = _semanticModel.GetTypeInfo(identifier);
            var type = GetTypeName(typeInfo.Type);
            
            return new IRVariable(identifier.Identifier.Text, type);
        }

        private IRExpression BuildBinaryExpression(BinaryExpressionSyntax binary)
        {
            var left = BuildExpression(binary.Left);
            var right = BuildExpression(binary.Right);
            var operation = MapBinaryOperation(binary.OperatorToken.Kind());
            
            var typeInfo = _semanticModel.GetTypeInfo(binary);
            var type = GetTypeName(typeInfo.Type);
            
            return new IRBinaryOperation(left, right, operation, type);
        }

        private IRExpression BuildPrefixUnary(PrefixUnaryExpressionSyntax prefixUnary)
        {
            var operand = BuildExpression(prefixUnary.Operand);
            var operation = MapPrefixUnaryOperation(prefixUnary.OperatorToken.Kind());
            
            var typeInfo = _semanticModel.GetTypeInfo(prefixUnary);
            var type = GetTypeName(typeInfo.Type);
            
            return new IRUnaryOperation(operand, operation, type, isPrefix: true);
        }

        private IRExpression BuildPostfixUnary(PostfixUnaryExpressionSyntax postfixUnary)
        {
            var operand = BuildExpression(postfixUnary.Operand);
            var operation = MapPostfixUnaryOperation(postfixUnary.OperatorToken.Kind());
            
            var typeInfo = _semanticModel.GetTypeInfo(postfixUnary);
            var type = GetTypeName(typeInfo.Type);
            
            return new IRUnaryOperation(operand, operation, type, isPrefix: false);
        }

        private IRExpression BuildMethodCall(InvocationExpressionSyntax invocation)
        {
            string methodName;
            IRExpression? target = null;

            if (invocation.Expression is IdentifierNameSyntax identifier)
            {
                methodName = identifier.Identifier.Text;
            }
            else if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                methodName = memberAccess.Name.Identifier.Text;
                target = BuildExpression(memberAccess.Expression);
            }
            else
            {
                throw new NotSupportedException($"Method call type not supported: {invocation.Expression.GetType().Name}");
            }

            var arguments = new List<IRExpression>();
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                arguments.Add(BuildExpression(arg.Expression));
            }

            var typeInfo = _semanticModel.GetTypeInfo(invocation);
            var type = GetTypeName(typeInfo.Type);

            return new IRMethodCall(methodName, arguments, type, target);
        }

        #endregion

        #region Type and Operation Mappers

        private string GetTypeName(TypeSyntax? typeSyntax)
        {
            if (typeSyntax == null)
                return "void";

            var typeInfo = _semanticModel.GetTypeInfo(typeSyntax);
            return GetTypeName(typeInfo.Type);
        }

        private string GetTypeName(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol == null)
                return "int";

            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Int32 => "int",
                SpecialType.System_Int64 => "long",
                SpecialType.System_Double => "double",
                SpecialType.System_Single => "float",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_String => "string",
                SpecialType.System_Void => "void",
                _ => typeSymbol.Name
            };
        }

        private IROperationType MapBinaryOperation(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => IROperationType.Add,
                SyntaxKind.MinusToken => IROperationType.Subtract,
                SyntaxKind.AsteriskToken => IROperationType.Multiply,
                SyntaxKind.SlashToken => IROperationType.Divide,
                SyntaxKind.PercentToken => IROperationType.Modulo,
                SyntaxKind.EqualsEqualsToken => IROperationType.Equals,
                SyntaxKind.ExclamationEqualsToken => IROperationType.NotEquals,
                SyntaxKind.GreaterThanToken => IROperationType.GreaterThan,
                SyntaxKind.LessThanToken => IROperationType.LessThan,
                SyntaxKind.GreaterThanEqualsToken => IROperationType.GreaterThanOrEqual,
                SyntaxKind.LessThanEqualsToken => IROperationType.LessThanOrEqual,
                SyntaxKind.AmpersandAmpersandToken => IROperationType.LogicalAnd,
                SyntaxKind.BarBarToken => IROperationType.LogicalOr,
                _ => throw new NotSupportedException($"Binary operation not supported: {kind}")
            };
        }

        private IROperationType MapPrefixUnaryOperation(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusPlusToken => IROperationType.PreIncrement,
                SyntaxKind.MinusMinusToken => IROperationType.PreDecrement,
                SyntaxKind.MinusToken => IROperationType.UnaryMinus,
                SyntaxKind.PlusToken => IROperationType.UnaryPlus,
                SyntaxKind.ExclamationToken => IROperationType.LogicalNot,
                _ => throw new NotSupportedException($"Prefix unary operation not supported: {kind}")
            };
        }

        private IROperationType MapPostfixUnaryOperation(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusPlusToken => IROperationType.PostIncrement,
                SyntaxKind.MinusMinusToken => IROperationType.PostDecrement,
                _ => throw new NotSupportedException($"Postfix unary operation not supported: {kind}")
            };
        }

        #endregion
    }
}
