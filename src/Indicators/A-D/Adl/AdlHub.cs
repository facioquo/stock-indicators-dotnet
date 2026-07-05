namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Accumulation/Distribution Line (ADL).
/// </summary>
public class AdlHub : ChainHub<IBar, AdlResult>
{
    private double _previousAdl;

    internal AdlHub(IBarProvider<IBar> provider)
        : base(provider)
    {
        Name = "ADL";
        // Validate cache size for warmup requirements
        ValidateCacheSize(1, Name);  // Requires at least 1 period

        Reinitialize();
    }

    /// <inheritdoc/>
    protected override (AdlResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // candidate result
        AdlResult r = Adl.Increment(
            item.Timestamp,
            item.High,
            item.Low,
            item.Close,
            item.Volume,
            _previousAdl);

        _previousAdl = r.Adl;

        return (r, i);
    }

    /// <summary>
    /// Restores the running ADL sum to the state immediately before the rollback timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex) =>
        // restore from the last cache entry before the rollback point
        _previousAdl = restoreIndex >= 0
            ? Cache[restoreIndex].Adl
            : 0;

    /// <inheritdoc/>
    public override string ToString() => Cache.Count == 0 ? "ADL" : $"ADL({Cache[0].Timestamp:d})";
}

/// <summary>
/// Provides methods for the Accumulation/Distribution Line (ADL) indicator.
/// </summary>
public static partial class Adl
{
    /// <summary>
    /// Creates an AdlHub that is subscribed to an IBarProvider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    public static AdlHub ToAdlHub(
        this IBarProvider<IBar> barProvider)
        => new(barProvider);
}
