namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides streaming calculation of the Hurst Exponent indicator.
/// </summary>
public class HurstHub
    : ChainHub<IReusable, HurstResult>, IHurst
{
    private readonly Queue<double> _buffer;
    private readonly double[] _alCorrections;
    private readonly double[] _windowValues;

    internal HurstHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Hurst.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"HURST({lookbackPeriods})";
        _buffer = new Queue<double>(lookbackPeriods + 1);
        _alCorrections = Hurst.PrecomputeAlCorrections(lookbackPeriods);
        _windowValues = new double[lookbackPeriods];

        // Validate cache size for warmup requirements
        // Hurst requires (lookbackPeriods + 1) values in ProviderCache to compute lookbackPeriods returns.
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    protected override (HurstResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Add current value to buffer
        _buffer.Update(LookbackPeriods + 1, item.Value);

        double? h = null;
        double? hAl = null;

        // Need enough periods to calculate Hurst (lookbackPeriods + 1 values to get lookbackPeriods returns)
        if (_buffer.Count == LookbackPeriods + 1)
        {
            // Get evaluation batch - calculate returns from buffer values
            double[] values = _windowValues;

            int x = 0;
            double l = double.NaN;
            bool isFirst = true;

            // Skip first value (used as initial l) and calculate returns for the rest
            foreach (double ps in _buffer)
            {
                if (isFirst)
                {
                    l = ps;
                    isFirst = false;
                    continue;
                }

                // log returns require strictly positive prices on both ends
                values[x] = (l > 0 && ps > 0) ? DeMath.Log(ps / l) : double.NaN;

                l = ps;
                x++;
            }

            // calculate hurst exponent
            (double rawH, double correctedH) = Hurst.CalcHurstWindow(values, _alCorrections);
            h = rawH.NaN2Null();
            hAl = correctedH.NaN2Null();
        }

        // Candidate result
        HurstResult r = new(
            Timestamp: item.Timestamp,
            HurstExponent: h,
            HurstExponentAL: hAl);

        return (r, i);
    }

    /// <summary>
    /// Restores the buffer state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear buffer
        _buffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild buffer from cache
        // We need at most the last (lookbackPeriods + 1) values
        int startIdx = Math.Max(0, restoreIndex + 1 - (LookbackPeriods + 1));

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IReusable item = ProviderCache[p];
            _buffer.Update(LookbackPeriods + 1, item.Value);
        }
    }
}

public static partial class Hurst
{
    /// <summary>
    /// Creates a Hurst Exponent streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation.</param>
    /// <returns>A Hurst hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static HurstHub ToHurstHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 100)
             => new(chainProvider, lookbackPeriods);
}
