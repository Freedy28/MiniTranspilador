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

            var needsListImports = ProgramNeedsListImports(program);
            var needsScanner = ProgramNeedsScanner(program);

            if (needsListImports)
            {
                WriteLine("import java.util.ArrayList;");
                WriteLine("import java.util.List;");
            }

            if (needsScanner)
            {
                WriteLine("import java.util.Scanner;");
            }

            if (needsListImports || needsScanner)
            {
                WriteLine();
            }

            VisitProgram(program);

            return _sb.ToString();
        }

        public override void VisitClass(IRClass irClass)
        {
            var modifier = MapModifierToJava(irClass.AccessModifier);
            var modifierStr = string.IsNullOrEmpty(modifier) ? "" : $"{modifier} ";
            var abstractStr = irClass.IsAbstract ? "abstract " : "";
            var extendsStr = irClass.BaseClass != null ? $" extends {irClass.BaseClass}" : "";
            var implementsStr = irClass.Interfaces.Count > 0 ? $" implements {string.Join(", ", irClass.Interfaces)}" : "";
            WriteLine($"{modifierStr}{abstractStr}class {irClass.Name}{extendsStr}{implementsStr} {{");
            Indent();

            if (ClassNeedsScanner(irClass))
                WriteLine("private static Scanner scanner = new Scanner(System.in);");

            base.VisitClass(irClass);

            Unindent();
            WriteLine("}");
            WriteLine();
        }

        public override void VisitMethod(IRMethod method)
        {
            var modifier = MapModifierToJava(method.AccessModifier);
            var modifierStr = string.IsNullOrEmpty(modifier) ? "" : $"{modifier} ";
            var parameters = string.Join(", ", method.Parameters.Select(p => $"{MapTypeToJava(p.Type)} {p.Name}"));

            if (method.IsConstructor)
            {
                WriteLine($"{modifierStr}{method.Name}({parameters}) {{");
                Indent();

                if (method.BaseCallArguments.Count > 0)
                {
                    Write("super(");
                    for (int i = 0; i < method.BaseCallArguments.Count; i++)
                    {
                        if (i > 0) Write(", ");
                        GenerateExpression(method.BaseCallArguments[i]);
                    }
                    WriteLine(");");
                }

                base.VisitMethod(method);
                Unindent();
                WriteLine("}");
                return;
            }

            if (method.IsAbstract)
            {
                var returnType = MapTypeToJava(method.ReturnType);
                var staticMod = method.IsStatic ? "static " : "";
                WriteLine($"{modifierStr}{staticMod}abstract {returnType} {method.Name}({parameters});");
                return;
            }

            if (method.IsEntryPoint)
            {
                WriteLine("public static void main(String[] args) {");
            }
            else
            {
                var returnType = MapTypeToJava(method.ReturnType);
                var staticMod = method.IsStatic ? "static " : "";
                if (method.IsOverride)
                    WriteLine("@Override");
                WriteLine($"{modifierStr}{staticMod}{returnType} {method.Name}({parameters}) {{");
            }
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

        protected override void VisitField(IRField field)
        {
            var type        = MapTypeToJava(field.Type);
            var modifier    = MapModifierToJava(field.AccessModifier);
            var modifierStr = string.IsNullOrEmpty(modifier) ? "" : $"{modifier} ";
            var staticMod   = field.IsStatic ? "static " : "";

            Write($"{modifierStr}{staticMod}{type} {field.Name}");
            if (field.InitialValue != null)
            {
                Write(" = ");
                GenerateExpression(field.InitialValue);
            }
            WriteLine(";");
        }

        protected override void VisitConsoleOutput(IRConsoleOutput output)
        {
            var method = output.NewLine ? "System.out.println" : "System.out.print";
            Write($"{method}(");
            if (output.Argument != null)
                GenerateExpression(output.Argument);
            WriteLine(");");
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
        protected override void VisitExpressionStatement(IRExpressionStatement exprStmt)
        {
            GenerateExpression(exprStmt.Expression);
            WriteLine(";");
        }

        protected override void VisitIf(IRIf ifStmt)
        {
            // 1. Escribir "if (condición) {"
            Write("if (");
            GenerateExpression(ifStmt.Condition);
            WriteLine(") {");
            
            // 2. Indentar y escribir el cuerpo del 'then'
            Indent();
            foreach (var stmt in ifStmt.ThenBranch)
            {
                VisitStatement(stmt);
            }
            Unindent();
            
            // 3. Si hay bloque 'else', generarlo
            if (ifStmt.ElseBranch != null && ifStmt.ElseBranch.Count > 0)
            {
                WriteLine("} else {");
                Indent();
                foreach (var stmt in ifStmt.ElseBranch)
                {
                    VisitStatement(stmt);
                }
                Unindent();
            }
            
            // 4. Cerrar el bloque
            WriteLine("}");
        }
        protected override void VisitWhile(IRWhile whileLoop)
        {
            Write("while (");
            GenerateExpression(whileLoop.Condition);
            WriteLine(") {");
            
            Indent();
            foreach (var stmt in whileLoop.Body)
            {
                VisitStatement(stmt);
            }
            Unindent();
            
            WriteLine("}");
        }

        protected override void VisitDoWhile(IRDoWhile doWhileLoop)
        {
            WriteLine("do {");
            Indent();
            foreach (var stmt in doWhileLoop.Body)
            {
                VisitStatement(stmt);
            }
            Unindent();

            Write("} while (");
            GenerateExpression(doWhileLoop.Condition);
            WriteLine(");");
        }

        protected override void VisitForeach(IRForeach foreachLoop)
        {
            var itemType = MapTypeToJava(foreachLoop.ItemType);

            Write($"for ({itemType} {foreachLoop.ItemName} : ");
            GenerateExpression(foreachLoop.Collection);
            WriteLine(") {");

            Indent();
            foreach (var stmt in foreachLoop.Body)
            {
                VisitStatement(stmt);
            }
            Unindent();

            WriteLine("}");
        }

        protected override void VisitSwitch(IRSwitch switchStmt)
        {
            Write("switch (");
            GenerateExpression(switchStmt.Expression);
            WriteLine(") {");
            Indent();

            foreach (var switchCase in switchStmt.Cases)
            {
                if (switchCase.IsDefault)
                {
                    WriteLine("default:");
                }
                else
                {
                    Write("case ");
                    GenerateExpression(switchCase.Label);
                    WriteLine(":");
                }

                Indent();
                foreach (var stmt in switchCase.Body)
                {
                    VisitStatement(stmt);
                }
                Unindent();
            }

            Unindent();
            WriteLine("}");
        }

        protected override void VisitBreak(IRBreak breakStmt)
        {
            WriteLine("break;");
        }

        protected override void VisitContinue(IRContinue continueStmt)
        {
            WriteLine("continue;");
        }

        protected override void VisitFor(IRFor forLoop)
        {
            Write("for (");
            
            // 1. Initializer
            if (forLoop.Initializer is IRVariableDeclaration decl)
            {
                var type = MapTypeToJava(decl.Type);
                Write($"{type} {decl.Name}");
                if (decl.InitialValue != null)
                {
                    Write(" = ");
                    GenerateExpression(decl.InitialValue);
                }
            }
            Write("; ");
            
            // 2. Condition
            if (forLoop.Condition != null) GenerateExpression(forLoop.Condition);
            Write("; ");
            
            // 3. Incrementor
            if (forLoop.Incrementor != null) GenerateExpression(forLoop.Incrementor);
            
            WriteLine(") {");
            
            Indent();
            foreach (var stmt in forLoop.Body)
            {
                VisitStatement(stmt);
            }
            Unindent();
            
            WriteLine("}");
        }
        
        private void GenerateExpression(IRExpression expression)
        {
            switch (expression)
            {
                case IRLiteral literal:
                    if (literal.Type == "string")
                        Write($"\"{literal.Value}\"");
                    else if (literal.Type == "bool")
                        Write(literal.Value.ToString().ToLower());
                    else
                        Write(literal.Value.ToString());
                    break;
                case IRVariable variable:
                    Write(variable.Name);
                    break;
                case IRBinaryOperation binary:
                    GenerateBinaryOperation(binary);
                    break;
                case IRUnaryOperation unary:
                    GenerateUnaryOperation(unary);
                    break;
                case IRConsoleInput input:
                    GenerateConsoleInput(input);
                    break;
                case IRArrayCreation arrayCreation:
                    GenerateArrayCreation(arrayCreation);
                    break;
                case IRArrayAccess arrayAccess:
                    GenerateArrayAccess(arrayAccess);
                    break;
                case IRArrayLength arrayLength:
                    GenerateArrayLength(arrayLength);
                    break;
                case IRMethodCall methodCall:
                    GenerateMethodCall(methodCall);
                    break;
                case IRListCreation listCreation:
                    GenerateListCreation(listCreation);
                    break;
                case IRObjectCreation objectCreation:
                    GenerateObjectCreation(objectCreation);
                    break;
                case IRTypeCheck typeCheck:
                    GenerateTypeCheck(typeCheck);
                    break;
                case IRCastExpression castExpr:
                    GenerateCastExpression(castExpr);
                    break;
            }
        }

        private void GenerateBinaryOperation(IRBinaryOperation binary)
        {
            GenerateExpression(binary.Left);
            Write($" {binary.Operation.ToJavaSymbol()} ");
            GenerateExpression(binary.Right);
        }
        private void GenerateUnaryOperation(IRUnaryOperation unary)
        {
            if (unary.Operation == IRUnaryOperationType.Increment || unary.Operation == IRUnaryOperationType.Decrement)
            {
                GenerateExpression(unary.Operand);
                Write(unary.ToSymbol());
            }
            else
            {
                Write("(");
                Write(unary.ToSymbol());
                GenerateExpression(unary.Operand);
                Write(")");
            }
        }
        private void GenerateConsoleInput(IRConsoleInput input)
        {
            var javaCode = input.Type switch
            {
                "int"    => "Integer.parseInt(scanner.nextLine())",
                "double" => "Double.parseDouble(scanner.nextLine())",
                "float"  => "Float.parseFloat(scanner.nextLine())",
                "long"   => "Long.parseLong(scanner.nextLine())",
                _        => "scanner.nextLine()"
            };
            Write(javaCode);
        }

        private void GenerateArrayCreation(IRArrayCreation arrayCreation)
        {
            var elementType = MapTypeToJava(arrayCreation.ElementType);

            if (arrayCreation.InitialValues != null && arrayCreation.InitialValues.Count > 0)
            {
                Write($"new {elementType}[] {{ ");
                for (int i = 0; i < arrayCreation.InitialValues.Count; i++)
                {
                    if (i > 0) Write(", ");
                    GenerateExpression(arrayCreation.InitialValues[i]);
                }
                Write(" }");
                return;
            }

            Write($"new {elementType}[");
            if (arrayCreation.SizeExpression != null)
            {
                GenerateExpression(arrayCreation.SizeExpression);
            }
            Write("]");
        }

        private void GenerateArrayAccess(IRArrayAccess arrayAccess)
        {
            GenerateExpression(arrayAccess.ArrayExpression);
            Write("[");
            GenerateExpression(arrayAccess.IndexExpression);
            Write("]");
        }

        private void GenerateArrayLength(IRArrayLength arrayLength)
        {
            GenerateExpression(arrayLength.ArrayExpression);
            Write(".length");
        }

        private void GenerateMethodCall(IRMethodCall methodCall)
        {
            Write($"{methodCall.MethodName}(");
            for (int i = 0; i < methodCall.Arguments.Count; i++)
            {
                if (i > 0) Write(", ");
                GenerateExpression(methodCall.Arguments[i]);
            }
            Write(")");
        }

        private void GenerateListCreation(IRListCreation listCreation)
        {
            var elementType = MapTypeToJavaBoxed(listCreation.ElementType);
            Write($"new ArrayList<{elementType}>()");
        }

        private void GenerateObjectCreation(IRObjectCreation objectCreation)
        {
            Write($"new {objectCreation.ClassName}(");
            for (int i = 0; i < objectCreation.Arguments.Count; i++)
            {
                if (i > 0) Write(", ");
                GenerateExpression(objectCreation.Arguments[i]);
            }
            Write(")");
        }

        private void GenerateTypeCheck(IRTypeCheck typeCheck)
        {
            GenerateExpression(typeCheck.Expression);
            Write($" instanceof {typeCheck.TargetType}");
        }

        private void GenerateCastExpression(IRCastExpression castExpr)
        {
            Write($"({MapTypeToJava(castExpr.TargetType)})(");
            GenerateExpression(castExpr.Expression);
            Write(")");
        }

        private bool ProgramNeedsListImports(IRProgram program) =>
            program.Classes.Any(c => ClassNeedsListImports(c));

        private bool ClassNeedsListImports(IRClass cls) =>
            cls.Fields.Any(f => IsListTypeName(f.Type))
            || cls.Methods.Any(m => MethodNeedsListImports(m));

        private bool MethodNeedsListImports(IRMethod method) =>
            IsListTypeName(method.ReturnType)
            || method.Parameters.Any(p => IsListTypeName(p.Type))
            || method.Body.Any(StatementNeedsListImports)
            || (method.ReturnExpression != null && ExpressionNeedsListImports(method.ReturnExpression));

        private bool StatementNeedsListImports(IRStatement stmt) => stmt switch
        {
            IRVariableDeclaration decl => IsListTypeName(decl.Type)
                || (decl.InitialValue != null && ExpressionNeedsListImports(decl.InitialValue)),
            IRAssignment assign => ExpressionNeedsListImports(assign.Value),
            IRExpressionStatement exprStmt => ExpressionNeedsListImports(exprStmt.Expression),
            IRIf ifStmt => ExpressionNeedsListImports(ifStmt.Condition)
                || ifStmt.ThenBranch.Any(StatementNeedsListImports)
                || ifStmt.ElseBranch.Any(StatementNeedsListImports),
            IRFor forStmt => (forStmt.Initializer != null && StatementNeedsListImports(forStmt.Initializer))
                || (forStmt.Condition != null && ExpressionNeedsListImports(forStmt.Condition))
                || (forStmt.Incrementor != null && ExpressionNeedsListImports(forStmt.Incrementor))
                || forStmt.Body.Any(StatementNeedsListImports),
            IRWhile whileStmt => ExpressionNeedsListImports(whileStmt.Condition)
                || whileStmt.Body.Any(StatementNeedsListImports),
            IRDoWhile doWhileStmt => ExpressionNeedsListImports(doWhileStmt.Condition)
                || doWhileStmt.Body.Any(StatementNeedsListImports),
            IRForeach foreachStmt => IsListTypeName(foreachStmt.ItemType)
                || ExpressionNeedsListImports(foreachStmt.Collection)
                || foreachStmt.Body.Any(StatementNeedsListImports),
            IRSwitch switchStmt => ExpressionNeedsListImports(switchStmt.Expression)
                || switchStmt.Cases.Any(c => (c.Label != null && ExpressionNeedsListImports(c.Label))
                    || c.Body.Any(StatementNeedsListImports)),
            _ => false
        };

        private bool ExpressionNeedsListImports(IRExpression expr) => expr switch
        {
            IRListCreation => true,
            IRMethodCall methodCall => methodCall.MethodName.Contains(".add")
                || methodCall.MethodName.Contains(".size")
                || methodCall.MethodName.Contains(".get")
                || methodCall.MethodName.Contains(".set"),
            IRBinaryOperation bin => ExpressionNeedsListImports(bin.Left) || ExpressionNeedsListImports(bin.Right),
            IRUnaryOperation unary => ExpressionNeedsListImports(unary.Operand),
            IRArrayCreation arrayCreation =>
                (arrayCreation.SizeExpression != null && ExpressionNeedsListImports(arrayCreation.SizeExpression))
                || arrayCreation.InitialValues.Any(ExpressionNeedsListImports),
            IRArrayAccess arrayAccess =>
                ExpressionNeedsListImports(arrayAccess.ArrayExpression)
                || ExpressionNeedsListImports(arrayAccess.IndexExpression),
            IRArrayLength arrayLength => ExpressionNeedsListImports(arrayLength.ArrayExpression),
            IRObjectCreation objCreation => objCreation.Arguments.Any(ExpressionNeedsListImports),
            IRTypeCheck typeCheck => ExpressionNeedsListImports(typeCheck.Expression),
            IRCastExpression castExpr => ExpressionNeedsListImports(castExpr.Expression),
            _ => false
        };

        private bool ProgramNeedsScanner(IRProgram program) =>
            program.Classes.Any(c => ClassNeedsScanner(c));

        private bool ClassNeedsScanner(IRClass cls) =>
            cls.Methods.Any(m => MethodNeedsScanner(m));

        private bool MethodNeedsScanner(IRMethod method) =>
            method.Body.Any(s => StatementNeedsScanner(s)) ||
            (method.ReturnExpression != null && ExpressionNeedsScanner(method.ReturnExpression));

        private bool StatementNeedsScanner(IRStatement stmt) => stmt switch
        {
            IRVariableDeclaration decl => decl.InitialValue != null && ExpressionNeedsScanner(decl.InitialValue),
            IRAssignment assign        => ExpressionNeedsScanner(assign.Value),
            IRExpressionStatement exprStmt => ExpressionNeedsScanner(exprStmt.Expression),
            IRConsoleOutput output     => output.Argument != null && ExpressionNeedsScanner(output.Argument),
            IRIf ifStmt => ExpressionNeedsScanner(ifStmt.Condition)
                || ifStmt.ThenBranch.Any(StatementNeedsScanner)
                || ifStmt.ElseBranch.Any(StatementNeedsScanner),
            IRFor forStmt => (forStmt.Initializer != null && StatementNeedsScanner(forStmt.Initializer))
                || (forStmt.Condition != null && ExpressionNeedsScanner(forStmt.Condition))
                || (forStmt.Incrementor != null && ExpressionNeedsScanner(forStmt.Incrementor))
                || forStmt.Body.Any(StatementNeedsScanner),
            IRWhile whileStmt => ExpressionNeedsScanner(whileStmt.Condition)
                || whileStmt.Body.Any(StatementNeedsScanner),
            IRDoWhile doWhileStmt => ExpressionNeedsScanner(doWhileStmt.Condition)
                || doWhileStmt.Body.Any(StatementNeedsScanner),
            IRForeach foreachStmt => ExpressionNeedsScanner(foreachStmt.Collection)
                || foreachStmt.Body.Any(StatementNeedsScanner),
            IRSwitch switchStmt => ExpressionNeedsScanner(switchStmt.Expression)
                || switchStmt.Cases.Any(c => (c.Label != null && ExpressionNeedsScanner(c.Label))
                    || c.Body.Any(StatementNeedsScanner)),
            _                         => false
        };

        private bool ExpressionNeedsScanner(IRExpression expr) => expr switch
        {
            IRConsoleInput                => true,
            IRBinaryOperation bin         => ExpressionNeedsScanner(bin.Left) || ExpressionNeedsScanner(bin.Right),
            IRUnaryOperation unary        => ExpressionNeedsScanner(unary.Operand),
            IRArrayCreation arrayCreation =>
                (arrayCreation.SizeExpression != null && ExpressionNeedsScanner(arrayCreation.SizeExpression))
                || arrayCreation.InitialValues.Any(ExpressionNeedsScanner),
            IRArrayAccess arrayAccess =>
                ExpressionNeedsScanner(arrayAccess.ArrayExpression)
                || ExpressionNeedsScanner(arrayAccess.IndexExpression),
            IRArrayLength arrayLength =>
                ExpressionNeedsScanner(arrayLength.ArrayExpression),
            IRMethodCall methodCall =>
                methodCall.Arguments.Any(ExpressionNeedsScanner),
            IRObjectCreation objCreation => objCreation.Arguments.Any(ExpressionNeedsScanner),
            IRTypeCheck typeCheck => ExpressionNeedsScanner(typeCheck.Expression),
            IRCastExpression castExpr => ExpressionNeedsScanner(castExpr.Expression),
            _                            => false
        };

        private string MapTypeToJava(string csharpType)
        {
            if (IsListTypeName(csharpType))
            {
                var innerType = csharpType.Trim().Substring(5, csharpType.Trim().Length - 6).Trim();
                return $"List<{MapTypeToJavaBoxed(innerType)}>";
            }

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

        private string MapTypeToJavaBoxed(string csharpType)
        {
            if (IsListTypeName(csharpType))
            {
                return MapTypeToJava(csharpType);
            }

            return csharpType switch
            {
                "int" => "Integer",
                "double" => "Double",
                "float" => "Float",
                "long" => "Long",
                "string" => "String",
                "bool" => "Boolean",
                _ => MapTypeToJava(csharpType)
            };
        }

        private bool IsListTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return false;
            var trimmed = typeName.Trim();
            return trimmed.StartsWith("List<", StringComparison.Ordinal) && trimmed.EndsWith(">", StringComparison.Ordinal);
        }

        private string MapModifierToJava(IRAccessModifier modifier)
        {
            return modifier switch
            {
                IRAccessModifier.Public             => "public",
                IRAccessModifier.Private            => "private",
                IRAccessModifier.Protected          => "protected",
                IRAccessModifier.Internal           => "",           // package-private
                IRAccessModifier.ProtectedInternal  => "protected",  // equivalente más cercano en Java
                IRAccessModifier.PrivateProtected   => "private",    // sin equivalente exacto en Java
                _ => ""
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