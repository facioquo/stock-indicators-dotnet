namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for the rolling median value.
/// </summary>
public class RollingMedianHub
    : ChainHub<IReusable, MathResult>, IRollingMath
{
    private readonly RollingMedianWindow _window;

    internal RollingMedianHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        _window = new RollingMedianWindow(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"ROLLINGMEDIAN({lookbackPeriods})";

        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <summary>
    /// Gets the number of periods in the window, including the current one.
    /// </summary>
    public int LookbackPeriods { get; }

    /// <inheritdoc/>
    protected override (MathResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        return (new MathResult(item.Timestamp, _window.Add(item.Value).NaN2Null()), i);
    }

    /// <summary>
    /// Rebuilds the rolling window from the preserved provider values.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _window.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // the item at restoreIndex is preserved, not replayed, so it is included here
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            _window.Add(ProviderCache[p].Value);
        }
    }
}

public static partial class IndyMath
{
    /// <summary>
    /// Creates a streaming hub for the rolling median value.
    /// </summary>
    /// <remarks>If providers contain historical data, this hub will fast-forward its cache.</remarks>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <returns>A hub of rolling median values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than 1 or exceed the provider's cache size.
    /// </exception>
    public static RollingMedianHub ToRollingMedianHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider, lookbackPeriods);
    }
}
