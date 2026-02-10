public class Calculator {
    public int Calculate() {
        int a = 10;
        int b = 5;
        int suma = (a + b);
        if ((suma > 10)) {
            suma = (suma * 2);
        }
        else {
            suma = (suma + 1);
        }
        for (int i = 0; (i < 3); i++) {
            suma = (suma + i);
        }
        int counter = 0;
        while ((counter < 5)) {
            counter++;
        }
        return suma;
    }

}
