using System;

public class Banco
{
    public int saldo = 5000;
    private int pin = 1234;
    protected int limiteCredito = 10000;
    internal int codigoSucursal = 42;
    public static int totalCuentas = 0;

    public int ConsultarSaldo()
    {
        return saldo;
    }

    private int CalcularInteres()
    {
        int tasa    = 3;
        int interes = saldo * tasa;
        return interes;
    }

    protected int LimiteDisponible()
    {
        int disponible = limiteCredito - saldo;
        return disponible;
    }

    internal int ObtenerCodigo()
    {
        return codigoSucursal;
    }

    protected internal int ResumenCuenta()
    {
        int resumen = saldo + limiteCredito;
        return resumen;
    }

    private protected int ValidarPin()
    {
        int intentos  = 3;
        int restantes = intentos - 1;
        return restantes;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("=== Prueba 03: Modificadores de Acceso ===");

        int saldo         = 5000;
        int pin           = 1234;
        int limiteCredito = 10000;
        int codigoSucursal = 42;
        int totalCuentas  = 0;

        Console.Write("Saldo (public): ");
        Console.WriteLine(saldo);
        Console.Write("PIN (private): ");
        Console.WriteLine(pin);
        Console.Write("Limite de credito (protected): ");
        Console.WriteLine(limiteCredito);
        Console.Write("Codigo sucursal (internal): ");
        Console.WriteLine(codigoSucursal);
        Console.Write("Total cuentas (public static): ");
        Console.WriteLine(totalCuentas);

        int tasa       = 3;
        int interes    = saldo * tasa;
        int disponible = limiteCredito - saldo;
        int resumen    = saldo + limiteCredito;
        int intentos   = 3;
        int restantes  = intentos - 1;

        Console.Write("Interes calculado (private): ");
        Console.WriteLine(interes);
        Console.Write("Limite disponible (protected): ");
        Console.WriteLine(disponible);
        Console.Write("Resumen cuenta (protected internal): ");
        Console.WriteLine(resumen);
        Console.Write("Intentos restantes (private protected): ");
        Console.WriteLine(restantes);
    }
}
