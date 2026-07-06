namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Fisher Transform indicator using a stream hub.
/// </summary>
public class FisherTransformHub
    : ChainHub<IReusable, FisherTransformResult>, IFisherTransform
{

    private readonly List<double> xv = []; // value transform (intermediate state)
    private CircularDoubleBuffer _priceBuffer;

    internal FisherTransformHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods = 10) : base(provider)
    {
        FisherTransform.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _priceBuffer = new CircularDoubleBuffer(lookbackPeriods);
        Name = $"FISHER({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }
    /// <inheritdoc/>
    protected override (FisherTransformResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        while (xv.Count <= i)
        {
            xv.Add(0);
        }

        double currentValue = item.Hl2OrValue();

        if (!double.IsNaN(currentValue))
        {
            _priceBuffer.Add(currentValue);
        }

        double maxPrice;
        double minPrice;

        if (!_priceBuffer.IsEmpty)
        {
            maxPrice = _priceBuffer.GetMax();
            minPrice = _priceBuffer.GetMin();
        }
        else
        {
            maxPrice = currentValue;
            minPrice = currentValue;
        }

        double fisher;
        double? trigger = null;

        if (i > 0)
        {
            xv[i] = maxPrice - minPrice != 0
                ? (0.33 * 2 * (((currentValue - minPrice) / (maxPrice - minPrice)) - 0.5))
                      + (0.67 * xv[i - 1])
                : 0d;

            xv[i] = xv[i] > 0.99 ? 0.999 : xv[i];
            xv[i] = xv[i] < -0.99 ? -0.999 : xv[i];

            fisher = DeMath.Atanh(xv[i]) + (0.5d * Cache[i - 1].Fisher);

            trigger = Cache[i - 1].Fisher;
        }
        else
        {
            xv[i] = 0;
            fisher = 0;
        }

        FisherTransformResult r = new(
            Timestamp: item.Timestamp,
            Fisher: fisher,
            Trigger: trigger);

        return (r, i);
    }

    /// <summary>
    /// Restores the rolling window and xv state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        _priceBuffer.Clear();

        if (restoreIndex < 0)
        {
            xv.Clear();
            return;
        }

        int keepCount = restoreIndex + 1;
        if (keepCount < xv.Count)
        {
            xv.RemoveRange(keepCount, xv.Count - keepCount);
        }

        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);

        for (int p = startIdx; p <= restoreIndex; p++)
        {
            double priceValue = ProviderCache[p].Hl2OrValue();

            if (!double.IsNaN(priceValue))
            {
                _priceBuffer.Add(priceValue);
            }
        }
    }

    /// <inheritdoc/>
    protected override void PruneState(DateTime toTimestamp)
    {
        int targetSize = Cache.Count;
        if (xv.Count > targetSize)
        {
            int excessCount = xv.Count - targetSize;
            xv.RemoveRange(0, excessCount);
        }
    }
}

public static partial class FisherTransform
{
    /// <summary>
    /// Creates a Fisher Transform streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Number of periods to look back for the calculation. Default is 10.</param>
    /// <returns>A Fisher Transform hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static FisherTransformHub ToFisherTransformHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 10)
             => new(chainProvider, lookbackPeriods);
}
