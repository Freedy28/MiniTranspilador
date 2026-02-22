using System;

public class LogicaBooleana
{
    public bool EvaluarComparaciones()
    {
        int puntos = 85;
        int minimo = 60;
        int maximo = 100;

        bool aprobado    = puntos >= minimo;
        bool excelente   = puntos >= 90;
        bool valido      = puntos <= maximo;
        bool noExcelente = !excelente;

        bool resultado = aprobado && valido;
        bool destacado = excelente || aprobado;
        bool combinado = resultado && !noExcelente;

        return combinado;
    }

    public bool ComparacionesNumericas()
    {
        int x = 15;
        int y = 15;
        int z = 20;

        bool igual      = x == y;
        bool diferente  = x != z;
        bool menor      = x < z;
        bool mayor      = z > y;
        bool mayorIgual = x >= y;
        bool menorIgual = x <= z;

        bool parcial = igual && diferente;
        bool todo    = parcial && menor && mayor;

        return todo;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("=== Prueba 02: Comparaciones y Logica ===");

        int a = 15;
        int b = 10;

        bool mayor      = a > b;
        bool menor      = a < b;
        bool igual      = a == b;
        bool diferente  = a != b;
        bool mayorIgual = a >= b;
        bool menorIgual = a <= b;

        Console.Write("15 > 10: ");
        Console.WriteLine(mayor);
        Console.Write("15 < 10: ");
        Console.WriteLine(menor);
        Console.Write("15 == 10: ");
        Console.WriteLine(igual);
        Console.Write("15 != 10: ");
        Console.WriteLine(diferente);
        Console.Write("15 >= 10: ");
        Console.WriteLine(mayorIgual);
        Console.Write("15 <= 10: ");
        Console.WriteLine(menorIgual);

        bool and1 = mayor && diferente;
        bool or1  = menor || igual;
        bool not1 = !igual;
        bool comp = and1 || or1;

        Console.Write("AND (mayor && diferente): ");
        Console.WriteLine(and1);
        Console.Write("OR  (menor || igual): ");
        Console.WriteLine(or1);
        Console.Write("NOT (!igual): ");
        Console.WriteLine(not1);
        Console.Write("Combinado (and1 || or1): ");
        Console.WriteLine(comp);
    }
}
