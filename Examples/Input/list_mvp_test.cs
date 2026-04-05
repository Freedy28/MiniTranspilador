using System;
using System.Collections.Generic;

class ListDemo
{
    public static int SumarLista(List<int> nums)
    {
        int total = 0;
        for (int i = 0; i < nums.Count; i++)
        {
            total = total + nums[i];
        }
        return total;
    }

    public static void Main(string[] args)
    {
        List<int> nums = new List<int>();
        nums.Add(10);
        nums.Add(20);
        nums.Add(30);
        nums[1] = 25;

        int total = SumarLista(nums);
        Console.WriteLine(total);
    }
}
