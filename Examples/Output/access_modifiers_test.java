public class CuentaBancaria {
    public int ObtenerSaldo() {
        int saldo = 1000;
        return saldo;
    }
    private int CalcularComision() {
        int base1 = 100;
        int tasa = 5;
        int comision = (base1 * tasa);
        return comision;
    }
    protected int CalcularInteres() {
        int capital = 5000;
        int tasa = 3;
        int interes = (capital * tasa);
        return interes;
    }
    int ObtenerLimite() {
        int limite = 10000;
        return limite;
    }
}

