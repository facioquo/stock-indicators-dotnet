namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Elder Ray indicator using a stream hub.
/// </summary>
public class ElderRayHub
    : StreamHub<IBar, ElderRayResult>, IElderRay
{
    internal ElderRayHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        ElderRay.Validate(lookbackPeriods);

        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);
        Name = $"ELDER-RAY({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <summary>
    /// Gets the EMA smoothing constant.
    /// </summary>
    private double K { get; init; }

    /// <inheritdoc/>
    protected override (ElderRayResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double ema = i >= LookbackPeriods - 1
            ? i > 0 && Cache[i - 1].Ema is not null

                // normal EMA calculation
                ? Ema.Increment(K, Cache[i - 1].Ema!.Value, (double)item.Close)

                // re/initialize as SMA
                : Sma.Increment(ProviderCache, LookbackPeriods, i)

            // warmup periods are never calculable
            : double.NaN;

        // calculate Elder Ray values
        ElderRayResult r = new(
            Timestamp: item.Timestamp,
            Ema: ema.NaN2Null(),
            BullPower: ema.NaN2Null() is not null ? (double)item.High - ema : null,
            BearPower: ema.NaN2Null() is not null ? (double)item.Low - ema : null);

        return (r, i);
    }
}

public static partial class ElderRay
{
    /// <summary>
    /// Creates an Elder Ray streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation. Default is 13.</param>
    /// <returns>An Elder Ray hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static ElderRayHub ToElderRayHub(
       this IBarProvider<IBar> barProvider,
       int lookbackPeriods = 13)
           => new(barProvider, lookbackPeriods);
}
