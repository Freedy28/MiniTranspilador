class EjemploParametros {
    public static int Sumar(int a, int b) {
        int resultado = (a + b);
        return resultado;
    }
    public static int Multiplicar(int x, int y) {
        int resultado = (x * y);
        return resultado;
    }
    public static void main(String[] args) {
        int suma = Sumar(2, 3);
        int producto = Multiplicar(4, 5);
        System.out.println(suma);
        System.out.println(producto);
    }
}
