using TestData = Test.Data.Data;

namespace Test.Base;

/// <summary>
/// Base setup for regression tests
/// </summary>
/// <param name="filename">Relative file path and name</param>
public abstract class RegressionTestBase<TResult>(string filename) : TestBase
    where TResult : ISeries
{
    protected IReadOnlyList<TResult> Expected { get; init; } = TestData.ResultsFromJson<TResult>(filename);

    private static BarHub CreateBarHub()
    {
        BarHub hub = new();
        hub.Add(Bars);
        return hub;
    }

    protected static BarHub BarHub { get; } = CreateBarHub();

    public abstract void Series_AgainstBaseline_MatchesExactly();
    public abstract void Buffer_AgainstBaseline_MatchesExactly();
    public abstract void Stream_AgainstBaseline_MatchesExactly();
}
