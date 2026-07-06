namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a stream hub for calculating the Ichimoku Cloud indicator.
/// </summary>
public class IchimokuHub
    : StreamHub<IBar, IchimokuResult>, IIchimoku
{

    private CircularDoubleBuffer tenkanHighBuffer;
    private CircularDoubleBuffer tenkanLowBuffer;
    private CircularDoubleBuffer kijunHighBuffer;
    private CircularDoubleBuffer kijunLowBuffer;

    internal IchimokuHub(
        IBarProvider<IBar> provider,
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int senkouOffset,
        int chikouOffset)
        : base(provider)
    {
        Ichimoku.Validate(
            tenkanPeriods,
            kijunPeriods,
            senkouBPeriods,
            senkouOffset,
            chikouOffset);

        TenkanPeriods = tenkanPeriods;
        KijunPeriods = kijunPeriods;
        SenkouBPeriods = senkouBPeriods;
        SenkouOffset = senkouOffset;
        ChikouOffset = chikouOffset;

        Name = $"ICHIMOKU({tenkanPeriods},{kijunPeriods},{senkouBPeriods})";

        // Validate cache size for warmup requirements
        // Ichimoku needs the maximum of all component periods plus the Senkou offset.
        int requiredWarmup = Math.Max(Math.Max(tenkanPeriods, kijunPeriods), senkouBPeriods) + senkouOffset;
        ValidateCacheSize(requiredWarmup, Name);

        tenkanHighBuffer = new CircularDoubleBuffer(tenkanPeriods);
        tenkanLowBuffer = new CircularDoubleBuffer(tenkanPeriods);
        kijunHighBuffer = new CircularDoubleBuffer(kijunPeriods);
        kijunLowBuffer = new CircularDoubleBuffer(kijunPeriods);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int TenkanPeriods { get; init; }

    /// <inheritdoc/>
    public int KijunPeriods { get; init; }

    /// <inheritdoc/>
    public int SenkouBPeriods { get; init; }

    /// <inheritdoc/>
    public int SenkouOffset { get; init; }

    /// <inheritdoc/>
    public int ChikouOffset { get; init; }
    /// <summary>
    /// Handles adding a new bar and updates past results that now have sufficient data for ChikouSpan.
    /// </summary>
    /// <param name="item">New item from provider.</param>
    /// <param name="notify">Whether to notify observers.</param>
    /// <param name="indexHint">Provider index hint.</param>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        lock (CacheLock)
        {
            // Add the new item normally
            base.OnAdd(item, notify, indexHint);

            // Update past result that can now have its ChikouSpan calculated
            // This happens when a new bar provides the forward data needed
            if (ChikouOffset > 0 && Cache.Count > ChikouOffset && ProviderCache.Count > ChikouOffset)
            {
                int providerIndex = indexHint ?? ProviderCache.IndexOf(item, true);

                // Find the past result that should be updated
                // It's the result that is ChikouOffset periods before the current item
                int backfillProviderIndex = providerIndex - ChikouOffset;

                if (backfillProviderIndex >= 0 && backfillProviderIndex < ProviderCache.Count)
                {
                    // Find the past result in Cache by timestamp (not by index)
                    DateTime backfillTimestamp = ProviderCache[backfillProviderIndex].Timestamp;
                    int backfillCacheIndex = Cache.IndexOf(backfillTimestamp, false);

                    if (backfillCacheIndex >= 0 && backfillCacheIndex < Cache.Count)
                    {
                        IchimokuResult pastResult = Cache[backfillCacheIndex];
                        double chikouClose = (double)ProviderCache[providerIndex].Close;

                        // Update if ChikouSpan is null or value has changed (forward bar revision)
                        // During rebuilds (notify=false), always update to ensure correctness
                        if (!notify || pastResult.ChikouSpan is null || pastResult.ChikouSpan != chikouClose)
                        {
                            // Update the past result
                            Cache[backfillCacheIndex] = pastResult with { ChikouSpan = chikouClose };

                            // Notify observers when value changes during normal streaming
                            if (notify && pastResult.ChikouSpan.HasValue && pastResult.ChikouSpan != chikouClose)
                            {
                                NotifyObserversOnRebuild(backfillTimestamp);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    protected override (IchimokuResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Add current bar to rolling buffers
        tenkanHighBuffer.Add((double)item.High);
        tenkanLowBuffer.Add((double)item.Low);
        kijunHighBuffer.Add((double)item.High);
        kijunLowBuffer.Add((double)item.Low);

        // Calculate Tenkan-sen (conversion line)
        double? tenkanSen = null;
        if (i >= TenkanPeriods - 1)
        {
            double max = tenkanHighBuffer.GetMax();
            double min = tenkanLowBuffer.GetMin();
            tenkanSen = (min + max) / 2d;
        }

        // Calculate Kijun-sen (base line)
        double? kijunSen = null;
        if (i >= KijunPeriods - 1)
        {
            double max = kijunHighBuffer.GetMax();
            double min = kijunLowBuffer.GetMin();
            kijunSen = (min + max) / 2d;
        }

        // Calculate Senkou Span A (leading span A)
        double? senkouSpanA = null;
        int senkouStartPeriod = Math.Max(
            2 * SenkouOffset,
            Math.Max(TenkanPeriods, KijunPeriods)) - 1;

        if (i >= senkouStartPeriod)
        {
            if (SenkouOffset == 0)
            {
                senkouSpanA = (tenkanSen + kijunSen) / 2d;
            }
            else
            {
                IchimokuResult skq = Cache[i - SenkouOffset];
                senkouSpanA = (skq.TenkanSen + skq.KijunSen) / 2d;
            }
        }

        // Calculate Senkou Span B (leading span B)
        double? senkouSpanB = null;
        if (i >= SenkouOffset + SenkouBPeriods - 1)
        {
            double max = double.MinValue;
            double min = double.MaxValue;

            for (int p = i - SenkouOffset - SenkouBPeriods + 1;
                 p <= i - SenkouOffset; p++)
            {
                IBar d = ProviderCache[p];

                if ((double)d.High > max)
                {
                    max = (double)d.High;
                }

                if ((double)d.Low < min)
                {
                    min = (double)d.Low;
                }
            }

            senkouSpanB = min == double.MaxValue ? null : (min + max) / 2d;
        }

        // Calculate Chikou Span (lagging span)
        // This is forward-looking: ChikouSpan at index i uses Close from index i + ChikouOffset
        double? chikouSpan = null;
        int chikouIndex = i + ChikouOffset;
        if (chikouIndex < ProviderCache.Count)
        {
            chikouSpan = (double)ProviderCache[chikouIndex].Close;
        }

        // Create result
        IchimokuResult r = new(
            Timestamp: item.Timestamp,
            TenkanSen: tenkanSen,
            KijunSen: kijunSen,
            SenkouSpanA: senkouSpanA,
            SenkouSpanB: senkouSpanB,
            ChikouSpan: chikouSpan);

        return (r, i);
    }

    /// <summary>
    /// Restores the rolling window states up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear all buffers
        tenkanHighBuffer.Clear();
        tenkanLowBuffer.Clear();
        kijunHighBuffer.Clear();
        kijunLowBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild Tenkan buffers
        int tenkanStart = Math.Max(0, restoreIndex - TenkanPeriods + 1);
        for (int p = tenkanStart; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            tenkanHighBuffer.Add((double)bar.High);
            tenkanLowBuffer.Add((double)bar.Low);
        }

        // Rebuild Kijun buffers
        int kijunStart = Math.Max(0, restoreIndex - KijunPeriods + 1);
        for (int p = kijunStart; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            kijunHighBuffer.Add((double)bar.High);
            kijunLowBuffer.Add((double)bar.Low);
        }

        // After Remove operations, ChikouSpan values that point past the end need to be nulled
        // Calculate which results might have invalid ChikouSpan values
        if (ChikouOffset > 0 && Cache.Count > 0)
        {
            int firstInvalidIndex = ProviderCache.Count - ChikouOffset;

            // Null out ChikouSpan for results that now reference beyond ProviderCache
            for (int i = Math.Max(0, firstInvalidIndex); i < Cache.Count; i++)
            {
                IchimokuResult result = Cache[i];
                if (result.ChikouSpan is not null)
                {
                    Cache[i] = result with { ChikouSpan = null };
                }
            }
        }
    }

}

public static partial class Ichimoku
{
    /// <summary>
    /// Converts a bar provider to an Ichimoku hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <returns>An Ichimoku hub.</returns>
    public static IchimokuHub ToIchimokuHub(
        this IBarProvider<IBar> barProvider,
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52)
        => new(barProvider, tenkanPeriods, kijunPeriods, senkouBPeriods, kijunPeriods, kijunPeriods);

    /// <summary>
    /// Converts a bar provider to an Ichimoku hub with specified parameters.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="offsetPeriods">Number of periods for the offset.</param>
    /// <returns>An Ichimoku hub.</returns>
    public static IchimokuHub ToIchimokuHub(
        this IBarProvider<IBar> barProvider,
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int offsetPeriods)
        => new(barProvider, tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods, offsetPeriods);
}
