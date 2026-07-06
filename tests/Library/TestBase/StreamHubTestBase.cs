namespace Test.Base;

/// <summary>
/// Base tests that all streamed indicators should have.
/// </summary>
public abstract class StreamHubTestBase : TestBaseWithPrecision  // default: bar observer
{
    internal const int removeAtIndex = 495;

    internal static readonly IReadOnlyList<Bar> RevisedBars
        = Bars.Where(static (_, idx) => idx != removeAtIndex).ToList();

    /// <summary>
    /// Tests hub-unique name string
    /// </summary>
    public abstract void ToStringOverride_ReturnsExpectedName();
}

/// <summary>
/// Add this to stream indicator tests
/// (all get this directly or inherited from <see cref="ITestChainObserver"/>).
/// </summary>
public interface ITestBarObserver
{
    /// <summary>
    /// <para>Tests hub compatibility with a bar provider.</para>
    /// <para>
    /// Implementations should verify the observer reproduces the canonical
    /// time-series given a realistic bar stream, including warmup
    /// behaviour, late arrivals, duplicate re-sends and removals. When
    /// applicable, include checks that the final observed series is
    /// identical to the equivalent static-series computation.
    /// </para>
    /// </summary>
    void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly();

    /// <summary>
    /// <para>Tests hub cache pruning behavior.</para>
    /// <para>
    /// Implementations should verify that when a BarHub has MaxCacheSize set,
    /// the observer produces results matching the last N results from the full
    /// series computation (where N = MaxCacheSize), not a recomputation on just
    /// the cached bars which would have different warmup.
    /// </para>
    /// </summary>
    void WithCachePruning_MatchesSeriesExactly();
}

public interface ITestTradeTickObserver
{
    /// <summary>
    /// Tests hub compatibility with tick provider
    /// </summary>
    void TradeTickObserver_WithWarmupAndMultipleSameTimestamp_WorksCorrectly();
}

/// <summary>
/// Add this to stream chainee indicator tests.
/// </summary>
public interface ITestChainObserver : ITestBarObserver
{
    /// <summary>
    /// <para>
    /// Tests hub compatibility when the hub is chained to another provider
    /// (for example: BarHub -> SmaHub -> SubjectHub).
    /// </para>
    /// <para>
    /// Implementations should verify that chained providers produce the
    /// same final series as the equivalent time-series pipeline and that
    /// cache/pruning behaviour of upstream providers does not corrupt
    /// downstream results.
    /// </para>
    /// </summary>
    void ChainObserver_ChainedProvider_MatchesSeriesExactly();
}

/// <summary>
/// Add this to all stream chainor indicator tests.
/// </summary>
public interface ITestChainProvider
{
    /// <summary>
    /// <para>Tests hub capability as a chain provider.</para>
    /// <para>
    /// Implementations should confirm that the hub can act as a provider
    /// for downstream hubs and that chaining preserves series equality,
    /// correct warmup handling, and predictable cache-pruning behaviour
    /// when upstream caches are constrained.
    /// </para>
    /// </summary>
    void ChainProvider_MatchesSeriesExactly();
}
