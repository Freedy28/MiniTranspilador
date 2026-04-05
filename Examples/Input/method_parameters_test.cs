using System;

class EjemploParametros
{
    public static int Sumar(int a, int b)
    {
        int resultado = a + b;
        return resultado;
    }

    public static int Multiplicar(int x, int y)
    {
        int resultado = x * y;
        return resultado;
    }

    public static void Main(string[] args)
    {
        int suma = Sumar(2, 3);
        int producto = Multiplicar(4, 5);
        Console.WriteLine(suma);
        Console.WriteLine(producto);
    }
}
