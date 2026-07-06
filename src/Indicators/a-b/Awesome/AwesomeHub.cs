namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Awesome Oscillator.
/// </summary>
public class AwesomeHub
    : ChainHub<IReusable, AwesomeResult>, IAwesome
{
    internal AwesomeHub(
        IChainProvider<IReusable> provider,
        int fastPeriods,
        int slowPeriods) : base(provider)
    {
        Awesome.Validate(fastPeriods, slowPeriods);
        FastPeriods = fastPeriods;
        SlowPeriods = slowPeriods;
        Name = $"AWESOME({fastPeriods},{slowPeriods})";

        // Validate cache size for warmup requirements
        // Awesome.Validate() guarantees slowPeriods > fastPeriods, so SlowPeriods drives ToIndicator activation
        int requiredWarmup = slowPeriods;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int FastPeriods { get; init; }

    /// <inheritdoc/>
    public int SlowPeriods { get; init; }
    /// <inheritdoc/>
    protected override (AwesomeResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? oscillator = null;
        double? normalized = null;

        if (i >= SlowPeriods - 1)
        {
            double sumSlow = 0;
            double sumFast = 0;

            for (int p = i + 1 - SlowPeriods; p <= i; p++)
            {
                // Use HL2 when available (IBar), matching static series behavior
                double value = ProviderCache[p].Hl2OrValue();
                sumSlow += value;

                if (p >= i + 1 - FastPeriods)
                {
                    sumFast += value;
                }
            }

            double avgFast = sumFast / FastPeriods;
            double avgSlow = sumSlow / SlowPeriods;
            oscillator = (avgFast - avgSlow).NaN2Null();

            // Use HL2 for current value too
            double currentValue = item.Hl2OrValue();
            normalized = currentValue != 0 ? 100 * oscillator / currentValue : null;
        }

        // Candidate result
        AwesomeResult r = new(
            Timestamp: item.Timestamp,
            Oscillator: oscillator,
            Normalized: normalized);

        return (r, i);
    }
}

public static partial class Awesome
{
    /// <summary>
    /// Creates an Awesome Oscillator hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="fastPeriods">Number of periods for the fast moving average. Default is 5.</param>
    /// <param name="slowPeriods">Number of periods for the slow moving average. Default is 34.</param>
    /// <returns>An Awesome Oscillator hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the periods are invalid.</exception>
    public static AwesomeHub ToAwesomeHub(
        this IChainProvider<IReusable> chainProvider,
        int fastPeriods = 5,
        int slowPeriods = 34)
        => new(chainProvider, fastPeriods, slowPeriods);
}
