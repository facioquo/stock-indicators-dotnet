namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Schaff Trend Cycle from incremental reusable values.
/// </summary>
public class StcList : BufferList<StcResult>, IIncrementFromChain, IStc
{
    private readonly MacdList _macdList;
    private readonly StochList _stochList;

    /// <summary>
    /// Initializes a new instance of the <see cref="StcList"/> class.
    /// </summary>
    /// <param name="cyclePeriods">Number of periods for the cycle calculation.</param>
    /// <param name="fastPeriods">Number of periods for the fast MA.</param>
    /// <param name="slowPeriods">Number of periods for the slow MA.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slowPeriods"/> is invalid.</exception>
    public StcList(
        int cyclePeriods = 10,
        int fastPeriods = 23,
        int slowPeriods = 50)
    {
        Stc.Validate(cyclePeriods, fastPeriods, slowPeriods);

        CyclePeriods = cyclePeriods;
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;

        _macdList = new MacdList(fastPeriods, slowPeriods, 1);
        _stochList = new StochList(cyclePeriods, 3, 3, 3, 2, MaType.SMA);

        Name = $"STC({cyclePeriods}, {fastPeriods}, {slowPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StcList"/> class with initial reusable values.
    /// </summary>
    /// <param name="cyclePeriods">Number of periods for the cycle calculation.</param>
    /// <param name="fastPeriods">Number of periods for the fast MA.</param>
    /// <param name="slowPeriods">Number of periods for the slow MA.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public StcList(
        int cyclePeriods,
        int fastPeriods,
        int slowPeriods,
        IReadOnlyList<IReusable> values)
        : this(cyclePeriods, fastPeriods, slowPeriods) => Add(values);

    /// <inheritdoc />
    public int CyclePeriods { get; init; }

    /// <inheritdoc />
    public int FastPeriods { get; init; }

    /// <inheritdoc />
    public int SlowPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Add to MACD list
        _macdList.Add(timestamp, value);

        // Get the latest MACD result
        MacdResult macdResult = _macdList[^1];

        // If MACD has a value, feed it to the Stochastic calculation
        // MACD value is used as high, low, and close for stochastic calculation
        if (macdResult.Macd.HasValue)
        {
            double macdValue = macdResult.Macd.Value;
            _stochList.Add(timestamp, macdValue, macdValue, macdValue);
        }

        // Create STC result from the Stochastic result
        StcResult result;
        if (_stochList.Count > 0)
        {
            StochResult stochResult = _stochList[^1];
            result = new StcResult(
                Timestamp: timestamp,
                Stc: stochResult.Oscillator);
        }
        else
        {
            result = new StcResult(timestamp, null);
        }

        AddInternal(result);
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _macdList.Clear();
        _stochList.Clear();
    }
    /// <summary>
    /// Overrides list pruning to synchronize the nested child lists.
    /// </summary>
    protected override void PruneList()
    {
        // Synchronize child lists' MaxListSize with the parent list
        // Keep enough history to support calculations
        int minSize = Math.Max(SlowPeriods, CyclePeriods) + 1;
        _macdList.MaxListSize = Math.Max(minSize, MaxListSize + 1);
        _stochList.MaxListSize = Math.Max(minSize, MaxListSize + 1);

        // Call base implementation to prune the outer result list
        base.PruneList();
    }
}

public static partial class Stc
{
    /// <summary>
    /// Creates a buffer list for Schaff Trend Cycle calculations.
    /// </summary>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="cyclePeriods">Number of periods for the cycle calculation.</param>
    /// <param name="fastPeriods">Number of periods for the fast MA.</param>
    /// <param name="slowPeriods">Number of periods for the slow MA.</param>
    /// <returns>A buffer list for Schaff Trend Cycle calculations.</returns>
    public static StcList ToStcList(
        this IReadOnlyList<IReusable> source,
        int cyclePeriods = 10,
        int fastPeriods = 23,
        int slowPeriods = 50)
        => new(cyclePeriods, fastPeriods, slowPeriods) { source };
}
