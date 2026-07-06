namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for PVO (Percentage Volume Oscillator).
/// </summary>
public class PvoHub
    : ChainHub<IReusable, PvoResult>, IPvo
{
    private double _prevFastEma = double.NaN;
    private double _prevSlowEma = double.NaN;

    internal PvoHub(
        IChainProvider<IReusable> provider,
        int fastPeriods,
        int slowPeriods,
        int signalPeriods) : base(provider)
    {
        Pvo.Validate(fastPeriods, slowPeriods, signalPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        SignalPeriods = signalPeriods;

        FastK = 2d / (fastPeriods + 1);
        SlowK = 2d / (slowPeriods + 1);
        SignalK = 2d / (signalPeriods + 1);

        Name = $"PVO({fastPeriods},{slowPeriods},{signalPeriods})";

        // Validate cache size for warmup requirements
        int requiredWarmup = Math.Max(fastPeriods, slowPeriods) + signalPeriods;
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
    protected override (PvoResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate Fast EMA
        double fastEma;
        if (i < FastPeriods - 1)
        {
            fastEma = double.NaN;
        }
        else if (double.IsNaN(_prevFastEma))
        {
            // Initialize as SMA
            fastEma = Sma.Increment(ProviderCache, FastPeriods, i);
        }
        else
        {
            // Calculate EMA normally
            fastEma = Ema.Increment(FastK, _prevFastEma, item.Value);
        }

        // Calculate Slow EMA
        double slowEma;
        if (i < SlowPeriods - 1)
        {
            slowEma = double.NaN;
        }
        else if (double.IsNaN(_prevSlowEma))
        {
            // Initialize as SMA
            slowEma = Sma.Increment(ProviderCache, SlowPeriods, i);
        }
        else
        {
            // Calculate EMA normally
            slowEma = Ema.Increment(SlowK, _prevSlowEma, item.Value);
        }

        // Update state for next iteration
        _prevFastEma = fastEma;
        _prevSlowEma = slowEma;

        // Calculate PVO
        double pvo = slowEma != 0 ? 100 * ((fastEma - slowEma) / slowEma) : double.NaN;

        // Calculate Signal
        double signal;
        if (i >= SignalPeriods + SlowPeriods - 2 && (i == 0 || Cache[i - 1].Signal is null))
        {
            // Initialize signal as SMA of PVO values
            double sum = pvo;
            for (int j = i - SignalPeriods + 1; j < i; j++)
            {
                sum += Cache[j].Value;
            }

            signal = sum / SignalPeriods;
        }
        else
        {
            // Calculate signal EMA normally
            signal = Ema.Increment(SignalK, i > 0 ? Cache[i - 1].Signal ?? double.NaN : double.NaN, pvo);
        }

        // Candidate result
        PvoResult r = new(
            Timestamp: item.Timestamp,
            Pvo: pvo.NaN2Null(),
            Signal: signal.NaN2Null(),
            Histogram: (pvo - signal).NaN2Null());

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset state
        _prevFastEma = double.NaN;
        _prevSlowEma = double.NaN;

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild state up to restoreIndex
        for (int i = 0; i <= restoreIndex; i++)
        {
            IReusable item = ProviderCache[i];

            // Calculate Fast EMA
            if (i >= FastPeriods - 1)
            {
                if (double.IsNaN(_prevFastEma))
                {
                    _prevFastEma = Sma.Increment(ProviderCache, FastPeriods, i);
                }
                else
                {
                    _prevFastEma = Ema.Increment(FastK, _prevFastEma, item.Value);
                }
            }

            // Calculate Slow EMA
            if (i >= SlowPeriods - 1)
            {
                if (double.IsNaN(_prevSlowEma))
                {
                    _prevSlowEma = Sma.Increment(ProviderCache, SlowPeriods, i);
                }
                else
                {
                    _prevSlowEma = Ema.Increment(SlowK, _prevSlowEma, item.Value);
                }
            }
        }
    }
}

public static partial class Pvo
{
    /// <summary>
    /// Creates a PVO streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 12.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 26.</param>
    /// <param name="signalPeriods">Number of periods for the signal line. Default is 9.</param>
    /// <returns>A PVO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static PvoHub ToPvoHub(
        this IChainProvider<IReusable> chainProvider,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        => new(chainProvider, fastPeriods, slowPeriods, signalPeriods);

    /// <summary>
    /// Creates a PVO streaming hub from a bar provider (extracts Volume).
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast EMA. Default is 12.</param>
    /// <param name="slowPeriods">Number of periods for the slow EMA. Default is 26.</param>
    /// <param name="signalPeriods">Number of periods for the signal line. Default is 9.</param>
    /// <returns>A PVO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static PvoHub ToPvoHub(
        this IBarProvider<IBar> barProvider,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
        => barProvider
            .ToBarPartHub(CandlePart.Volume)
            .ToPvoHub(fastPeriods, slowPeriods, signalPeriods);
}
