// CASO NO SOPORTADO: Bucle for
// Error esperado: Expresión no soportada: ForStatementSyntax
//
// El transpilador actualmente no tiene IRForLoop ni VisitForLoop.
// Para soportarlo habría que agregar:
//   1. Models/Statements/IRForLoop.cs
//   2. VisitForLoop en IRWalker
//   3. Detección en CSharpParser.VisitForStatement
//   4. Generación en JavaGenerator.VisitForLoop

class EjemploFor
{
    public int Sumar()
    {
        int total = 0;

        for (int i = 0; i < 10; i++)
        {
            total = total + i;
        }

        return total;
    }
}
