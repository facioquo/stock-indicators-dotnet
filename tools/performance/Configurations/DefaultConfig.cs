using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;

namespace Performance;

/// <summary>
/// BenchmarkDotNet configuration for performance testing (default).
/// </summary>
public class DefaultConfig : ManualConfig
{
    public DefaultConfig()
    {
        // Build every job, including attribute jobs, with the .NET 11 toolchain
        AddJob(Job.Default.WithToolchain(Net11Toolchain.Instance).AsMutator());

        // Build sequentially: the parallel-build check reads a runtime version 0.15.8 can't name for .NET 11
        WithOptions(ConfigOptions.DisableParallelBuild);

        // Add standard exporters for different formats
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(JsonExporter.Full);

        // Add columns for detailed analysis
        AddColumn(TargetMethodColumn.Method);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Error);
        AddColumn(StatisticColumn.StdDev);

        // Add memory diagnostics with GC collection columns
        AddDiagnoser(new MemoryDiagnoser(new MemoryDiagnoserConfig(displayGenColumns: true)));

        // Sort results by method name
        WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Method));

        // Add logger
        AddLogger(ConsoleLogger.Default);
    }
}
