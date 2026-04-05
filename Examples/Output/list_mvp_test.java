import java.util.ArrayList;
import java.util.List;

class ListDemo {
    public static int SumarLista(List<Integer> nums) {
        int total = 0;
        for (int i = 0; i < nums.size(); i++) {
            total = (total + nums.get(i));
        }
        return total;
    }

    public static void main(String[] args) {
        List<Integer> nums = new ArrayList<Integer>();
        nums.add(10);
        nums.add(20);
        nums.add(30);
        nums.set(1, 25);
        int total = SumarLista(nums);
        System.out.println(total);
    }
}
