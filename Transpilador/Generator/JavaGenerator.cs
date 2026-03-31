using System;
using System.Linq;
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

            if (ProgramNeedsScanner(program))
            {
                WriteLine("import java.util.Scanner;");
                WriteLine();
            }

            VisitProgram(program);

            return _sb.ToString();
        }

        public override void VisitClass(IRClass irClass)
        {
            var modifier = MapModifierToJava(irClass.AccessModifier);
            var modifierStr = string.IsNullOrEmpty(modifier) ? "" : $"{modifier} ";
            WriteLine($"{modifierStr}class {irClass.Name} {{");
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
            if (method.IsEntryPoint)
            {
                WriteLine("public static void main(String[] args) {");
            }
            else
            {
                var returnType = MapTypeToJava(method.ReturnType);
                var modifier = MapModifierToJava(method.AccessModifier);
                var modifierStr = string.IsNullOrEmpty(modifier) ? "" : $"{modifier} ";
                var staticMod = method.IsStatic ? "static " : "";
                WriteLine($"{modifierStr}{staticMod}{returnType} {method.Name}() {{");
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
        // Agregar este método en la clase JavaGenerator, 
// después del método VisitAssignment

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
            IRConsoleOutput output     => output.Argument != null && ExpressionNeedsScanner(output.Argument),
            _                         => false
        };

        private bool ExpressionNeedsScanner(IRExpression expr) => expr switch
        {
            IRConsoleInput                => true,
            IRBinaryOperation bin         => ExpressionNeedsScanner(bin.Left) || ExpressionNeedsScanner(bin.Right),
            IRUnaryOperation unary        => ExpressionNeedsScanner(unary.Operand),
            _                            => false
        };

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