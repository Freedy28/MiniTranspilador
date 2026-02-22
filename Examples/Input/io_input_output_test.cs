using System;

class Calculadora
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Ingresa el primer numero:");
        int a = int.Parse(Console.ReadLine());

        Console.WriteLine("Ingresa el segundo numero:");
        int b = int.Parse(Console.ReadLine());

        int suma = a + b;
        int resta = a - b;

        Console.WriteLine("Suma:");
        Console.WriteLine(suma);
        Console.WriteLine("Resta:");
        Console.WriteLine(resta);
    }
}
