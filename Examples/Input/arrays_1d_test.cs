using System;

class ArrayDemo
{
    public static void Main(string[] args)
    {
        int[] a = new int[3];
        a[0] = 7;
        a[1] = 8;
        a[2] = 9;

        int[] b = { 10, 20, 30 };
        int first = b[0];

        Console.WriteLine(first);
        Console.WriteLine(a[1]);
    }
}
