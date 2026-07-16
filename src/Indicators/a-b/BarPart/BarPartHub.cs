namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for managing bar parts.
/// </summary>
public class BarPartHub
    : ChainHub<IBar, TimeValue>, IBarPart
{
    internal BarPartHub(
        IBarProvider<IBar> provider,
        CandlePart candlePart
    ) : base(provider)
    {
        CandlePartSelection = candlePart;

        Reinitialize();
    }

    /// <inheritdoc/>
    public CandlePart CandlePartSelection { get; init; }

    /// <inheritdoc/>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        // Lock to prevent concurrent cache access.
        lock (CacheLock)
        {
            (TimeValue result, int index) = ToIndicator(item, indexHint);

            if (index >= 0 && index < Cache.Count)
            {
                InsertWithoutRebuild(result, index, notify);
                return;
            }

            AppendCache(result, notify);
        }
    }


    /// <inheritdoc/>
    protected override (TimeValue result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        // candidate result
        TimeValue r
            = item.ToBarPart(CandlePartSelection);

        return (r, indexHint ?? Cache.Count);
    }

    /// <inheritdoc/>
    public override string ToString()
        => $"BAR-PART({CandlePartSelection.ToString().ToUpperInvariant()})";
}

public static partial class BarParts
{
    /// <summary>
    /// Creates an BarPart streaming hub from a chain provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <returns>An new <see cref="BarPartHub"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="candlePart"/> invalid.</exception>
    public static BarPartHub ToBarPartHub(
        this IBarProvider<IBar> barProvider,
        CandlePart candlePart)
        => new(barProvider, candlePart);
}
