namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Rate of Change with Bands (RocWb).
/// </summary>
public class RocWbHub
    : ChainHub<IReusable, RocWbResult>, IRocWb
{
    private readonly double k;
    private double prevEma = double.NaN;
    private readonly Queue<double> rocSqBuffer;
    private readonly Queue<double> rocEmaInitBuffer;

    internal RocWbHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        int emaPeriods,
        int stdDevPeriods) : base(provider)
    {
        RocWb.Validate(lookbackPeriods, emaPeriods, stdDevPeriods);
        LookbackPeriods = lookbackPeriods;
        EmaPeriods = emaPeriods;
        StdDevPeriods = stdDevPeriods;
        k = 2d / (emaPeriods + 1);
        rocSqBuffer = new Queue<double>(stdDevPeriods);
        rocEmaInitBuffer = new Queue<double>(emaPeriods);
        Name = $"ROCWB({lookbackPeriods},{emaPeriods},{stdDevPeriods})";

        // Validate cache size for warmup requirements
        // RocWb needs lookbackPeriods items for the first ROC value, then Max(emaPeriods, stdDevPeriods)
        // more items to fill the EMA/StdDev buffers — total item count, not a zero-based index.
        int requiredWarmup = lookbackPeriods + Math.Max(emaPeriods, stdDevPeriods);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public int EmaPeriods { get; init; }

    /// <inheritdoc/>
    public int StdDevPeriods { get; init; }
    /// <inheritdoc/>
    protected override (RocWbResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate ROC
        double roc;
        if (i >= LookbackPeriods)
        {
            IReusable back = ProviderCache[i - LookbackPeriods];
            roc = back.Value != 0
                ? 100d * (item.Value - back.Value) / back.Value
                : double.NaN;
        }
        else
        {
            roc = double.NaN;
        }

        // Update the ROC squared buffer for std dev calculation
        double rocSq = roc * roc;
        if (!double.IsNaN(roc))
        {
            if (rocSqBuffer.Count >= StdDevPeriods)
            {
                rocSqBuffer.Dequeue();
            }

            rocSqBuffer.Enqueue(rocSq);
        }

        // Calculate EMA of ROC
        double rocEma;
        if (double.IsNaN(prevEma))
        {
            // Accumulate ROC values for initial EMA calculation
            if (!double.IsNaN(roc))
            {
                if (rocEmaInitBuffer.Count >= EmaPeriods)
                {
                    rocEmaInitBuffer.Dequeue();
                }

                rocEmaInitBuffer.Enqueue(roc);
            }

            // Initialize EMA with SMA when we have enough values
            if (rocEmaInitBuffer.Count >= EmaPeriods)
            {
                double sum = 0;
                foreach (double value in rocEmaInitBuffer)
                {
                    sum += value;
                }

                rocEma = sum / rocEmaInitBuffer.Count;
            }
            else
            {
                rocEma = double.NaN;
            }
        }
        else
        {
            // Normal EMA calculation
            rocEma = Ema.Increment(k, prevEma, roc);
        }

        prevEma = rocEma;

        // Calculate RMS deviation bands
        double? rocDev = null;
        if (rocSqBuffer.Count >= StdDevPeriods)
        {
            double sumSq = 0;
            foreach (double value in rocSqBuffer)
            {
                sumSq += value;
            }

            rocDev = Math.Sqrt(sumSq / StdDevPeriods).NaN2Null();
        }

        // candidate result
        RocWbResult r = new(
            Timestamp: item.Timestamp,
            Roc: roc.NaN2Null(),
            RocEma: rocEma.NaN2Null(),
            UpperBand: rocDev,
            LowerBand: rocDev.HasValue ? -rocDev.Value : null);

        return (r, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear state
        prevEma = double.NaN;
        rocSqBuffer.Clear();
        rocEmaInitBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Replay state up to restoreIndex
        for (int p = 0; p <= restoreIndex; p++)
        {
            IReusable current = ProviderCache[p];

            // Calculate ROC
            double roc;
            if (p >= LookbackPeriods)
            {
                IReusable back = ProviderCache[p - LookbackPeriods];
                roc = back.Value != 0
                    ? 100d * (current.Value - back.Value) / back.Value
                    : double.NaN;
            }
            else
            {
                roc = double.NaN;
            }

            // Update ROC squared buffer
            double rocSq = roc * roc;
            if (!double.IsNaN(roc))
            {
                if (rocSqBuffer.Count >= StdDevPeriods)
                {
                    rocSqBuffer.Dequeue();
                }

                rocSqBuffer.Enqueue(rocSq);
            }

            // Calculate EMA of ROC
            double rocEma;
            if (double.IsNaN(prevEma))
            {
                // Accumulate ROC values for initial EMA calculation
                if (!double.IsNaN(roc))
                {
                    if (rocEmaInitBuffer.Count >= EmaPeriods)
                    {
                        rocEmaInitBuffer.Dequeue();
                    }

                    rocEmaInitBuffer.Enqueue(roc);
                }

                // Initialize EMA with SMA when we have enough values
                if (rocEmaInitBuffer.Count >= EmaPeriods)
                {
                    double sum = 0;
                    foreach (double value in rocEmaInitBuffer)
                    {
                        sum += value;
                    }

                    rocEma = sum / rocEmaInitBuffer.Count;
                }
                else
                {
                    rocEma = double.NaN;
                }
            }
            else
            {
                // Normal EMA calculation
                rocEma = Ema.Increment(k, prevEma, roc);
            }

            prevEma = rocEma;
        }
    }
}

public static partial class RocWb
{
    /// <summary>
    /// Creates a RocWb streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window for ROC.</param>
    /// <param name="emaPeriods">Quantity of periods for EMA smoothing.</param>
    /// <param name="stdDevPeriods">Quantity of periods for standard deviation bands.</param>
    /// <returns>A RocWb hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the parameters are invalid.</exception>
    public static RocWbHub ToRocWbHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 20,
        int emaPeriods = 5,
        int stdDevPeriods = 5)
        => new(chainProvider, lookbackPeriods, emaPeriods, stdDevPeriods);
}
