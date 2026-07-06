namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Chaikin Oscillator.
/// </summary>
public class ChaikinOscHub
    : ChainHub<IBar, ChaikinOscResult>, IChaikinOsc
{
    internal ChaikinOscHub(
        IBarProvider<IBar> provider,
        int fastPeriods,
        int slowPeriods) : base(provider)
    {
        ChaikinOsc.Validate(fastPeriods, slowPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;

        FastK = 2d / (fastPeriods + 1);
        SlowK = 2d / (slowPeriods + 1);

        Name = $"CHAIKIN_OSC({fastPeriods},{slowPeriods})";

        // Validate cache size for warmup requirements
        int requiredWarmup = Math.Max(fastPeriods, slowPeriods);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }

    /// <inheritdoc/>
    public double FastK { get; private init; }

    /// <inheritdoc/>
    public double SlowK { get; private init; }
    /// <inheritdoc/>
    protected override (ChaikinOscResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        IBar bar = ProviderCache[i];

        // Calculate Money Flow Multiplier and Money Flow Volume (ADL components)
        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;
        double volume = (double)bar.Volume;

        double mfm = high == low
            ? 0
            : (close - low - (high - close)) / (high - low);
        double mfv = mfm * volume;

        // Calculate ADL (cumulative)
        double adl = i > 0 && Cache[i - 1].Adl.HasValue
            ? Cache[i - 1].Adl!.Value + mfv
            : mfv;

        // Calculate Fast EMA of ADL
        double fastEma = i >= FastPeriods - 1
            ? i > 0 && Cache[i - 1].FastEma is not null
                // Calculate EMA normally
                ? Ema.Increment(FastK, Cache[i - 1].FastEma!.Value, adl)
                // Initialize as SMA of ADL values
                : CalculateInitialSma(i, FastPeriods, adl)
            // warmup periods are never calculable
            : double.NaN;

        // Calculate Slow EMA of ADL
        double slowEma = i >= SlowPeriods - 1
            ? i > 0 && Cache[i - 1].SlowEma is not null
                // Calculate EMA normally
                ? Ema.Increment(SlowK, Cache[i - 1].SlowEma!.Value, adl)
                // Initialize as SMA of ADL values
                : CalculateInitialSma(i, SlowPeriods, adl)
            // warmup periods are never calculable
            : double.NaN;

        // Calculate Oscillator
        double? oscillator = (fastEma - slowEma).NaN2Null();

        // Candidate result
        ChaikinOscResult r = new(
            Timestamp: item.Timestamp,
            MoneyFlowMultiplier: mfm,
            MoneyFlowVolume: mfv,
            Adl: adl,
            Oscillator: oscillator,
            FastEma: fastEma.NaN2Null(),
            SlowEma: slowEma.NaN2Null());

        return (r, i);
    }

    private double CalculateInitialSma(int currentIndex, int periods, double currentAdl)
    {
        // Check if we have enough data
        if (currentIndex < periods - 1)
        {
            return double.NaN;
        }

        // Sum ADL values from Cache (which has entries 0 through currentIndex-1)
        // plus the current ADL value
        double sum = currentAdl;
        for (int j = currentIndex - periods + 1; j < currentIndex; j++)
        {
            if (j >= 0 && j < Cache.Count)
            {
                sum += Cache[j].Adl ?? 0;
            }
        }

        return sum / periods;
    }
}

public static partial class ChaikinOsc
{
    /// <summary>
    /// Creates a Chaikin Oscillator hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 3.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 10.</param>
    /// <returns>A Chaikin Oscillator hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the periods are invalid.</exception>
    public static ChaikinOscHub ToChaikinOscHub(
        this IBarProvider<IBar> chainProvider,
        int fastPeriods = 3,
        int slowPeriods = 10)
        => new(chainProvider, fastPeriods, slowPeriods);
}
