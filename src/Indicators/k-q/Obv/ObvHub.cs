namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating OBV hubs.
/// </summary>
public class ObvHub : ChainHub<IBar, ObvResult>
{
    internal ObvHub(
        IBarProvider<IBar> provider) : base(provider)
    {
        Name = "OBV";
        // Validate cache size for warmup requirements
        ValidateCacheSize(1, Name);  // Requires at least 1 period

        Reinitialize();
    }

    /// <inheritdoc/>
    protected override (ObvResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Get previous close and OBV values for calculation
        double prevClose = i > 0 ? (double)ProviderCache[i - 1].Close : double.NaN;
        double prevObv = i > 0 ? Cache[i - 1].Value : 0;

        // Calculate OBV using the Increment method
        ObvResult r = Obv.Increment(
            item.Timestamp,
            (double)item.Close,
            (double)item.Volume,
            prevClose,
            prevObv);

        return (r, i);
    }
}

public static partial class Obv
{
    /// <summary>
    /// Converts the bar provider to an OBV hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <returns>An OBV hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    public static ObvHub ToObvHub(
        this IBarProvider<IBar> barProvider)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider);
    }
}
