public class Calculator {
    public int Calculate() {
        int a = 10;
        int b = 5;
        int suma = (a + b);
        int resta = (a - b);
        int multiplicacion = (a * b);
        int division = (a / b);
        return ((suma + (resta * multiplicacion)) - division);
    }

}
