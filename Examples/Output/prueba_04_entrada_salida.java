import java.util.Scanner;

class Conversor {
    private static Scanner scanner = new Scanner(System.in);
    public static void main(String[] args) {
        System.out.println("=== Conversor de Unidades ===");
        System.out.println("Ingresa los centimetros:");
        int cm = Integer.parseInt(scanner.nextLine());
        int metros = (cm / 100);
        int sobrante = (cm % 100);
        System.out.print("Metros enteros: ");
        System.out.println(metros);
        System.out.print("Centimetros sobrantes: ");
        System.out.println(sobrante);
        System.out.println("Conversion completada.");
    }
}

