// CASO NO SOPORTADO: Sentencias if/else
// Error esperado: Expresión no soportada: IfStatementSyntax
//
// El transpilador actualmente no tiene IRIfStatement ni VisitIfStatement.
// Para soportarlo habría que agregar:
//   1. Models/Statements/IRIfStatement.cs
//   2. VisitIfStatement en IRWalker
//   3. Detección en CSharpParser.VisitIfStatement
//   4. Generación en JavaGenerator.VisitIfStatement

class EjemploIf
{
    public int Clasificar()
    {
        int x = 10;

        if (x > 5)
        {
            int resultado = x * 2;
        }
        else
        {
            int resultado = x + 1;
        }

        return x;
    }
}
