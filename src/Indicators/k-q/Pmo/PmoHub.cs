namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Price Momentum Oscillator (PMO).
/// </summary>
public class PmoHub
    : ChainHub<IReusable, PmoResult>, IPmo
{
    private readonly double smoothingConstant1;
    private readonly double smoothingConstant2;
    private readonly double smoothingConstant3;

    private double _prevRocEma;

    // History lists for SMA seed initialization at warmup boundaries
    private readonly List<double> _rocHistory;
    private readonly List<double> _rocEmaHistory;  // scaled ×10

    internal PmoHub(
        IChainProvider<IReusable> provider,
        int timePeriods,
        int smoothPeriods,
        int signalPeriods) : base(provider)
    {
        Pmo.Validate(timePeriods, smoothPeriods, signalPeriods);
        TimePeriods = timePeriods;
        SmoothPeriods = smoothPeriods;
        SignalPeriods = signalPeriods;

        smoothingConstant1 = 2d / smoothPeriods;
        smoothingConstant2 = 2d / timePeriods;
        smoothingConstant3 = 2d / (signalPeriods + 1);

        Name = $"PMO({timePeriods},{smoothPeriods},{signalPeriods})";

        _prevRocEma = double.NaN;
        _rocHistory = [];
        _rocEmaHistory = [];

        // Validate cache size for warmup requirements
        // PMO signal first appears at index (timePeriods + smoothPeriods + signalPeriods - 2),
        // requiring timePeriods + smoothPeriods + signalPeriods - 1 items.
        int requiredWarmup = timePeriods + smoothPeriods + signalPeriods - 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int TimePeriods { get; init; }

    /// <inheritdoc/>
    public int SmoothPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }
    /// <inheritdoc/>
    protected override (PmoResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double currentValue = item.Value;
        double prevValue = i > 0 ? ProviderCache[i - 1].Value : double.NaN;

        // Rate of change (ROC)
        double roc = prevValue == 0 ? double.NaN : 100 * ((currentValue / prevValue) - 1);
        _rocHistory.Add(roc);

        // ROC EMA — re/initialize as SMA seed, then O(1) incremental
        double rocEma;

        if (double.IsNaN(_prevRocEma) && i >= TimePeriods)
        {
            double sum = 0;
            for (int p = i - TimePeriods + 1; p <= i; p++)
            {
                sum += _rocHistory[p];
            }

            rocEma = sum / TimePeriods;
        }
        else if (!double.IsNaN(_prevRocEma))
        {
            rocEma = _prevRocEma + (smoothingConstant2 * (roc - _prevRocEma));
        }
        else
        {
            rocEma = double.NaN;
        }

        _prevRocEma = rocEma;
        double rocEmaScaled = rocEma * 10;
        _rocEmaHistory.Add(rocEmaScaled);

        // PMO — re/initialize as SMA seed, then O(1) incremental via Cache reference
        double prevPmo = i > 0 ? Cache[i - 1].Pmo ?? double.NaN : double.NaN;
        double pmoValue;

        if (double.IsNaN(prevPmo) && i >= SmoothPeriods + TimePeriods - 1)
        {
            double sum = 0;
            for (int p = i - SmoothPeriods + 1; p <= i; p++)
            {
                sum += _rocEmaHistory[p];
            }

            pmoValue = sum / SmoothPeriods;
        }
        else if (!double.IsNaN(prevPmo))
        {
            pmoValue = prevPmo + (smoothingConstant1 * (rocEmaScaled - prevPmo));
        }
        else
        {
            pmoValue = double.NaN;
        }

        // Signal — re/initialize as SMA seed of PMO, then O(1) incremental via Cache reference
        double prevSignal = i > 0 ? Cache[i - 1].Signal ?? double.NaN : double.NaN;
        double signalValue;

        if (double.IsNaN(prevSignal) && i >= SignalPeriods + SmoothPeriods + TimePeriods - 2)
        {
            double sum = 0;
            for (int j = i - SignalPeriods + 1; j < i; j++)
            {
                sum += Cache[j].Pmo ?? double.NaN;
            }

            sum += pmoValue;
            signalValue = sum / SignalPeriods;
        }
        else
        {
            signalValue = Ema.Increment(smoothingConstant3, prevSignal, pmoValue);
        }

        // Candidate result
        PmoResult r = new(
            Timestamp: item.Timestamp,
            Pmo: pmoValue.NaN2Null(),
            Signal: signalValue.NaN2Null());

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _prevRocEma = double.NaN;
        _rocHistory.Clear();
        _rocEmaHistory.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Replay items 0..restoreIndex to rebuild _prevRocEma,
        // _rocHistory, and _rocEmaHistory so the next ToIndicator
        // call continues with correct EMA state.
        for (int i = 0; i <= restoreIndex; i++)
        {
            double currVal = ProviderCache[i].Value;
            double prevVal = i > 0 ? ProviderCache[i - 1].Value : double.NaN;

            double roc = prevVal == 0 ? double.NaN : 100 * ((currVal / prevVal) - 1);
            _rocHistory.Add(roc);

            double rocEma;

            if (double.IsNaN(_prevRocEma) && i >= TimePeriods)
            {
                double sum = 0;
                for (int p = i - TimePeriods + 1; p <= i; p++)
                {
                    sum += _rocHistory[p];
                }

                rocEma = sum / TimePeriods;
            }
            else if (!double.IsNaN(_prevRocEma))
            {
                rocEma = _prevRocEma + (smoothingConstant2 * (roc - _prevRocEma));
            }
            else
            {
                rocEma = double.NaN;
            }

            _prevRocEma = rocEma;
            _rocEmaHistory.Add(rocEma * 10);
        }
    }

    /// <inheritdoc/>
    protected override void PruneState(DateTime toTimestamp)
    {
        // Keep history lists aligned with Cache after provider-driven pruning.
        int targetSize = Cache.Count;

        if (_rocHistory.Count > targetSize)
        {
            int excessCount = _rocHistory.Count - targetSize;
            _rocHistory.RemoveRange(0, excessCount);
            _rocEmaHistory.RemoveRange(0, excessCount);
        }
    }
}

public static partial class Pmo
{
    /// <summary>
    /// Creates a PMO streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="timePeriods">Number of periods for the time span.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <returns>A PMO hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static PmoHub ToPmoHub(
        this IChainProvider<IReusable> chainProvider,
        int timePeriods = 35,
        int smoothPeriods = 20,
        int signalPeriods = 10)
        => new(chainProvider, timePeriods, smoothPeriods, signalPeriods);
}
