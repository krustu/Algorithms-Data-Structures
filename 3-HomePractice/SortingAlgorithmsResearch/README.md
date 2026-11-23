# Sorting Algorithms Research (C# / .NET 8)

An experimental project comparing Insertion Sort, Selection Sort and Merge Sort.

## Requirements
- .NET 8 SDK
- Visual Studio 2022, JetBrains Rider, or VS Code

## Run

Open a terminal in this folder and run:

```bash
dotnet run
```

Or open `SortingAlgorithmsResearch.csproj` in Visual Studio and press Run.

## Menu
1. Correctness tests.
2. Preliminary experiments (one run per configured size for random, sorted, and reverse-sorted data).
3. Main experiments (five measurement points, ten repetitions, random/sorted/reverse-sorted data).
4. Insertion Sort vs Merge Sort (sorted base sequence with unsorted values appended).
5. Run all stages in order.

## Recommended workflow

1. Run option `1` to verify all algorithms.
2. Run option `2` and inspect `Results/Preliminary.csv` and `Results/PreliminarySummary.csv`. Preliminary measurements include random, sorted, and reverse-sorted inputs.
3. If the largest preliminary sizes are still much faster than about one second, cautiously add larger sizes to `PreliminarySizes` in `Models.cs`. Selection Sort can become very slow because its work grows quadratically; increase sizes gradually.
4. Based on the preliminary results, edit `MeasurementPoints` in `Models.cs`. Choose exactly five equally spaced sizes in a range that is feasible for all three algorithms and all three input orders. The program checks that the five points are equally spaced.
5. Run option `3` to perform the main experiments.
5. Run option `4` for the required Insertion-vs-Merge experiment.
6. Keep all CSV files; they contain raw measurements and summary statistics.

## CSV output

For each experiment the program writes:
- a raw CSV file containing each individual measurement;
- a `...Summary.csv` file containing sample count, average time and sample standard deviation.

Times are recorded in milliseconds. The sample standard deviation uses `n - 1` in the denominator.

## Experimental methodology

- All arrays contain `double` values.
- The same source sequence is cloned for each algorithm in a repetition.
- Array generation and cloning are outside the timed section.
- Only the sorting call is timed with `Stopwatch`.
- Sortedness validation is performed after timing.
- Main experiments use ten repetitions for each algorithm, data type and measurement point.
- Each sorting algorithm is run once on a small warm-up array before timed experiments, to reduce first-use JIT effects.

## Important notes

- The sizes in `PreliminarySizes`, `MeasurementPoints`, and `CrossoverAddedSizes` are starting values, not guaranteed optimal values for every computer. Adjust them based on actual preliminary results.
- Selection Sort and Insertion Sort have quadratic worst-case time complexity. Large input sizes can take a very long time. Increase sizes gradually and do not start with huge arrays.
- The crossover experiment appends random values to an already sorted base sequence. This is the interpretation used by this implementation; describe it clearly in the report.
- Run experiments with other CPU-intensive applications closed. Do not compare results from different power modes or machines as if they were the same setup.
