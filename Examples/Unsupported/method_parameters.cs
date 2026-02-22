// CASO PARCIALMENTE SOPORTADO: Parámetros en métodos
// Comportamiento actual: los parámetros se omiten en la firma Java generada.
//
// C# genera:   public int Sumar(int a, int b)
// Java genera: public int Sumar()              ← parámetros perdidos
//
// Dentro del cuerpo, las variables a y b son tratadas como identificadores
// desconocidos y no generan error, pero el Java resultante no compila.

class EjemploParametros
{
    public int Sumar(int a, int b)
    {
        int resultado = a + b;
        return resultado;
    }

    public int Multiplicar(int x, int y)
    {
        int resultado = x * y;
        return resultado;
    }
}
