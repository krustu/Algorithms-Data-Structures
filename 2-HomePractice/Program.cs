using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        SelfTest();

        if (args.Length > 0 && args[0] == "calibrate") { Calibrate(); return; }

        int nMin = int.Parse(args[0]);
        int nMax = int.Parse(args[1]);
        RunExperiments(nMin, nMax);
    }

    public static int SinglePass(int[] array) => SinglePass(array, array.Length);
    public static int FourPass(int[] array) => FourPass(array, array.Length);

    // length = сколько первых элементов массива считать данными
    public static int SinglePass(int[] array, int length)
    {
        if (array == null || length < 4)
            throw new ArgumentException("Array must contain at least 4 elements.");

        int first = int.MinValue, second = int.MinValue,
            third = int.MinValue, fourth = int.MinValue;

        for (int i = 0; i < length; i++)
        {
            int number = array[i];
            if (number > first)
            {
                fourth = third; third = second; second = first; first = number;
            }
            else if (number > second)
            {
                fourth = third; third = second; second = number;
            }
            else if (number > third)
            {
                fourth = third; third = number;
            }
            else if (number > fourth)
            {
                fourth = number;
            }
        }
        return fourth;
    }

    // Изменяет массив (переставляет элементы)
    public static int FourPass(int[] array, int length)
    {
        if (array == null || length < 4)
            throw new ArgumentException("Array must contain at least 4 elements.");

        int result = 0;
        for (int j = 0; j < 4; j++)
        {
            int max = array[j];
            int index = j;
            for (int a = j + 1; a < length; a++)
            {
                if (array[a] > max) { max = array[a]; index = a; }
            }
            (array[j], array[index]) = (array[index], array[j]);
            result = array[j];
        }
        return result;
    }

    static void Fill(int[] data, int length, Random rnd)
    {
        for (int i = 0; i < length; i++) data[i] = rnd.Next();
    }

    static double TimeSingle(int[] data, int n, out int res)
    {
        var sw = Stopwatch.StartNew();
        res = SinglePass(data, n);
        sw.Stop();
        return sw.Elapsed.TotalSeconds;
    }

    static double TimeFour(int[] data, int n, out int res)
    {
        var sw = Stopwatch.StartNew();
        res = FourPass(data, n);
        sw.Stop();
        return sw.Elapsed.TotalSeconds;
    }

    static void SelfTest()
    {
        var rnd = new Random(1);
        for (int t = 0; t < 1000; t++)
        {
            int n = rnd.Next(4, 200);
            var arr = Enumerable.Range(0, n).Select(_ => rnd.Next(0, 50)).ToArray();
            int expected = arr.OrderByDescending(x => x).ElementAt(3);
            if (SinglePass(arr) != expected || FourPass((int[])arr.Clone()) != expected)
                throw new Exception("Self-test FAILED");
        }
        Console.WriteLine("Self-test OK");
    }

    static void Calibrate()
    {
        var rnd = new Random(42);
        var data = new int[1_400_000_000];
        for (int n = 100_000_000; n <= 1_400_000_000; n += 300_000_000)
        {
            Fill(data, n, rnd);
            double ts = TimeSingle(data, n, out int a);
            double tf = TimeFour(data, n, out int b);
            Console.WriteLine($"N={n,14:N0}  single={ts:F3}s  four={tf:F3}s  ok={a == b}");
        }
    }

    static void RunExperiments(int nMin, int nMax)
    {
        var rnd = new Random();
        var data = new int[nMax];              // ОДНО выделение памяти на весь прогон

        using var csv = new StreamWriter("results.csv");
        csv.WriteLine("N,run,single_s,four_s");
        var summary = new System.Text.StringBuilder();
        summary.AppendLine("N,single_mean,single_std,four_mean,four_std");

        for (int p = 0; p < 5; p++)
        {
            int n = nMin + (int)((long)(nMax - nMin) * p / 4);
            var ts = new double[10];
            var tf = new double[10];
            for (int r = 0; r < 10; r++)
            {
                Fill(data, n, rnd);                    // новые числа каждый раз, вне замера
                ts[r] = TimeSingle(data, n, out int a);
                tf[r] = TimeFour(data, n, out int b);
                if (a != b) throw new Exception("Results differ!");
                csv.WriteLine($"{n},{r + 1},{ts[r]:F6},{tf[r]:F6}");
                csv.Flush();                           // чтобы ничего не потерялось при сбое
                Console.WriteLine($"N={n:N0} run {r + 1}: single={ts[r]:F3}s four={tf[r]:F3}s");
            }
            summary.AppendLine($"{n},{ts.Average():F6},{Std(ts):F6},{tf.Average():F6},{Std(tf):F6}");
            File.WriteAllText("summary.csv", summary.ToString());
        }
        Console.WriteLine(summary);
    }

    static double Std(double[] x)
    {
        double m = x.Average();
        return Math.Sqrt(x.Sum(v => (v - m) * (v - m)) / (x.Length - 1));
    }
}