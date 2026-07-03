using BenchmarkDotNet.Running;

namespace Performance;

public static class Program
{
    public static void Main(string[] args)
    {
        DefaultConfig config = new();

        if (args?.Length == 0)
        {
            // With no filter, run the official BASELINE SET.
            // This is the single source of truth for what `perf.ps1 reset`
            // and `perf.ps1 evaluate` cover. Keep this list in sync with the
            // $BaselineClasses list in perf.ps1.
            // example: dotnet run -c Release
            BenchmarkRunner.Run<SeriesIndicators>(config);   // every indicator, Series style
            BenchmarkRunner.Run<BufferIndicators>(config);   // every indicator, BufferList style
            BenchmarkRunner.Run<StreamIndicators>(config);   // every indicator, StreamHub style
            BenchmarkRunner.Run<Utility>(config);            // shared conversion/utility hot paths
            BenchmarkRunner.Run<UtilityNullMath>(config);    // null-math helpers
            BenchmarkRunner.Run<UtilityStdDev>(config);      // standard-deviation helper

            // NOTE: StyleComparison, StreamExternal, and ManualTestDirect are
            // intentionally NOT baselined. They are ad-hoc diagnostics; run them
            // explicitly with `-- --filter` when needed.
        }
        else
        {
            // with filter, run based on arguments (e.g. filter)
            // example: dotnet run -c Release -- --filter "*.ToAdxBatch"
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}
