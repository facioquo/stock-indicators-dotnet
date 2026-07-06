using System.Text.Json;
using FacioQuo.Stock.Indicators;

namespace Test.DataGenerator;

/// <summary>
/// Console application for generating regression baseline files for StaticSeries indicators.
/// </summary>
internal static class Program
{
    // Serialize with standard JSON options
    private static readonly JsonSerializerOptions options = new() {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    private static int Main(string[] args)
    {
        Console.WriteLine("Baseline Generator for Stock Indicators");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Change to test indicators directory so test data files can be found
        // Find repo root by looking for .git directory
        string currentDir = Directory.GetCurrentDirectory();
        string? repoRoot = currentDir;

        while (repoRoot != null && !Directory.Exists(Path.Combine(repoRoot, ".git")))
        {
            repoRoot = Path.GetDirectoryName(repoRoot);
        }

        if (repoRoot == null)
        {
            Console.Error.WriteLine("Error: Could not find repository root (no .git directory found)");
            Console.Error.WriteLine($"Current directory: {currentDir}");
            return 1;
        }

        string testIndicatorsPath = Path.Combine(repoRoot, "tests", "Indicators");

        if (!Directory.Exists(testIndicatorsPath))
        {
            Console.Error.WriteLine($"Error: Test indicators directory not found at {testIndicatorsPath}");
            Console.Error.WriteLine($"Repository root: {repoRoot}");
            return 1;
        }

        Directory.SetCurrentDirectory(testIndicatorsPath);
        Console.WriteLine($"Working directory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine();

        if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();
            return 0;
        }

        try
        {
            if (args.Contains("--all"))
            {
                return GenerateAllBaselines();
            }

            int indicatorIndex = Array.IndexOf(args, "--indicator");
            if (indicatorIndex >= 0 && indicatorIndex + 1 < args.Length)
            {
                string indicatorName = args[indicatorIndex + 1];
                return GenerateSingleBaseline(indicatorName);
            }

            Console.Error.WriteLine("Error: Invalid arguments. Use --help for usage information.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  Test.DataGenerator --all");
        Console.WriteLine("  Test.DataGenerator --indicator <name>");
        Console.WriteLine("  Test.DataGenerator --help");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --all              Generate baselines for all StaticSeries indicators");
        Console.WriteLine("  --indicator <name> Generate baseline for a specific indicator");
        Console.WriteLine("  --help, -h         Display this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  Test.DataGenerator --all");
        Console.WriteLine("  Test.DataGenerator --indicator Sma");
        Console.WriteLine();
    }

    private static int GenerateSingleBaseline(string indicatorName)
    {
        Console.WriteLine($"Generating baseline for {indicatorName}...");

        IndicatorListing? listing = Catalog.Get(indicatorName, Style.Series);
        if (listing == null)
        {
            Console.Error.WriteLine($"Error: Indicator '{indicatorName}' not found in catalog.");
            return 1;
        }

        try
        {
            GenerateBaseline(listing);
            Console.WriteLine("✓ Baseline generated successfully");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"✗ Failed to generate baseline: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static int GenerateAllBaselines()
    {
        Console.WriteLine("Discovering StaticSeries indicators...");

        IReadOnlyList<IndicatorListing> seriesIndicators = Catalog.Get(Style.Series);
        Console.WriteLine($"Found {seriesIndicators.Count} StaticSeries indicators");
        Console.WriteLine();

        int successCount = 0;
        int failureCount = 0;
        int skippedCount = 0;
        List<string> failures = [];
        object lockObj = new();

        Parallel.ForEach(seriesIndicators, listing => {
            try
            {
                GenerateBaseline(listing);

                lock (lockObj)
                {
                    successCount++;
                    Console.WriteLine($"[{successCount + failureCount + skippedCount}/{seriesIndicators.Count}] ✓ {listing.Uiid}");
                }
            }
            catch (NotSupportedException ex)
            {
                lock (lockObj)
                {
                    skippedCount++;
                    Console.WriteLine($"[{successCount + failureCount + skippedCount}/{seriesIndicators.Count}] ⊘ {listing.Uiid}: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    failureCount++;
                    string errorMsg = $"{listing.Uiid}: {ex.Message}";
                    failures.Add(errorMsg);
                    Console.WriteLine($"[{successCount + failureCount + skippedCount}/{seriesIndicators.Count}] ✗ {errorMsg}");
                }
            }
        });

        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"  Success: {successCount}");
        Console.WriteLine($"  Failure: {failureCount}");
        Console.WriteLine($"  Skipped: {skippedCount}");

        if (failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Failed indicators:");
            foreach (string failure in failures)
            {
                Console.WriteLine($"  - {failure}");
            }
        }

        Console.WriteLine();

        return failureCount > 0 ? 1 : 0;
    }

    private static void GenerateBaseline(IndicatorListing listing)
    {
        // Execute indicator and get results
        object results = IndicatorExecutor.Execute(listing);

        // Write baseline file
        string baselinePath = IndicatorExecutor.GetBaselinePath(listing);
        WriteBaselineFile(baselinePath, results);
    }

    private static void WriteBaselineFile(string path, object results)
    {
        // Ensure directory exists
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(results, options);
        File.WriteAllText(path, json);
    }
}
