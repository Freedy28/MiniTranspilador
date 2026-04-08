using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Transpilador.Errors;
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
                AccessModifier = ParseAccessModifier(node.Modifiers),
                IsAbstract = node.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))
            };

            if (node.BaseList != null)
            {
                foreach (var baseType in node.BaseList.Types)
                {
                    var typeName = baseType.Type.ToString();
                    var typeSymbol = _semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                    bool isInterface = typeSymbol?.TypeKind == TypeKind.Interface
                        || (typeSymbol == null && typeName.Length > 1 && typeName[0] == 'I' && char.IsUpper(typeName[1]));
                    if (isInterface)
                        _currentClass.Interfaces.Add(typeName);
                    else
                        _currentClass.BaseClass = typeName;
                }
            }

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
                IsEntryPoint = isEntryPoint,
                IsOverride = node.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                IsVirtual = node.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                IsAbstract = node.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword))
            };
            _currentMethod.Parameters.AddRange(ParseMethodParameters(node.ParameterList));
            _currentClass.Methods.Add(_currentMethod);

            if (node.Body != null)
            {
                base.VisitMethodDeclaration(node);
            }

            _currentMethod = null;
        }
        

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            if (_currentClass == null) return;

            _currentMethod = new IRMethod(node.Identifier.Text, "void")
            {
                AccessModifier = ParseAccessModifier(node.Modifiers),
                IsConstructor = true
            };
            _currentMethod.Parameters.AddRange(ParseMethodParameters(node.ParameterList));

            if (node.Initializer != null && node.Initializer.IsKind(SyntaxKind.BaseConstructorInitializer))
            {
                foreach (var arg in node.Initializer.ArgumentList.Arguments)
                    _currentMethod.BaseCallArguments.Add(ParseExpression(arg.Expression));
            }

            _currentClass.Methods.Add(_currentMethod);

            if (node.Body != null)
            {
                base.VisitConstructorDeclaration(node);
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
                    if (IsArrayType(type) && variable.Initializer.Value is InitializerExpressionSyntax arrayInitializer)
                    {
                        initialValue = ParseArrayInitializer(arrayInitializer, GetArrayElementType(type));
                    }
                    else
                    {
                        initialValue = ParseExpression(variable.Initializer.Value);
                    }
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
                if (assignment.Left is ElementAccessExpressionSyntax elementAccess)
                {
                    var targetType = _semanticModel.GetTypeInfo(elementAccess.Expression).Type;
                    if (IsListType(targetType))
                    {
                        if (elementAccess.ArgumentList.Arguments.Count != 1)
                            throw new NotSupportedException("Solo se soporta indexador de una dimensión para List<T>.");

                        var indexExpr = ParseExpression(elementAccess.ArgumentList.Arguments[0].Expression);
                        var valueExpr = ParseExpression(assignment.Right);
                        var setCall = new IRMethodCall(
                            $"{elementAccess.Expression}.set",
                            [indexExpr, valueExpr],
                            "void"
                        );
                        _currentMethod.Body.Add(new IRExpressionStatement(setCall));
                    }
                    else
                    {
                        var variableName = assignment.Left.ToString();
                        var value = ParseExpression(assignment.Right);
                        _currentMethod.Body.Add(new IRAssignment(variableName, value));
                    }
                }
                else
                {
                    var variableName = assignment.Left.ToString();
                    var value = ParseExpression(assignment.Right);
                    _currentMethod.Body.Add(new IRAssignment(variableName, value));
                }
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
                else
                {
                    _currentMethod.Body.Add(new IRExpressionStatement(ParseInvocationExpression(invocation)));
                }
            }
            else if (node.Expression is PostfixUnaryExpressionSyntax postfix)
            {
                _currentMethod.Body.Add(new IRExpressionStatement(ParsePostfixUnaryExpression(postfix)));
            }       
            else if (node.Expression is PrefixUnaryExpressionSyntax prefix)
            {
                _currentMethod.Body.Add(new IRExpressionStatement(ParseUnaryExpression(prefix)));
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

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            if (_currentMethod == null) return;

            // 1. Parsear la condición (ej: x > 5)
            var condition = ParseExpression(node.Condition);
            var ifStmt = new IRIf(condition);

            // 2. Procesar el bloque 'then' (código dentro del if)
            var previousMethod = _currentMethod;
            var tempMethod = new IRMethod("temp", "void");
            _currentMethod = tempMethod;
            
            Visit(node.Statement);  // Visita el cuerpo del if
            
            ifStmt.ThenBranch = new List<IRStatement>(tempMethod.Body);
            _currentMethod = previousMethod;

            // 3. Procesar el bloque 'else' si existe
            if (node.Else != null)
            {
                tempMethod = new IRMethod("temp", "void");
                _currentMethod = tempMethod;
                
                Visit(node.Else.Statement);  // Visita el cuerpo del else
                
                ifStmt.ElseBranch = new List<IRStatement>(tempMethod.Body);
                _currentMethod = previousMethod;
            }

            // 4. Agregar el if completo al cuerpo del método actual
            _currentMethod.Body.Add(ifStmt);
        }
        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            if (_currentMethod == null) return;

            var condition = ParseExpression(node.Condition);
            var whileLoop = new IRWhile(condition);

            var previousMethod = _currentMethod;
            var tempMethod = new IRMethod("temp", "void");
            _currentMethod = tempMethod;

            Visit(node.Statement);

            whileLoop.Body = new List<IRStatement>(tempMethod.Body);
            _currentMethod = previousMethod;

            _currentMethod.Body.Add(whileLoop);
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            if (_currentMethod == null) return;

            IRStatement initializer = null;
            if (node.Declaration != null)
            {
                var type = MapType(node.Declaration.Type);
                var variable = node.Declaration.Variables.First();
                IRExpression initValue = null;
                if (variable.Initializer != null)
                    initValue = ParseExpression(variable.Initializer.Value);
                
                initializer = new IRVariableDeclaration(variable.Identifier.Text, type, initValue);
            }

            IRExpression condition = null;
            if (node.Condition != null) condition = ParseExpression(node.Condition);

            IRExpression incrementor = null;
            if (node.Incrementors.Count > 0) incrementor = ParseExpression(node.Incrementors.First());

            var forLoop = new IRFor(initializer, condition, incrementor);

            var previousMethod = _currentMethod;
            var tempMethod = new IRMethod("temp", "void");
            _currentMethod = tempMethod;

            Visit(node.Statement);

            forLoop.Body = new List<IRStatement>(tempMethod.Body);
            _currentMethod = previousMethod;

            _currentMethod.Body.Add(forLoop);
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            if (_currentMethod == null) return;

            var condition = ParseExpression(node.Condition);
            var doWhileLoop = new IRDoWhile(condition);

            var previousMethod = _currentMethod;
            var tempMethod = new IRMethod("temp", "void");
            _currentMethod = tempMethod;

            Visit(node.Statement);

            doWhileLoop.Body = new List<IRStatement>(tempMethod.Body);
            _currentMethod = previousMethod;

            _currentMethod.Body.Add(doWhileLoop);
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            if (_currentMethod == null) return;

            var itemType = MapType(node.Type);
            var collection = ParseExpression(node.Expression);
            var foreachLoop = new IRForeach(itemType, node.Identifier.Text, collection);

            var previousMethod = _currentMethod;
            var tempMethod = new IRMethod("temp", "void");
            _currentMethod = tempMethod;

            Visit(node.Statement);

            foreachLoop.Body = new List<IRStatement>(tempMethod.Body);
            _currentMethod = previousMethod;

            _currentMethod.Body.Add(foreachLoop);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            if (_currentMethod == null) return;

            var switchStmt = new IRSwitch(ParseExpression(node.Expression));

            foreach (var section in node.Sections)
            {
                foreach (var label in section.Labels)
                {
                    var switchCase = CreateSwitchCase(label);

                    var previousMethod = _currentMethod;
                    var tempMethod = new IRMethod("temp", "void");
                    _currentMethod = tempMethod;

                    foreach (var statement in section.Statements)
                    {
                        Visit(statement);
                    }

                    switchCase.Body = new List<IRStatement>(tempMethod.Body);
                    _currentMethod = previousMethod;

                    switchStmt.Cases.Add(switchCase);
                }
            }

            _currentMethod.Body.Add(switchStmt);
        }

        public override void VisitBreakStatement(BreakStatementSyntax node)
        {
            if (_currentMethod == null) return;
            _currentMethod.Body.Add(new IRBreak());
        }

        public override void VisitContinueStatement(ContinueStatementSyntax node)
        {
            if (_currentMethod == null) return;
            _currentMethod.Body.Add(new IRContinue());
        }

        private IRSwitchCase CreateSwitchCase(SwitchLabelSyntax label)
        {
            if (label is CaseSwitchLabelSyntax caseLabel)
                return new IRSwitchCase(ParseExpression(caseLabel.Value));

            if (label is DefaultSwitchLabelSyntax)
                return new IRSwitchCase(null, true);

            throw new NotSupportedException($"Etiqueta switch no soportada: {label.GetType().Name}");
        }


        private IRExpression ParseExpression(ExpressionSyntax expression)
        {
            switch (expression)
            {
                case LiteralExpressionSyntax literal:
                    return ParseLiteral(literal);

                case IdentifierNameSyntax identifier:
                    var identifierType = _semanticModel.GetTypeInfo(identifier).Type;
                    return new IRVariable(identifier.Identifier.Text, MapTypeSymbol(identifierType));

                case BinaryExpressionSyntax binary:
                    return ParseBinaryExpression(binary);

                case PrefixUnaryExpressionSyntax unary:
                    return ParseUnaryExpression(unary);

                case ParenthesizedExpressionSyntax paren:
                    return ParseExpression(paren.Expression);

                case InvocationExpressionSyntax invocation:
                    return ParseInvocationExpression(invocation);
                case PostfixUnaryExpressionSyntax postfix:
                    return ParsePostfixUnaryExpression(postfix);
                case ArrayCreationExpressionSyntax arrayCreation:
                    return ParseArrayCreationExpression(arrayCreation);
                case ImplicitArrayCreationExpressionSyntax implicitArray:
                    return ParseImplicitArrayCreationExpression(implicitArray);
                case ElementAccessExpressionSyntax elementAccess:
                    return ParseElementAccessExpression(elementAccess);
                case MemberAccessExpressionSyntax memberAccess:
                    return ParseMemberAccessExpression(memberAccess);
                case ObjectCreationExpressionSyntax objectCreation:
                    return ParseObjectCreationExpression(objectCreation);
                case IsPatternExpressionSyntax isPattern:
                    return ParseTypeCheckExpression(isPattern);
                case CastExpressionSyntax cast:
                    return ParseCastExpression(cast);

                default:
                    throw new NotSupportedException($"Expresión no soportada: {expression.GetType().Name}");
            }
        }

        private IRExpression ParseObjectCreationExpression(ObjectCreationExpressionSyntax objectCreation)
        {
            var createdType = _semanticModel.GetTypeInfo(objectCreation).Type;

            if (IsListType(createdType))
            {
                if (objectCreation.Initializer != null && objectCreation.Initializer.Expressions.Count > 0)
                    throw new NotSupportedException("Inicializador de List<T> no soportado. Usa Add().");

                var namedType = createdType as INamedTypeSymbol;
                var elementType = namedType != null && namedType.TypeArguments.Length > 0
                    ? MapTypeSymbol(namedType.TypeArguments[0])
                    : "int";

                return new IRListCreation(elementType);
            }

            var className = objectCreation.Type.ToString();
            var args = new List<IRExpression>();
            if (objectCreation.ArgumentList != null)
            {
                foreach (var arg in objectCreation.ArgumentList.Arguments)
                    args.Add(ParseExpression(arg.Expression));
            }
            return new IRObjectCreation(className, args);
        }

        private IRExpression ParseTypeCheckExpression(IsPatternExpressionSyntax isPattern)
        {
            var expression = ParseExpression(isPattern.Expression);
            if (isPattern.Pattern is TypePatternSyntax typePattern)
            {
                var typeName = typePattern.Type.ToString();
                return new IRTypeCheck(expression, typeName);
            }
            throw new NotSupportedException("Solo se soporta la verificación de tipo simple (x is TipoX).");
        }

        private IRExpression ParseCastExpression(CastExpressionSyntax cast)
        {
            var expression = ParseExpression(cast.Expression);
            var typeName = cast.Type.ToString();
            return new IRCastExpression(typeName, expression);
        }

        private IRArrayCreation ParseArrayInitializer(InitializerExpressionSyntax initializer, string elementType)
        {
            var values = new List<IRExpression>();
            foreach (var expr in initializer.Expressions)
            {
                values.Add(ParseExpression(expr));
            }

            return new IRArrayCreation(elementType, null, values);
        }

        private IRArrayCreation ParseArrayCreationExpression(ArrayCreationExpressionSyntax arrayCreation)
        {
            if (arrayCreation.Type.RankSpecifiers.Count != 1 || arrayCreation.Type.RankSpecifiers[0].Rank != 1)
                throw new NotSupportedException("Solo se soportan arreglos de una dimensión.");

            var elementType = MapType(arrayCreation.Type.ElementType);
            IRExpression sizeExpression = null;

            var rank = arrayCreation.Type.RankSpecifiers.FirstOrDefault();
            if (rank != null && rank.Sizes.Count > 0 && rank.Sizes[0] is not OmittedArraySizeExpressionSyntax)
            {
                sizeExpression = ParseExpression(rank.Sizes[0]);
            }

            if (arrayCreation.Initializer != null)
            {
                return ParseArrayInitializer(arrayCreation.Initializer, elementType);
            }

            return new IRArrayCreation(elementType, sizeExpression);
        }

        private IRArrayCreation ParseImplicitArrayCreationExpression(ImplicitArrayCreationExpressionSyntax implicitArray)
        {
            var values = new List<IRExpression>();
            foreach (var expr in implicitArray.Initializer.Expressions)
            {
                values.Add(ParseExpression(expr));
            }

            var elementType = values.Count > 0 ? values[0].Type : "int";
            return new IRArrayCreation(elementType, null, values);
        }

        private IRExpression ParseElementAccessExpression(ElementAccessExpressionSyntax elementAccess)
        {
            if (elementAccess.ArgumentList.Arguments.Count != 1)
                throw new NotSupportedException("Solo se soporta indexador de una dimensión.");

            var targetType = _semanticModel.GetTypeInfo(elementAccess.Expression).Type;
            if (IsListType(targetType))
            {
                var indexExpr = ParseExpression(elementAccess.ArgumentList.Arguments[0].Expression);
                var listAccessType = MapTypeSymbol(_semanticModel.GetTypeInfo(elementAccess).Type);
                return new IRMethodCall($"{elementAccess.Expression}.get", [indexExpr], listAccessType);
            }

            var arrayExpression = ParseExpression(elementAccess.Expression);
            var indexExpression = ParseExpression(elementAccess.ArgumentList.Arguments[0].Expression);
            var accessType = MapTypeSymbol(_semanticModel.GetTypeInfo(elementAccess).Type);

            return new IRArrayAccess(arrayExpression, indexExpression, accessType);
        }

        private IRExpression ParseMemberAccessExpression(MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess.Name.Identifier.Text == "Length")
            {
                var targetType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (targetType is IArrayTypeSymbol)
                {
                    return new IRArrayLength(ParseExpression(memberAccess.Expression));
                }
            }

            if (memberAccess.Name.Identifier.Text == "Count")
            {
                var targetType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (IsListType(targetType))
                {
                    return new IRMethodCall($"{memberAccess.Expression}.size", [], "int");
                }
            }

            throw new NotSupportedException($"Acceso de miembro no soportado: {memberAccess}");
        }

        private IRUnaryOperation ParsePostfixUnaryExpression(PostfixUnaryExpressionSyntax postfix)
        {
            var operand = ParseExpression(postfix.Operand);
            var operation = postfix.OperatorToken.Kind() switch
            {
                SyntaxKind.PlusPlusToken => IRUnaryOperationType.Increment,
                SyntaxKind.MinusMinusToken => IRUnaryOperationType.Decrement,
                _ => throw new NotSupportedException($"Operador postfix no soportado: {postfix.OperatorToken.Kind()}")
            };
            return new IRUnaryOperation(operand, operation, operand.Type);
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

            var parsedArgs = new List<IRExpression>();
            foreach (var arg in args)
            {
                parsedArgs.Add(ParseExpression(arg.Expression));
            }

            if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                var targetType = _semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                if (IsListType(targetType) && memberAccess.Name.Identifier.Text == "Add")
                {
                    methodName = $"{memberAccess.Expression}.add";
                }
                else if (memberAccess.Expression is BaseExpressionSyntax)
                {
                    methodName = $"super.{memberAccess.Name.Identifier.Text}";
                }
            }

            var returnType = MapTypeSymbol(_semanticModel.GetTypeInfo(invocation).Type);
            return new IRMethodCall(methodName, parsedArgs, returnType);
        }

        private List<IRParameter> ParseMethodParameters(ParameterListSyntax parameterList)
        {
            var parameters = new List<IRParameter>();

            foreach (var parameter in parameterList.Parameters)
            {
                if (parameter.Type == null)
                    throw new NotSupportedException($"Parámetro sin tipo no soportado: {parameter.Identifier.Text}");

                var type = MapType(parameter.Type);
                parameters.Add(new IRParameter(parameter.Identifier.Text, type));
            }

            return parameters;
        }



        private IRLiteral ParseLiteral(LiteralExpressionSyntax literal)
        {
            var typeInfo = _semanticModel.GetTypeInfo(literal);
            var type = typeInfo.Type?.SpecialType.ToString() ?? "int";

            return new IRLiteral(literal.Token.ValueText, MapSpecialType(type));
        }

        private IRExpression ParseBinaryExpression(BinaryExpressionSyntax binary)
        {
            if (binary.OperatorToken.IsKind(SyntaxKind.AsKeyword))
            {
                var expr = ParseExpression(binary.Left);
                var typeName = binary.Right.ToString();
                return new IRCastExpression(typeName, expr);
            }

            if (binary.OperatorToken.IsKind(SyntaxKind.IsKeyword))
            {
                var expr = ParseExpression(binary.Left);
                var typeName = binary.Right.ToString();
                return new IRTypeCheck(expr, typeName);
            }

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

        private string MapTypeSymbol(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null) return "int";

            if (typeSymbol is IArrayTypeSymbol arrayType)
            {
                return $"{MapTypeSymbol(arrayType.ElementType)}[]";
            }

            return typeSymbol.SpecialType switch
            {
                SpecialType.System_Int32 => "int",
                SpecialType.System_Double => "double",
                SpecialType.System_Single => "float",
                SpecialType.System_Int64 => "long",
                SpecialType.System_String => "string",
                SpecialType.System_Boolean => "bool",
                SpecialType.System_Void => "void",
                _ => typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            };
        }

        private bool IsArrayType(string type)
        {
            return type != null && type.EndsWith("[]", StringComparison.Ordinal);
        }

        private string GetArrayElementType(string arrayType)
        {
            if (!IsArrayType(arrayType)) return arrayType;
            return arrayType.Substring(0, arrayType.Length - 2);
        }

        private bool IsListType(ITypeSymbol typeSymbol)
        {
            return typeSymbol is INamedTypeSymbol named
                && named.Name == "List"
                && named.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic";
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
                {
                    if (IsArrayType(type) && variable.Initializer.Value is InitializerExpressionSyntax arrayInitializer)
                    {
                        initialValue = ParseArrayInitializer(arrayInitializer, GetArrayElementType(type));
                    }
                    else
                    {
                        initialValue = ParseExpression(variable.Initializer.Value);
                    }
                }

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