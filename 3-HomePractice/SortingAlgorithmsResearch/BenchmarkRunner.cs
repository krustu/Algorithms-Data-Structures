using System.Diagnostics;
using System.Globalization;

namespace SortingAlgorithmsResearch;

public sealed class BenchmarkRunner
{
    private readonly string _resultsDirectory;
    private readonly Random _random;

    private readonly (string Name, Action<double[]> Sort)[] _algorithms =
    {
        ("InsertionSort", SortingAlgorithms.InsertionSort),
        ("SelectionSort", SortingAlgorithms.SelectionSort),
        ("MergeSort", SortingAlgorithms.MergeSort)
    };

    public BenchmarkRunner(string resultsDirectory, int seed)
    {
        _resultsDirectory = resultsDirectory;
        Directory.CreateDirectory(_resultsDirectory);
        _random = new Random(seed);
    }

    public void RunCorrectnessTests()
    {
        Console.WriteLine("Running correctness tests...");

        double[][] testCases =
        {
            Array.Empty<double>(),
            new double[] { 42 },
            new double[] { 5, 2, 8, 1, 3 },
            new double[] { 1, 2, 3, 4, 5 },
            new double[] { 5, 4, 3, 2, 1 },
            new double[] { 3, 3, 3, 3 },
            new double[] { -5, 0, 12, -2, 7 },
            new double[] { 1.5, -3.2, 1.5, 0.0, 9.8 }
        };

        foreach (var algorithm in _algorithms)
        {
            foreach (double[] testCase in testCases)
            {
                double[] actual = (double[])testCase.Clone();
                double[] expected = (double[])testCase.Clone();
                Array.Sort(expected);
                algorithm.Sort(actual);

                if (!actual.SequenceEqual(expected))
                    throw new InvalidOperationException($"{algorithm.Name} failed a correctness test.");
            }

            Console.WriteLine($"  {algorithm.Name}: PASS");
        }

        Console.WriteLine("All correctness tests passed.\n");
    }

    public void RunPreliminaryExperiments(int[] sizes)
    {
        Console.WriteLine("PRELIMINARY EXPERIMENTS (one run per size and data order)");
        WarmUpAlgorithms();
        var measurements = new List<Measurement>();

        foreach (int size in sizes)
        {
            ValidateSize(size);
            double[] randomSource = CreateRandomArray(size);
            var dataSets = new[]
            {
                (Name: "Random", Data: randomSource),
                (Name: "Sorted", Data: randomSource.OrderBy(x => x).ToArray()),
                (Name: "ReverseSorted", Data: randomSource.OrderByDescending(x => x).ToArray())
            };

            foreach (var dataSet in dataSets)
            {
                foreach (var algorithm in _algorithms)
                {
                    double[] input = (double[])dataSet.Data.Clone();
                    double elapsed = Measure(algorithm.Sort, input);
                    measurements.Add(new Measurement("Preliminary", dataSet.Name, algorithm.Name, size, 1, elapsed));
                    Console.WriteLine($"n={size,8:N0} | {dataSet.Name,-13} | {algorithm.Name,-15} | {elapsed,12:F3} ms");
                }
            }
        }

        WriteMeasurements("Preliminary.csv", measurements);
        WriteSummaries("PreliminarySummary.csv", BuildSummaries(measurements));
        Console.WriteLine("Saved Preliminary.csv and PreliminarySummary.csv");
        Console.WriteLine("Use these measurements to choose five equally spaced sizes that are feasible for ALL algorithms and data orders.");
    }

