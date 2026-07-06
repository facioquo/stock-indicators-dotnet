namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a hub for Aroon Oscillator calculations.
/// </summary>
public class AroonHub
    : ChainHub<IBar, AroonResult>, IAroon
{
    internal AroonHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Aroon.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"AROON({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods + 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (AroonResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? aroonUp = null;
        double? aroonDown = null;

        // Calculate Aroon when we have enough data
        if (i >= LookbackPeriods)
        {
            decimal lastHighPrice = 0;
            decimal lastLowPrice = decimal.MaxValue;
            int lastHighIndex = 0;
            int lastLowIndex = 0;

            // Look back over the specified period
            for (int p = i - LookbackPeriods; p <= i; p++)
            {
                IBar d = ProviderCache[p];

                if (d.High > lastHighPrice)
                {
                    lastHighPrice = d.High;
                    lastHighIndex = p;
                }

                if (d.Low < lastLowPrice)
                {
                    lastLowPrice = d.Low;
                    lastLowIndex = p;
                }
            }

            aroonUp = 100d * (LookbackPeriods - (i - lastHighIndex)) / LookbackPeriods;
            aroonDown = 100d * (LookbackPeriods - (i - lastLowIndex)) / LookbackPeriods;
        }

        // Candidate result
        AroonResult r = new(
            Timestamp: item.Timestamp,
            AroonUp: aroonUp,
            AroonDown: aroonDown,
            Oscillator: aroonUp - aroonDown);

        return (r, i);
    }
}

/// <summary>
/// Streaming hub for Aroon Oscillator.
/// </summary>
public static partial class Aroon
{
    /// <summary>
    /// Creates an Aroon hub from a bar provider.
    /// </summary>
    /// <param name="provider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An Aroon hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static AroonHub ToAroonHub(
        this IBarProvider<IBar> provider,
        int lookbackPeriods = 25)
        => new(provider, lookbackPeriods);
}
