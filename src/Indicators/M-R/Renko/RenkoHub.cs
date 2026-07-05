namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for generating Renko chart series in a streaming manner.
/// </summary>
public class RenkoHub
    : BarProvider<IBar, RenkoResult>, IRenko
{

    private RenkoResult lastBrick
        = new(default, default, default,
            default, default, default, default);

    // Track the last provider index used to form the lastBrick
    // This allows aggregating bars even when provider cache is pruned
    private int lastBrickProviderIndex = -1;

    internal RenkoHub(
        IBarProvider<IBar> provider,
        decimal brickSize,
        EndType endType) : base(provider)
    {
        Renko.Validate(brickSize);
        BrickSize = brickSize;
        EndType = endType;
        Name = $"RENKO({brickSize},{endType.ToString().ToUpperInvariant()})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(2, Name);  // Renko needs minimum history for brick formation

        Reinitialize();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Renko is a transformation hub that doesn't have 1:1 timestamp alignment with input bars.
    /// Bricks are only created when price moves by brickSize, so not every bar produces a brick.
    /// Provider-driven pruning would incorrectly remove bricks, so Renko opts out.
    /// </remarks>
    protected override bool ShouldPruneOnProviderPrune => false;

    /// <summary>
    /// Renko hub settings. Since it can produce 0 or many bricks per bar,
    /// the default 1:1 in/out is not used and must be skipped to prevent
    /// same-date triggered rebuilds when caching.
    /// </summary>
    public override BinarySettings Properties { get; init; } = new(0b00000010);  // custom

    /// <inheritdoc/>
    public decimal BrickSize { get; }

    /// <inheritdoc/>
    public EndType EndType { get; }
    /// <inheritdoc/>
    public override void OnAdd(IBar item, bool notify, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        lock (CacheLock)
        {
            ToIndicator(item, notify, indexHint);
        }
    }

    /// <inheritdoc />
    protected override (RenkoResult result, int index)
        ToIndicator(IBar item, int? indexHint)
        => throw new InvalidOperationException(); // not used

    /// <summary>
    /// Restores the last brick marker to the state at the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // restore last brick marker from cache
        if (Cache.Count != 0 && restoreIndex >= 0)
        {
            DateTime preserveTimestamp = ProviderCache[restoreIndex].Timestamp;

            RenkoResult? brick = Cache
                .LastOrDefault(c => c.Timestamp <= preserveTimestamp);

            if (brick is not null)
            {
                lastBrick = brick;

                // Restore the provider index corresponding to this brick
                // Use IndexOf to find the index in the (potentially pruned) provider cache
                lastBrickProviderIndex = ProviderCache.IndexOf(brick.Timestamp, true);

                // If not found (brick predates pruned cache), use first available index
                if (lastBrickProviderIndex == -1)
                {
                    lastBrickProviderIndex = 0;
                }

                return;
            }

            // no brick before timestamp - reset to baseline
        }

        // reset baseline if we have provider data
        if (ProviderCache.Count > 1)
        {
            SetBaselineBrick();
            lastBrickProviderIndex = 0;
        }
    }



    /// <summary>
    /// re/initialize last brick marker
    /// </summary>
    private void SetBaselineBrick()
    {
        int decimals = BrickSize.GetDecimalPlaces();

        IBar q0 = ProviderCache[0];

        decimal baseline
            = Math.Round(q0.Close,
                Math.Max(decimals - 1, 0));

        lastBrick = new(
            q0.Timestamp,
            Open: baseline,
            High: 0,
            Low: 0,
            Close: baseline,
            Volume: 0,
            IsUp: false);
    }

    /// <summary>
    /// custom: build 0 to many bricks per bar
    /// </summary>
    /// <param name="item">Item to process</param>
    /// <param name="notify">Whether to notify observers</param>
    /// <param name="indexHint">Optional index hint for performance</param>
    /// <exception cref="InvalidOperationException">Thrown when the operation is invalid for the current state</exception>
    private void ToIndicator(IBar item, bool notify, int? indexHint)
    {
        int providerIndex = indexHint
            ?? throw new InvalidOperationException($"{nameof(indexHint)} cannot be empty");

        // nothing to do
        if (providerIndex <= 0)
        {
            return;
        }

        // establish baseline brick
        if (providerIndex == 1)
        {
            SetBaselineBrick();
            lastBrickProviderIndex = 0;
        }

        // determine new brick quantity
        int newBrickQty
            = Renko.GetNewBrickQuantity(
                item, lastBrick, BrickSize, EndType);

        int absBrickQty = Math.Abs(newBrickQty);
        bool isUp = newBrickQty >= 0;

        // add new brick(s) ... can add more than one!
        if (absBrickQty > 0)
        {
            // get high/low/volume between bricks
            decimal h = decimal.MinValue;
            decimal l = decimal.MaxValue;
            decimal sumV = 0;  // cumulative

            // Aggregate bars from last brick to current bar
            // Find the starting index by looking for bars after lastBrick timestamp
            int startIndex = ProviderCache.IndexOf(lastBrick.Timestamp, true) + 1;

            // Ensure startIndex is valid and within bounds
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (startIndex > providerIndex)
            {
                startIndex = providerIndex;
            }

            for (int w = startIndex; w <= providerIndex; w++)
            {
                IBar pq = ProviderCache[w];

                h = Math.Max(h, pq.High);
                l = Math.Min(l, pq.Low);
                sumV += pq.Volume;
            }

            decimal v = sumV / absBrickQty;

            for (int b = 0; b < absBrickQty; b++)
            {
                decimal o = isUp
                    ? Math.Max(lastBrick.Open, lastBrick.Close)
                    : Math.Min(lastBrick.Open, lastBrick.Close);

                decimal c = isUp
                    ? o + BrickSize
                    : o - BrickSize;

                // candidate result
                RenkoResult r
                    = new(item.Timestamp, o, h, l, c, v, isUp);

                lastBrick = r;
                lastBrickProviderIndex = providerIndex;

                // save and send
                AppendCache(r, notify);

                // note: bypass rebuild bit set in Properties to allow
                // sequential bricks with duplicate dates that would
                // normally trigger rebuild, causing stack overflow.
            }
        }
    }
}

public static partial class Renko
{
    /// <summary>
    /// Converts a bar provider to a Renko hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="brickSize">Size of each Renko brick.</param>
    /// <param name="endType">Price candle end type to use as the brick threshold.</param>
    /// <returns>A Renko hub.</returns>
    public static RenkoHub ToRenkoHub(
        this IBarProvider<IBar> barProvider,
        decimal brickSize,
        EndType endType = EndType.Close)
        => new(barProvider, brickSize, endType);
}
