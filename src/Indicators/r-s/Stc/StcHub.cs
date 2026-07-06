namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating Schaff Trend Cycle (STC) streaming hubs.
/// </summary>
public class StcHub
    : ChainHub<MacdResult, StcResult>, IStc
{
    private CircularDoubleBuffer _macdBuffer;
    private readonly Queue<double> _rawKBuffer;

    internal StcHub(
        IChainProvider<IReusable> provider,
        int cyclePeriods,
        int fastPeriods,
        int slowPeriods)
        : this(
            provider.ToMacdHub(fastPeriods, slowPeriods, 0),
            cyclePeriods)
    { }

    internal StcHub(
        MacdHub macdHub,
        int cyclePeriods)
        : base(macdHub)
    {
        ArgumentNullException.ThrowIfNull(macdHub);
        Stc.Validate(cyclePeriods, macdHub.FastPeriods, macdHub.SlowPeriods);

        CyclePeriods = cyclePeriods;
        FastPeriods = macdHub.FastPeriods;
        SlowPeriods = macdHub.SlowPeriods;

        _macdBuffer = new CircularDoubleBuffer(cyclePeriods);
        _rawKBuffer = new Queue<double>(3);

        Name = $"STC({cyclePeriods},{FastPeriods},{SlowPeriods})";

        int requiredWarmup = Math.Max(FastPeriods, SlowPeriods) + cyclePeriods + 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int CyclePeriods { get; init; }

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    protected override (StcResult result, int index)
        ToIndicator(MacdResult item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double macd = item.Macd ?? double.NaN;

        if (!double.IsNaN(macd))
        {
            _macdBuffer.Add(macd);
        }

        double rawK = double.NaN;
        if (i >= SlowPeriods + CyclePeriods - 2 &&
            !double.IsNaN(macd) &&
            _macdBuffer.IsFull)
        {
            double highHigh = _macdBuffer.GetMax();
            double lowLow = _macdBuffer.GetMin();

            rawK = (highHigh - lowLow) != 0
                 ? 100 * (macd - lowLow) / (highHigh - lowLow)
                 : 0;
        }

        _rawKBuffer.Enqueue(rawK);
        if (_rawKBuffer.Count > 3)
        {
            _rawKBuffer.Dequeue();
        }

        double? stc = null;
        if (i >= SlowPeriods + CyclePeriods)
        {
            double sum = 0;
            foreach (double rawKValue in _rawKBuffer)
            {
                sum += rawKValue;
            }

            double smoothedK = sum / 3;
            stc = double.IsNaN(smoothedK) ? null : smoothedK;
        }

        StcResult result = new(
            Timestamp: item.Timestamp,
            Stc: stc);

        return (result, i);
    }

    /// <summary>
    /// Restores the stochastic state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _macdBuffer.Clear();
        _rawKBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        int startIdx = Math.Max(0, restoreIndex + 1 - CyclePeriods);
        for (int p = startIdx; p <= restoreIndex; p++)
        {
            MacdResult macdResult = ProviderCache[p];
            double macdValue = macdResult.Macd ?? double.NaN;
            if (!double.IsNaN(macdValue))
            {
                _macdBuffer.Add(macdValue);
            }
        }

        if (restoreIndex >= SlowPeriods + CyclePeriods - 2)
        {
            int kStart = Math.Max(SlowPeriods + CyclePeriods - 2, restoreIndex + 1 - 3);
            for (int p = kStart; p <= restoreIndex; p++)
            {
                int rStart = Math.Max(0, p + 1 - CyclePeriods);
                double hh = double.NegativeInfinity;
                double ll = double.PositiveInfinity;

                for (int r = rStart; r <= p; r++)
                {
                    MacdResult macdAtR = ProviderCache[r];
                    double macdValue = macdAtR.Macd ?? double.NaN;
                    if (macdValue > hh)
                    {
                        hh = macdValue;
                    }

                    if (macdValue < ll)
                    {
                        ll = macdValue;
                    }
                }

                MacdResult macdAtP = ProviderCache[p];
                double macdAtPValue = macdAtP.Macd ?? double.NaN;
                double rawAtP = (hh - ll) != 0 ? 100 * (macdAtPValue - ll) / (hh - ll) : 0;
                _rawKBuffer.Enqueue(rawAtP);
            }
        }
    }
}

public static partial class Stc
{
    /// <summary>
    /// Creates a Schaff Trend Cycle (STC) streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="cyclePeriods">Number of periods for the cycle. Default is 10.</param>
    /// <param name="fastPeriods">Number of fast periods for the MACD calculation. Default is 23.</param>
    /// <param name="slowPeriods">Number of slow periods for the MACD calculation. Default is 50.</param>
    /// <returns>A STC hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static StcHub ToStcHub(
        this IChainProvider<IReusable> chainProvider,
        int cyclePeriods = 10,
        int fastPeriods = 23,
        int slowPeriods = 50)
        => new(chainProvider, cyclePeriods, fastPeriods, slowPeriods);

    /// <summary>
    /// Creates a new STC hub, using values from an existing MACD hub.
    /// </summary>
    /// <param name="macdHub">MACD hub.</param>
    /// <param name="cyclePeriods">Number of periods for the cycle. Default is 10.</param>
    /// <returns>An STC hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the MACD hub is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when cyclePeriods is invalid.</exception>
    /// <remarks>
    /// <para>IMPORTANT: This is not a normal chaining approach.</para>
    /// This extension overrides and enables a chain that specifically
    /// reuses the existing <see cref="MacdHub"/> in its internal construction.
    ///</remarks>
    public static StcHub ToStcHub(
        this MacdHub macdHub,
        int cyclePeriods = 10)
        => new(macdHub, cyclePeriods);
}
