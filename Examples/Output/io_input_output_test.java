import java.util.Scanner;

class Calculadora {
    private static Scanner scanner = new Scanner(System.in);
    public static void main(String[] args) {
        System.out.println("Ingresa el primer numero:");
        int a = Integer.parseInt(scanner.nextLine());
        System.out.println("Ingresa el segundo numero:");
        int b = Integer.parseInt(scanner.nextLine());
        int suma = (a + b);
        int resta = (a - b);
        System.out.println("Suma:");
        System.out.println(suma);
        System.out.println("Resta:");
        System.out.println(resta);
    }
}