    public void RunMainExperiments(int[] measurementPoints)
    {
        if (measurementPoints.Length != 5)
            throw new ArgumentException("Exactly five measurement points are required.");

        int[] points = measurementPoints.OrderBy(x => x).ToArray();
        if (points.Any(x => x <= 0) || points.Distinct().Count() != 5)
            throw new ArgumentException("Measurement points must be five distinct positive integers.");

        int step = points[1] - points[0];
        if (step <= 0 || points.Where((value, index) => index > 0 && value - points[index - 1] != step).Any())
            throw new ArgumentException("The five measurement points must be equally spaced.");

        WarmUpAlgorithms();
        Console.WriteLine("MAIN EXPERIMENTS");
        Console.WriteLine("Each generated source sequence is cloned and supplied to all three algorithms.");

        RunDataOrderExperiment("Random", points, CreateRandomArray, "RandomData");
        RunDataOrderExperiment("Sorted", points, size => CreateRandomArray(size).OrderBy(x => x).ToArray(), "SortedData");
        RunDataOrderExperiment("ReverseSorted", points, size => CreateRandomArray(size).OrderByDescending(x => x).ToArray(), "ReverseSortedData");
    }

    private void RunDataOrderExperiment(
        string dataType,
        int[] points,
        Func<int, double[]> createSource,
        string filePrefix)
    {
        var measurements = new List<Measurement>();
        Console.WriteLine($"\n--- {dataType} ---");

        foreach (int size in points)
        {
            for (int repetition = 1; repetition <= ExperimentSettings.Repetitions; repetition++)
            {
                double[] source = createSource(size);

                foreach (var algorithm in _algorithms)
                {
                    double[] input = (double[])source.Clone(); // copy is outside the timed section
                    double elapsed = Measure(algorithm.Sort, input);
                    measurements.Add(new Measurement("Main", dataType, algorithm.Name, size, repetition, elapsed));
                    Console.WriteLine($"n={size,8:N0} | run={repetition,2} | {algorithm.Name,-15} | {elapsed,12:F3} ms");
                }
            }
        }

        WriteMeasurements($"{filePrefix}.csv", measurements);
        WriteSummaries($"{filePrefix}Summary.csv", BuildSummaries(measurements));
        Console.WriteLine($"Saved {filePrefix}.csv and {filePrefix}Summary.csv");
    }

    public void RunInsertionVsMergeExperiment(int baseSize, int[] addedSizes, int repetitions)
    {
        ValidateSize(baseSize);
        if (repetitions < 2)
            throw new ArgumentOutOfRangeException(nameof(repetitions), "Use at least two repetitions.");

        WarmUpAlgorithms();
        Console.WriteLine("\nINSERTION SORT VS MERGE SORT");
        Console.WriteLine($"Sorted base sequence: {baseSize:N0} elements");
        var measurements = new List<Measurement>();

        foreach (int addedCount in addedSizes.OrderBy(x => x))
        {
            if (addedCount < 0)
                throw new ArgumentOutOfRangeException(nameof(addedSizes));

            for (int repetition = 1; repetition <= repetitions; repetition++)
            {
                // Start with an already sorted base, then append unsorted values.
                double[] baseSequence = CreateRandomArray(baseSize).OrderBy(x => x).ToArray();
                double[] appendedValues = CreateRandomArray(addedCount);
                double[] source = baseSequence.Concat(appendedValues).ToArray();

                foreach (var algorithm in new[]
                {
                    (Name: "InsertionSort", Sort: (Action<double[]>)SortingAlgorithms.InsertionSort),
                    (Name: "MergeSort", Sort: (Action<double[]>)SortingAlgorithms.MergeSort)
                })
                {
                    double[] input = (double[])source.Clone();
                    double elapsed = Measure(algorithm.Sort, input);
                    measurements.Add(new Measurement("Crossover", $"SortedBasePlus{addedCount}", algorithm.Name,
                        source.Length, repetition, elapsed));
                    Console.WriteLine($"added={addedCount,6:N0} | run={repetition,2} | {algorithm.Name,-15} | {elapsed,12:F3} ms");
                }
            }
        }

        WriteMeasurements("InsertionVsMerge.csv", measurements);
        WriteSummaries("InsertionVsMergeSummary.csv", BuildSummaries(measurements));
        Console.WriteLine("Saved InsertionVsMerge.csv and InsertionVsMergeSummary.csv");
        Console.WriteLine("Compare the average times to identify where MergeSort becomes faster.");
    }

