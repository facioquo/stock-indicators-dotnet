namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Detrended Price Oscillator (DPO) from incremental reusable values.
/// </summary>
/// <remarks>
/// DPO calculation at any position relies on data values before and after it in the timeline.
/// Therefore, it can only be calculated after sufficient subsequent data arrives and is
/// retroactively updated. For incremental processing, DPO values are initially null
/// (incalculable) and are updated as enough data becomes available with an offset delay.
/// Results maintain 1:1 correspondence with inputs.
/// </remarks>
public class DpoList : BufferList<DpoResult>, IIncrementFromChain
{
    private readonly SmaList smaList;
    private readonly int offset;
    private readonly List<(DateTime Timestamp, double Value)> buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DpoList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public DpoList(int lookbackPeriods)
    {
        Dpo.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        offset = (lookbackPeriods / 2) + 1;
        smaList = new SmaList(lookbackPeriods);

        // Use List instead of Queue for O(1) indexing (needed for retroactive updates)
        int maxBufferSize = lookbackPeriods + offset;
        buffer = new List<(DateTime, double)>(maxBufferSize);

        Name = $"DPO({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DpoList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public DpoList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <remarks>
    /// Add a single value to the buffer list, calculate and emit DPO result.
    /// DPO calculation is offset-based: DPO[i] = Value[i] - SMA[i + offset].
    /// Results at index (i) represent the detrended value at time (i),
    /// calculated using the centered moving average from time range [i .. i + offset + lookbackPeriods - 1].
    /// Maintains 1:1 correspondence with input - emits null initially, then retroactively updates when sufficient data available.
    /// </remarks>
    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Add the value to the centered SMA buffer
        smaList.Add(timestamp, value);

        // Track value and timestamp in buffer for retroactive updates
        // Buffer maintains 1:1 correspondence with results list
        buffer.Add((timestamp, value));

        // Always emit a result for current position (null initially, retroactively updated)
        AddInternal(new DpoResult(timestamp, null, null));

        // Check if we now have enough data to calculate DPO for a previous position
        // At current index i (Count-1), we can calculate DPO[i - offset]
        // DPO[i - offset] = buffer[i - offset].Value - SMA[i].Sma
        //
        // We need:
        // 1. SMA[i] to be valid (i >= LookbackPeriods - 1, i.e., Count >= LookbackPeriods)
        // 2. Target index (i - offset) to be >= 0
        // 3. Target index to be within the buffer
        if (Count >= LookbackPeriods)
        {
            int currentIndex = Count - 1;
            int targetIndex = currentIndex - offset;

            // Make sure target index is valid
            if (targetIndex >= 0 && targetIndex < buffer.Count)
            {
                // Get the value at the target position from the buffer
                (DateTime targetTimestamp, double targetValue) = buffer[targetIndex];

                // Get the SMA at the current position (most recent SMA)
                // Note: after pruning, smaList may have more items than results list
                SmaResult smaResultAtOffset = smaList[^1];

                double? dpoSma = smaResultAtOffset.Sma;
                double? dpoVal = smaResultAtOffset.Sma.HasValue
                    ? targetValue - smaResultAtOffset.Sma.Value
                    : null;

                // Retroactively update the target position with calculated DPO values
                UpdateInternal(targetIndex, new DpoResult(targetTimestamp, dpoVal, dpoSma));
            }
        }
    }

    /// <summary>
    /// Adds a new reusable value to the DPO list.
    /// </summary>
    /// <param name="value">Reusable value to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <summary>
    /// Adds a list of reusable values to the DPO list.
    /// </summary>
    /// <param name="values">List of reusable values to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the values list is null.</exception>
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        smaList.Clear();
        buffer.Clear();
    }
    /// <summary>
    /// Synchronizes pruning of internal buffers with the parent list.
    /// </summary>
    protected override void PruneList()
    {
        // Call base first to prune the results list
        base.PruneList();

        // Synchronize buffer with pruned results list
        // After pruning, Count reflects the new list size
        // We need to trim buffer from the front to match
        int itemsToRemove = buffer.Count - Count;
        if (itemsToRemove > 0)
        {
            if (itemsToRemove >= buffer.Count)
            {
                buffer.Clear();
            }
            else
            {
                buffer.RemoveRange(0, itemsToRemove);
            }
        }

        // Keep enough SMA history to support DPO offset calculations
        smaList.MaxListSize = Math.Max(LookbackPeriods + offset, MaxListSize + offset);
    }
}

public static partial class Dpo
{
    /// <summary>
    /// Creates a buffer list for Detrended Price Oscillator (DPO) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static DpoList ToDpoList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
