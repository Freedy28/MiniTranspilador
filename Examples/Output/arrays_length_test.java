class ArraysLengthDemo {
    public static void main(String[] args) {
        int[] valores = new int[] { 4, 7, 9, 12 };
        int suma = 0;
        for (int i = 0; i < valores.length; i++) {
            suma = (suma + valores[i]);
        }
        boolean largo = (valores.length > 3);
        System.out.println(suma);
        System.out.println(largo);
    }
}
