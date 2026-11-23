namespace SortingAlgorithmsResearch;

public static class Statistics
{
    public static double Average(IEnumerable<double> values)
    {
        double[] data = values.ToArray();
        if (data.Length == 0)
            return 0;

        return data.Average();
    }

    // Sample standard deviation: denominator is n - 1.
    public static double SampleStandardDeviation(IEnumerable<double> values)
    {
        double[] data = values.ToArray();
        if (data.Length < 2)
            return 0;

        double mean = data.Average();
        double sumOfSquares = data.Sum(value => Math.Pow(value - mean, 2));
        return Math.Sqrt(sumOfSquares / (data.Length - 1));
    }
}
