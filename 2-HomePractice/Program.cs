using System;
class Program
{
    static void Main()
    {
        int[] nums = { 5, 12, 3, 8, 20, 1, 15, 9};
        // 20 , 15, 12 , 9

        var answer = FourPass(nums);
        Console.WriteLine(answer);
    }
    public static int FourPass(int[] array)
    {
        if(array == null || array.Length <= 4)
        throw new Exception("Array must contain at least 4 elements.");

        int result = 0;
        for (int j = 0 ; j < 4; j++)
        {
            int Max = array[j];
            int Index = j;
            for(int a = j + 1; a < array.Length ; a++)
            {

                if(array[a] > Max)
                {
                    Max = array[a]; 
                    Index = a;
                }
                
            }
            (array[j], array[Index]) = (array[Index], array[j]);
            result = array[j];
            
        }
        return result;
    }
}