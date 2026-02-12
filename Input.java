public class MiCalculadora {
    public int Calcular() {
        int numero1 = 20;
        int numero2 = 4;
        int suma = (numero1 + numero2);
        int resta = (numero1 - numero2);
        int multiplicacion = (numero1 * numero2);
        int division = (numero1 / numero2);
        int resultado = ((suma + (resta * multiplicacion)) - division);
        return resultado;
    }

}
