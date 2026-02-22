public class Banco {
    public int saldo = 5000;
    private int pin = 1234;
    protected int limiteCredito = 10000;
    int codigoSucursal = 42;
    public static int totalCuentas = 0;
    public int ConsultarSaldo() {
        return saldo;
    }
    private int CalcularInteres() {
        int tasa = 3;
        int interes = (saldo * tasa);
        return interes;
    }
    protected int LimiteDisponible() {
        int disponible = (limiteCredito - saldo);
        return disponible;
    }
    int ObtenerCodigo() {
        return codigoSucursal;
    }
    protected int ResumenCuenta() {
        int resumen = (saldo + limiteCredito);
        return resumen;
    }
    private int ValidarPin() {
        int intentos = 3;
        int restantes = (intentos - 1);
        return restantes;
    }
    public static void main(String[] args) {
        System.out.println("=== Prueba 03: Modificadores de Acceso ===");
        int saldo = 5000;
        int pin = 1234;
        int limiteCredito = 10000;
        int codigoSucursal = 42;
        int totalCuentas = 0;
        System.out.print("Saldo (public): ");
        System.out.println(saldo);
        System.out.print("PIN (private): ");
        System.out.println(pin);
        System.out.print("Limite de credito (protected): ");
        System.out.println(limiteCredito);
        System.out.print("Codigo sucursal (internal): ");
        System.out.println(codigoSucursal);
        System.out.print("Total cuentas (public static): ");
        System.out.println(totalCuentas);
        int tasa = 3;
        int interes = (saldo * tasa);
        int disponible = (limiteCredito - saldo);
        int resumen = (saldo + limiteCredito);
        int intentos = 3;
        int restantes = (intentos - 1);
        System.out.print("Interes calculado (private): ");
        System.out.println(interes);
        System.out.print("Limite disponible (protected): ");
        System.out.println(disponible);
        System.out.print("Resumen cuenta (protected internal): ");
        System.out.println(resumen);
        System.out.print("Intentos restantes (private protected): ");
        System.out.println(restantes);
    }
}

