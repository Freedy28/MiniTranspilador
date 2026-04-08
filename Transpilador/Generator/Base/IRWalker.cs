using Transpilador.Models.Base;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Generator.Base
{
    // Clase abstracta: Es una plantilla que otros heredarán
    public abstract class IRWalker
    {
        // 1. Recorre las interfaces y clases del programa
        public virtual void VisitProgram(IRProgram program)
        {
            foreach (var irInterface in program.Interfaces)
            {
                VisitInterface(irInterface);
            }
            foreach (var irClass in program.Classes)
            {
                VisitClass(irClass);
            }
        }

        // 1b. Recorre los métodos de una interfaz
        public virtual void VisitInterface(IRInterface irInterface)
        {
            foreach (var method in irInterface.Methods)
            {
                VisitMethod(method);
            }
        }

        // 2. Recorre los campos y métodos de una clase
        public virtual void VisitClass(IRClass irClass)
        {
            foreach (var field in irClass.Fields)
            {
                VisitField(field);
            }
            foreach (var method in irClass.Methods)
            {
                VisitMethod(method);
            }
        }

        // 3. Recorre las instrucciones (Body) de un método
        public virtual void VisitMethod(IRMethod method)
        {
            foreach (var stmt in method.Body)
            {
                VisitStatement(stmt);
            }
        }

        // 4. Actúa como un "semáforo" (Dispatcher) para saber qué instrucción es
        protected virtual void VisitStatement(IRStatement stmt)
        {
            switch (stmt)
            {
                case IRVariableDeclaration decl:
                    VisitVariableDeclaration(decl);
                    break;
                case IRAssignment assign:
                    VisitAssignment(assign);
                    break;
                case IRExpressionStatement exprStmt:
                    VisitExpressionStatement(exprStmt);
                    break;  
                case IRConsoleOutput output:
                    VisitConsoleOutput(output);
                    break;
                case IRIf ifStmt:
                    VisitIf(ifStmt);
                    break;
                case IRFor forStmt:
                    VisitFor(forStmt);
                    break;
                case IRWhile whileStmt: 
                    VisitWhile(whileStmt);    
                    break;
                case IRDoWhile doWhileStmt:
                    VisitDoWhile(doWhileStmt);
                    break;
                case IRForeach foreachStmt:
                    VisitForeach(foreachStmt);
                    break;
                case IRSwitch switchStmt:
                    VisitSwitch(switchStmt);
                    break;
                case IRBreak breakStmt:
                    VisitBreak(breakStmt);
                    break;
                case IRContinue continueStmt:
                    VisitContinue(continueStmt);
                    break;
                case IRTryCatch tryCatch:
                    VisitTryCatch(tryCatch);
                    break;
                case IRThrow throwStmt:
                    VisitThrow(throwStmt);
                    break;
                case IRReturn returnStmt:
                    VisitReturn(returnStmt);
                    break;
            }
        }

        // 5. Métodos vacíos que los hijos (como JavaGenerator) van a sobrescribir
        protected virtual void VisitField(IRField field) { }
        protected virtual void VisitVariableDeclaration(IRVariableDeclaration decl) { }
        protected virtual void VisitAssignment(IRAssignment assignment) { }
        protected virtual void VisitExpressionStatement(IRExpressionStatement exprStmt) { }
        protected virtual void VisitConsoleOutput(IRConsoleOutput output) { }
        protected virtual void VisitIf(IRIf ifStmt) { }
        protected virtual void VisitFor(IRFor forStmt) { }
        protected virtual void VisitWhile(IRWhile whileStmt) { }
        protected virtual void VisitDoWhile(IRDoWhile doWhileStmt) { }
        protected virtual void VisitForeach(IRForeach foreachStmt) { }
        protected virtual void VisitSwitch(IRSwitch switchStmt) { }
        protected virtual void VisitBreak(IRBreak breakStmt) { }
        protected virtual void VisitContinue(IRContinue continueStmt) { }
        protected virtual void VisitTryCatch(IRTryCatch tryCatch) { }
        protected virtual void VisitThrow(IRThrow throwStmt) { }
        protected virtual void VisitReturn(IRReturn returnStmt) { }
    }
}