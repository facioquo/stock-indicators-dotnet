namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for MACD (Moving Average Convergence Divergence).
/// </summary>
public class MacdHub
    : ChainHub<IReusable, MacdResult>, IMacd
{
    internal MacdHub(
        IChainProvider<IReusable> provider,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods) : base(provider)
    {
        Macd.Validate(fastPeriods, slowPeriods, signalPeriods);

        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        FastK = 2d / (fastPeriods + 1);
        SlowK = 2d / (slowPeriods + 1);
        SignalK = 2d / (signalPeriods + 1);

        Name = $"MACD({fastPeriods},{slowPeriods},{signalPeriods})";

        // Validate cache size for warmup requirements
        // First valid Signal is at index (slowPeriods + signalPeriods - 2), requiring slowPeriods + signalPeriods - 1 items.
        int requiredWarmup = slowPeriods + signalPeriods - 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }

    /// <inheritdoc/>
    public double FastK { get; private init; }

    /// <inheritdoc/>
    public double SlowK { get; private init; }

    /// <inheritdoc/>
    public double SignalK { get; private init; }
    /// <inheritdoc/>
    protected override (MacdResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate Fast EMA
        double fastEma = i >= FastPeriods - 1
            ? i > 0 && Cache[i - 1].FastEma is not null
                // Calculate EMA normally
                ? Ema.Increment(FastK, Cache[i - 1].FastEma!.Value, item.Value)
                // Initialize as SMA
                : Sma.Increment(ProviderCache, FastPeriods, i)
            // warmup periods are never calculable
            : double.NaN;

        // Calculate Slow EMA
        double slowEma = i >= SlowPeriods - 1
            ? i > 0 && Cache[i - 1].SlowEma is not null
                // Calculate EMA normally
                ? Ema.Increment(SlowK, Cache[i - 1].SlowEma!.Value, item.Value)
                // Initialize as SMA
                : Sma.Increment(ProviderCache, SlowPeriods, i)
            // warmup periods are never calculable
            : double.NaN;

        // Calculate MACD
        double macd = fastEma - slowEma;

        // Calculate Signal
        double signal;
        if (i >= SignalPeriods + SlowPeriods - 2 && (i == 0 || Cache[i - 1].Signal is null))
        {
            // Initialize signal as SMA of MACD values
            double sum = macd;
            for (int j = i - SignalPeriods + 1; j < i; j++)
            {
                sum += Cache[j].Value;
            }

            signal = sum / SignalPeriods;
        }
        else
        {
            // Calculate signal EMA normally
            signal = Ema.Increment(SignalK, i > 0 ? Cache[i - 1].Signal ?? double.NaN : double.NaN, macd);
        }

        // Candidate result
        MacdResult r = new(
            Timestamp: item.Timestamp,
            Macd: macd.NaN2Null(),
            Signal: signal.NaN2Null(),
            Histogram: (macd - signal).NaN2Null(),
            FastEma: fastEma.NaN2Null(),
            SlowEma: slowEma.NaN2Null());

        return (r, i);
    }
}

public static partial class Macd
{
    /// <summary>
    /// Creates a MACD streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 12.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 26.</param>
    /// <param name="signalPeriods">Number of periods for the signal line. Default is 9.</param>
    /// <returns>A MACD hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static MacdHub ToMacdHub(
        this IChainProvider<IReusable> chainProvider,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        => new(chainProvider, fastPeriods, slowPeriods, signalPeriods);
}
