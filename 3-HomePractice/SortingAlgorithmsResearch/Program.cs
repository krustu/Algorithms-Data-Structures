namespace SortingAlgorithmsResearch;

internal static class Program
{
    private static void Main()
    {
        string resultsPath = Path.Combine(Directory.GetCurrentDirectory(), "Results");
        var runner = new BenchmarkRunner(resultsPath, ExperimentSettings.RandomSeed);
        
        Console.WriteLine("============================================");
        Console.WriteLine(" SORTING ALGORITHMS - EXPERIMENTAL RESEARCH");
        Console.WriteLine("============================================");
        Console.WriteLine("1. Run correctness tests");
        Console.WriteLine("2. Run preliminary experiments");
        Console.WriteLine("3. Run main experiments (random, sorted, reverse-sorted)");
        Console.WriteLine("4. Run Insertion Sort vs Merge Sort experiment");
        Console.WriteLine("5. Run everything in order");
        Console.Write("Choose an option: ");

        string? choice = Console.ReadLine();
        try
        {
            switch (choice)
            {
                case "1":
                    runner.RunCorrectnessTests();
                    break;
                case "2":
                    runner.RunCorrectnessTests();
                    runner.RunPreliminaryExperiments(ExperimentSettings.PreliminarySizes);
                    break;
                case "3":
                    runner.RunCorrectnessTests();
                    runner.RunMainExperiments(ExperimentSettings.MeasurementPoints);
                    break;
                case "4":
                    runner.RunCorrectnessTests();
                    runner.RunInsertionVsMergeExperiment(
                        ExperimentSettings.CrossoverBaseSize,
                        ExperimentSettings.CrossoverAddedSizes,
                        ExperimentSettings.CrossoverRepetitions);
                    break;
                case "5":
                    runner.RunCorrectnessTests();
                    runner.RunPreliminaryExperiments(ExperimentSettings.PreliminarySizes);
                    Console.WriteLine("\nIMPORTANT: Check PreliminarySummary.csv and update MeasurementPoints in Models.cs.");
                    Console.WriteLine("Press Enter when you have updated the five measurement points, or Ctrl+C to stop.");
                    Console.ReadLine();
                    runner.RunMainExperiments(ExperimentSettings.MeasurementPoints);
                    runner.RunInsertionVsMergeExperiment(
                        ExperimentSettings.CrossoverBaseSize,
                        ExperimentSettings.CrossoverAddedSizes,
                        ExperimentSettings.CrossoverRepetitions);
                    break;
                default:
                    Console.WriteLine("Unknown option. Run the program again and choose 1-5.");
                    break;
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"\nERROR: {exception.Message}");
            Console.WriteLine(exception);
        }

        Console.WriteLine("\nFinished. Press Enter to close.");
        Console.ReadLine();
    }
}
