public class CuentaBancaria
{
    public int saldo = 1000;
    private int comision = 5;
    protected int interes = 3;
    internal int limite = 10000;
    public static int totalCuentas = 0;

    public int ObtenerSaldo()
    {
        return saldo;
    }

    private int CalcularComision()
    {
        int resultado = saldo * comision;
        return resultado;
    }

    protected int CalcularInteres()
    {
        int resultado = saldo * interes;
        return resultado;
    }

    internal int ObtenerLimite()
    {
        return limite;
    }
}
