// CASO NO SOPORTADO: Arreglos
// Error esperado: Expresión no soportada: ArrayCreationExpressionSyntax
//
// El transpilador no tiene soporte para declarar ni acceder a arreglos.
// En Java el equivalente de int[] nums = new int[5] es idéntico.

class EjemploArreglos
{
    public int ObtenerPrimero()
    {
        int[] numeros = new int[] { 10, 20, 30, 40, 50 };
        int primero = numeros[0];
        return primero;
    }
}
