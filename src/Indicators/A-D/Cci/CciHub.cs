namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Commodity Channel Index (CCI) calculations.
/// </summary>
public class CciHub
    : ChainHub<IBar, CciResult>, ICci
{
    private readonly CciList _cciList;

    internal CciHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods) : base(provider)
    {
        Cci.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"CCI({lookbackPeriods})";
        _cciList = new CciList(lookbackPeriods);

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (CciResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Add current item to CciList
        _cciList.Add(item);

        // Get the latest result from the CciList
        CciResult r = _cciList[^1];

        return (r, i);
    }

    /// <summary>
    /// Restores the CciList state up to the specified timestamp.
    /// Clears and rebuilds _cciList from ProviderCache for Add/Remove operations.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear CciList
        _cciList.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Optimize: only rebuild the rolling window needed for CciList
        // CciList maintains a _tpBuffer of size LookbackPeriods via Queue.Update()
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        // Rebuild CciList from ProviderCache
        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            _cciList.Add(bar);
        }
    }
}

/// <summary>
/// Streaming hub for Commodity Channel Index (CCI).
/// </summary>
public static partial class Cci
{
    /// <summary>
    /// Creates a CCI hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>A CCI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static CciHub ToCciHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 20)
        => new(barProvider, lookbackPeriods);
}
