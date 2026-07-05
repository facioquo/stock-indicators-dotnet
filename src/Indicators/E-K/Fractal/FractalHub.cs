namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Williams Fractal indicator using a stream hub.
/// </summary>
public class FractalHub
    : StreamHub<IBar, FractalResult>, IFractal
{
    internal FractalHub(
        IBarProvider<IBar> provider,
        int windowSpan,
        EndType endType) : this(provider, windowSpan, windowSpan, endType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalHub"/> class with different left and right spans.
    /// </summary>
    /// <param name="provider">Bar provider.</param>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation.</param>
    internal FractalHub(
        IBarProvider<IBar> provider,
        int leftSpan,
        int rightSpan,
        EndType endType) : base(provider)
    {
        // Validate both spans independently
        Fractal.Validate(leftSpan);
        Fractal.Validate(rightSpan);

        LeftSpan = leftSpan;
        RightSpan = rightSpan;
        EndType = endType;
        Name = $"FRACTAL({leftSpan},{rightSpan},{endType.ToString().ToUpperInvariant()})";

        // Validate cache size for warmup requirements
        int requiredWarmup = leftSpan + rightSpan + 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LeftSpan { get; init; }

    /// <inheritdoc/>
    public int RightSpan { get; init; }

    /// <inheritdoc/>
    public EndType EndType { get; init; }

    /// <inheritdoc/>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            // First, add the current item
            base.OnAdd(item, notify, indexHint);

            // For forward-looking indicators, we need to recalculate the position
            // that now has sufficient right context (i - RightSpan)
            int i = indexHint ?? ProviderCache.IndexOf(item, true);
            int targetIndex = i - RightSpan;

            // Only recalculate if the target position is valid and needs calculation
            if (targetIndex >= LeftSpan && targetIndex < Cache.Count)
            {
                FractalResult existing = Cache[targetIndex];

                // Only recalculate if the result was previously null (insufficient context)
                if (existing.FractalBear is null && existing.FractalBull is null)
                {
                    (FractalResult updated, int _) = ToIndicator(ProviderCache[targetIndex], targetIndex);
                    Cache[targetIndex] = updated;

                    // Notify observers if a fractal was actually found and notify is enabled
                    if (notify && (updated.FractalBear is not null || updated.FractalBull is not null))
                    {
                        NotifyObserversOnRebuild(updated.Timestamp);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override (FractalResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // For Fractal, we calculate the value for the CURRENT bar (at index i)
        // but we need to check if we have sufficient left AND right context
        // Series logic: i + 1 > leftSpan && i + 1 <= length - rightSpan

        int length = ProviderCache.Count;
        decimal? fractalBear = null;
        decimal? fractalBull = null;

        // Can we calculate fractal for the bar at index i?
        // We need LeftSpan bars before it and RightSpan bars after it
        if (i + 1 > LeftSpan && i + 1 <= length - RightSpan)
        {
            IBar center = ProviderCache[i];
            bool isHigh = true;
            bool isLow = true;

            decimal evalHigh = EndType == EndType.Close ? center.Close : center.High;
            decimal evalLow = EndType == EndType.Close ? center.Close : center.Low;

            // Compare center with wings (both left and right)
            for (int p = i - LeftSpan; p <= i + RightSpan; p++)
            {
                // Skip center
                if (p == i)
                {
                    continue;
                }

                IBar wing = ProviderCache[p];
                decimal wingHigh = EndType == EndType.Close ? wing.Close : wing.High;
                decimal wingLow = EndType == EndType.Close ? wing.Close : wing.Low;

                if (evalHigh <= wingHigh)
                {
                    isHigh = false;
                }

                if (evalLow >= wingLow)
                {
                    isLow = false;
                }
            }

            fractalBear = isHigh ? evalHigh : null;
            fractalBull = isLow ? evalLow : null;
        }

        // Create result for the current bar
        FractalResult result = new(ProviderCache[i].Timestamp, fractalBear, fractalBull);

        return (result, i);
    }
}

public static partial class Fractal
{
    /// <summary>
    /// Creates a Fractal hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="windowSpan">Window span used for both left and right spans.</param>
    /// <param name="endType">Price end type to use.</param>
    /// <returns>An instance of <see cref="FractalHub"/>.</returns>
    public static FractalHub ToFractalHub(
       this IBarProvider<IBar> barProvider,
       int windowSpan = 2,
       EndType endType = EndType.HighLow)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, windowSpan, endType);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractalHub"/> class with different left and right spans.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="leftSpan">Number of periods to look back for the calculation.</param>
    /// <param name="rightSpan">Number of periods to look forward for the calculation.</param>
    /// <param name="endType">Type of price to use for the calculation. Default is <see cref="EndType.HighLow"/>.</param>
    /// <returns>An instance of <see cref="FractalHub"/>.</returns>
    public static FractalHub ToFractalHub(
       this IBarProvider<IBar> barProvider,
       int leftSpan,
       int rightSpan,
       EndType endType = EndType.HighLow)
    {
        ArgumentNullException.ThrowIfNull(barProvider);
        return new(barProvider, leftSpan, rightSpan, endType);
    }
}
