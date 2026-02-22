using System;

public class Aritmetica
{
    public int OperarEnteros()
    {
        int a = 20;
        int b = 6;
        int c = 3;

        int suma  = a + b;
        int resta = a - b;
        int multi = a * b;
        int div   = a / b;
        int mod   = a % b;

        int paso1     = suma + resta;
        int paso2     = multi - div;
        int resultado = paso1 * paso2 + mod;

        return resultado;
    }

    public int OperarNegativos()
    {
        int x         = 10;
        int negX      = -x;
        int doble     = negX * 2;
        int resultado = doble + 5;
        return resultado;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("=== Prueba 01: Aritmetica ===");

        int a = 20;
        int b = 6;

        int suma  = a + b;
        int resta = a - b;
        int multi = a * b;
        int div   = a / b;
        int mod   = a % b;

        Console.Write("Suma (20+6): ");
        Console.WriteLine(suma);
        Console.Write("Resta (20-6): ");
        Console.WriteLine(resta);
        Console.Write("Multiplicacion (20*6): ");
        Console.WriteLine(multi);
        Console.Write("Division (20/6): ");
        Console.WriteLine(div);
        Console.Write("Modulo (20%6): ");
        Console.WriteLine(mod);

        int paso1     = suma + resta;
        int paso2     = multi - div;
        int resultado = paso1 * paso2 + mod;

        Console.Write("Expresion combinada: ");
        Console.WriteLine(resultado);

        int x    = 10;
        int negX = -x;
        int doble = negX * 2;

        Console.Write("Negacion de 10: ");
        Console.WriteLine(negX);
        Console.Write("Doble del negativo: ");
        Console.WriteLine(doble);
    }
}
