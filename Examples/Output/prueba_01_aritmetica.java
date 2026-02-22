public class Aritmetica {
    public int OperarEnteros() {
        int a = 20;
        int b = 6;
        int c = 3;
        int suma = (a + b);
        int resta = (a - b);
        int multi = (a * b);
        int div = (a / b);
        int mod = (a % b);
        int paso1 = (suma + resta);
        int paso2 = (multi - div);
        int resultado = ((paso1 * paso2) + mod);
        return resultado;
    }
    public int OperarNegativos() {
        int x = 10;
        int negX = (-x);
        int doble = (negX * 2);
        int resultado = (doble + 5);
        return resultado;
    }
    public static void main(String[] args) {
        System.out.println("=== Prueba 01: Aritmetica ===");
        int a = 20;
        int b = 6;
        int suma = (a + b);
        int resta = (a - b);
        int multi = (a * b);
        int div = (a / b);
        int mod = (a % b);
        System.out.print("Suma (20+6): ");
        System.out.println(suma);
        System.out.print("Resta (20-6): ");
        System.out.println(resta);
        System.out.print("Multiplicacion (20*6): ");
        System.out.println(multi);
        System.out.print("Division (20/6): ");
        System.out.println(div);
        System.out.print("Modulo (20%6): ");
        System.out.println(mod);
        int paso1 = (suma + resta);
        int paso2 = (multi - div);
        int resultado = ((paso1 * paso2) + mod);
        System.out.print("Expresion combinada: ");
        System.out.println(resultado);
        int x = 10;
        int negX = (-x);
        int doble = (negX * 2);
        System.out.print("Negacion de 10: ");
        System.out.println(negX);
        System.out.print("Doble del negativo: ");
        System.out.println(doble);
    }
}

