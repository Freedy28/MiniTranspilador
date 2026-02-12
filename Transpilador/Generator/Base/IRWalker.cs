using Transpilador.Models.Base;
using Transpilador.Models.Statements;
using Transpilador.Models.Structure;

namespace Transpilador.Generator.Base
{
    // Clase abstracta: Es una plantilla que otros heredarán
    public abstract class IRWalker
    {
        // 1. Recorre las clases del programa
        public virtual void VisitProgram(IRProgram program)
        {
            foreach (var irClass in program.Classes)
            {
                VisitClass(irClass);
            }
        }

        // 2. Recorre los métodos de una clase
        public virtual void VisitClass(IRClass irClass)
        {
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
            }
        }

        // 5. Métodos vacíos que los hijos (como JavaGenerator) van a sobrescribir
        protected virtual void VisitVariableDeclaration(IRVariableDeclaration decl) { }
        protected virtual void VisitAssignment(IRAssignment assignment) { }
    }
}