// CASO NO SOPORTADO: Bucle while
// Error esperado: Expresión no soportada: WhileStatementSyntax
//
// El transpilador actualmente no tiene IRWhileLoop ni VisitWhileLoop.

class EjemploWhile
{
    public int Contar()
    {
        int contador = 0;

        while (contador < 5)
        {
            contador = contador + 1;
        }

        return contador;
    }
}