    private void WarmUpAlgorithms()
    {
        // Run each implementation before collecting timings so first-use JIT compilation
        // is not included in the experimental measurements.
        foreach (var algorithm in _algorithms)
        {
            double[] warmupInput = { 5.0, 1.0, 4.0, 2.0, 3.0 };
            algorithm.Sort(warmupInput);
        }
    }

    private double Measure(Action<double[]> sort, double[] input)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        sort(input);
        stopwatch.Stop();

        // Validate outside the measured interval.
        if (!IsSorted(input))
            throw new InvalidOperationException("An algorithm produced an incorrectly sorted array.");

        return stopwatch.Elapsed.TotalMilliseconds;
    }

    private static bool IsSorted(double[] array)
    {
        for (int i = 1; i < array.Length; i++)
        {
            if (array[i - 1] > array[i])
                return false;
        }
        return true;
    }

    private double[] CreateRandomArray(int size)
    {
        var result = new double[size];
        for (int i = 0; i < size; i++)
            result[i] = _random.NextDouble() * 2_000_000.0 - 1_000_000.0;
        return result;
    }

    private static void ValidateSize(int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));
    }

    private static List<Summary> BuildSummaries(IEnumerable<Measurement> measurements)
    {
        return measurements
            .GroupBy(m => new { m.Experiment, m.DataType, m.Algorithm, m.ElementCount })
            .Select(group =>
            {
                double[] values = group.Select(x => x.ElapsedMilliseconds).ToArray();
                return new Summary(
                    group.Key.Experiment,
                    group.Key.DataType,
                    group.Key.Algorithm,
                    group.Key.ElementCount,
                    values.Length,
                    Statistics.Average(values),
                    Statistics.SampleStandardDeviation(values));
            })
            .OrderBy(x => x.DataType)
            .ThenBy(x => x.ElementCount)
            .ThenBy(x => x.Algorithm)
            .ToList();
    }

    private void WriteMeasurements(string fileName, IEnumerable<Measurement> measurements)
    {
        string path = Path.Combine(_resultsDirectory, fileName);
        using var writer = new StreamWriter(path, false);
        writer.WriteLine("Experiment,DataType,Algorithm,ElementCount,Repetition,ElapsedMilliseconds");

        foreach (Measurement m in measurements)
        {
            writer.WriteLine(string.Join(",",
                Csv(m.Experiment), Csv(m.DataType), Csv(m.Algorithm),
                m.ElementCount.ToString(CultureInfo.InvariantCulture),
                m.Repetition.ToString(CultureInfo.InvariantCulture),
                m.ElapsedMilliseconds.ToString("F6", CultureInfo.InvariantCulture)));
        }
    }

    private void WriteSummaries(string fileName, IEnumerable<Summary> summaries)
    {
        string path = Path.Combine(_resultsDirectory, fileName);
        using var writer = new StreamWriter(path, false);
        writer.WriteLine("Experiment,DataType,Algorithm,ElementCount,SampleCount,AverageMilliseconds,StandardDeviationMilliseconds");

        foreach (Summary s in summaries)
        {
            writer.WriteLine(string.Join(",",
                Csv(s.Experiment), Csv(s.DataType), Csv(s.Algorithm),
                s.ElementCount.ToString(CultureInfo.InvariantCulture),
                s.SampleCount.ToString(CultureInfo.InvariantCulture),
                s.AverageMilliseconds.ToString("F6", CultureInfo.InvariantCulture),
                s.StandardDeviationMilliseconds.ToString("F6", CultureInfo.InvariantCulture)));
        }
    }

    private static string Csv(string value) => $"\"{value.Replace("\"", "\"\"")}\"";
}
