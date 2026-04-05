using System;

class ArraysLengthDemo
{
    public static void Main(string[] args)
    {
        int[] valores = { 4, 7, 9, 12 };
        int suma = 0;

        for (int i = 0; i < valores.Length; i++)
        {
            suma = suma + valores[i];
        }

        bool largo = valores.Length > 3;

        Console.WriteLine(suma);
        Console.WriteLine(largo);
    }
}
