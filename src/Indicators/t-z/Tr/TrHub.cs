namespace FacioQuo.Stock.Indicators;

/// <inheritdoc />
public class TrHub
    : ChainHub<IBar, TrResult>
{
    internal TrHub(IBarProvider<IBar> provider)
        : base(provider)
    {
        Name = "TRUE RANGE";
        // Validate cache size for warmup requirements
        ValidateCacheSize(2, Name);  // Requires current + previous period

        Reinitialize();
    }

    /// <inheritdoc/>
    protected override (TrResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // skip first period
        if (i == 0)
        {
            return (new TrResult(item.Timestamp, null), i);
        }

        IBar prev = ProviderCache[i - 1];

        // candidate result
        TrResult r = new(
            item.Timestamp,
            Tr.Increment(
                (double)item.High,
                (double)item.Low,
                (double)prev.Close));

        return (r, i);
    }
}

public static partial class Tr
{
    /// <summary>
    /// Converts a bar provider to a True Range (TR) hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <returns>A True Range (TR) hub.</returns>
    public static TrHub ToTrHub(
        this IBarProvider<IBar> barProvider)
             => new(barProvider);
}
