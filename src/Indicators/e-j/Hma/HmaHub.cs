namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Hull Moving Average (HMA).
/// </summary>
public class HmaHub
    : ChainHub<IReusable, HmaResult>, IHma
{
    private readonly int wmaN1Periods;
    private readonly int wmaN2Periods;
    private readonly int sqrtPeriods;
    private readonly double divisorN1;
    private readonly double divisorN2;
    private readonly double divisorSqrt;
    private readonly int shiftQty;

    internal HmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Hma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        // initialize periods for nested WMA calculations
        wmaN1Periods = lookbackPeriods;
        wmaN2Periods = lookbackPeriods / 2;
        sqrtPeriods = (int)Math.Sqrt(lookbackPeriods);

        // calculate divisors for WMA
        divisorN1 = (double)wmaN1Periods * (wmaN1Periods + 1) / 2d;
        divisorN2 = (double)wmaN2Periods * (wmaN2Periods + 1) / 2d;
        divisorSqrt = (double)sqrtPeriods * (sqrtPeriods + 1) / 2d;

        shiftQty = lookbackPeriods - 1;

        Name = $"HMA({lookbackPeriods})";

        // Validate cache size for warmup requirements
        // HMA requires lookbackPeriods + sqrtPeriods - 1 samples (see minIndex in CalculateHma)
        ValidateCacheSize(lookbackPeriods + sqrtPeriods - 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (HmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int index = indexHint ?? ProviderCache.IndexOf(item, true);
        double? hma = CalculateHma(index);

        HmaResult result = new(
            Timestamp: item.Timestamp,
            Hma: hma);

        return (result, index);
    }

    private double? CalculateHma(int index)
    {
        int minIndex = shiftQty + sqrtPeriods - 1;
        if (index < minIndex)
        {
            return null;
        }

        double hma = 0d;
        int start = index - sqrtPeriods + 1;

        for (int offset = 0; offset < sqrtPeriods; offset++)
        {
            int synthIndex = start + offset;
            double? wmaN2 = CalculateWma(synthIndex, wmaN2Periods, divisorN2);
            double? wmaN1 = CalculateWma(synthIndex, wmaN1Periods, divisorN1);

            if (!wmaN2.HasValue || !wmaN1.HasValue)
            {
                return null;
            }

            double synthValue = (wmaN2.Value * 2d) - wmaN1.Value;
            hma += synthValue * (offset + 1) / divisorSqrt;
        }

        return hma;
    }

    private double? CalculateWma(int index, int periods, double divisor)
    {
        int start = index - periods + 1;
        if (start < 0)
        {
            return null;
        }

        double wma = 0d;

        for (int offset = 0; offset < periods; offset++)
        {
            double value = ProviderCache[start + offset].Value;
            if (double.IsNaN(value))
            {
                return null;
            }

            wma += value * (offset + 1) / divisor;
        }

        return wma;
    }
}

public static partial class Hma
{
    /// <summary>
    /// Creates an HMA streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An HMA hub.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static HmaHub ToHmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
        => new(chainProvider, lookbackPeriods);
}
