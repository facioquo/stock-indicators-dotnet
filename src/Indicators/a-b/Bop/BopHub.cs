namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Balance of Power (BOP).
/// </summary>
public class BopHub
    : ChainHub<IBar, BopResult>, IBop
{
    internal BopHub(
        IBarProvider<IBar> provider,
        int smoothPeriods) : base(provider)
    {
        Bop.Validate(smoothPeriods);
        SmoothPeriods = smoothPeriods;
        Name = $"BOP({smoothPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(smoothPeriods, Name);  // Requires at least SmoothPeriods periods

        Reinitialize();
    }

    /// <inheritdoc/>
    public int SmoothPeriods { get; init; }
    /// <inheritdoc/>
    protected override (BopResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double bop = double.NaN;

        if (i >= SmoothPeriods - 1)
        {
            double sum = 0;

            for (int p = i + 1 - SmoothPeriods; p <= i; p++)
            {
                IBar bar = ProviderCache[p];

                // Calculate raw BOP value: (Close - Open) / (High - Low)
                // Match static series calculation order exactly
                double high = (double)bar.High;
                double low = (double)bar.Low;
                double close = (double)bar.Close;
                double open = (double)bar.Open;

                double range = high - low;
                if (range != 0)
                {
                    double rawBop = (close - open) / range;
                    sum += rawBop;
                }
                else
                {
                    sum += double.NaN;
                }
            }

            bop = double.IsNaN(sum) ? double.NaN : sum / SmoothPeriods;
        }

        // Candidate result
        BopResult r = new(
            Timestamp: item.Timestamp,
            Bop: bop.NaN2Null());

        return (r, i);
    }
}

public static partial class Bop
{
    /// <summary>
    /// Creates a Balance of Power hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing. Default is 14.</param>
    /// <returns>A Balance of Power hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the smooth periods are invalid.</exception>
    public static BopHub ToBopHub(
        this IBarProvider<IBar> chainProvider,
        int smoothPeriods = 14)
        => new(chainProvider, smoothPeriods);
}
