using System;

class Conversor
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Conversor de Unidades ===");

        Console.WriteLine("Ingresa los centimetros:");
        int cm = int.Parse(Console.ReadLine());

        int metros  = cm / 100;
        int sobrante = cm % 100;

        Console.Write("Metros enteros: ");
        Console.WriteLine(metros);
        Console.Write("Centimetros sobrantes: ");
        Console.WriteLine(sobrante);

        Console.WriteLine("Conversion completada.");
    }
}
