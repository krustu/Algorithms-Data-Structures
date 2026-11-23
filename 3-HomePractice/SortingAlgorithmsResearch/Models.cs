namespace SortingAlgorithmsResearch;

public enum DataOrder
{
    Random,
    Sorted,
    ReverseSorted
}

public sealed record Measurement(
    string Experiment,
    string DataType,
    string Algorithm,
    int ElementCount,
    int Repetition,
    double ElapsedMilliseconds);

public sealed record Summary(
    string Experiment,
    string DataType,
    string Algorithm,
    int ElementCount,
    int SampleCount,
    double AverageMilliseconds,
    double StandardDeviationMilliseconds);

public static class ExperimentSettings
{
    // Replace these with five equally spaced values selected after the preliminary experiment.
    // Example only: 2,000 / 4,000 / 6,000 / 8,000 / 10,000.
    public static readonly int[] MeasurementPoints = { 2_000, 4_000, 6_000, 8_000, 10_000 };

    public const int Repetitions = 10;
    public const int RandomSeed = 20261124;

    // Initial exploration sizes only. Inspect PreliminarySummary.csv and extend this
    // list gradually if times are still much shorter than ~1 second. Large quadratic
    // runs can take a long time, especially SelectionSort on large arrays.
    public static readonly int[] PreliminarySizes =
        { 500, 1_000, 2_000, 4_000, 6_000, 8_000, 10_000, 12_000, 15_000 };

    public const int CrossoverBaseSize = 10_000;
    public static readonly int[] CrossoverAddedSizes =
        { 1, 5, 10, 25, 50, 100, 250, 500, 1_000, 2_000, 3_000, 5_000 };
    public const int CrossoverRepetitions = 10;
}
