namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a hub for Heikin-Ashi bar transformation.
/// </summary>
public class HeikinAshiHub
    : BarProvider<IBar, HeikinAshiResult>
{
    private decimal _prevOpen = decimal.MinValue;
    private decimal _prevClose = decimal.MinValue;

    internal HeikinAshiHub(
        IBarProvider<IBar> provider) : base(provider)
    {
        Name = "HEIKINASHI";
        // Validate cache size for warmup requirements
        ValidateCacheSize(1, Name);  // Pattern detection requires at least 1 period

        Reinitialize();
    }
    /// <inheritdoc/>
    protected override (HeikinAshiResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        decimal prevOpen;
        decimal prevClose;

        // Get previous values from cache or initialize from first bar
        if (i == 0)
        {
            prevOpen = item.Open;
            prevClose = item.Close;
        }
        else if (i > 0 && Cache.Count > i - 1)
        {
            // Use previous HeikinAshi result from cache
            HeikinAshiResult prev = Cache[i - 1];
            prevOpen = prev.Open;
            prevClose = prev.Close;
        }
        else
        {
            // Fallback to stored state (on mutation rebuilds)
            prevOpen = _prevOpen;
            prevClose = _prevClose;

            // If state is uninitialized, seed from first bar or current item
            if (prevOpen == decimal.MinValue || prevClose == decimal.MinValue)
            {
                if (ProviderCache.Count > 0)
                {
                    IBar q0 = ProviderCache[0];
                    prevOpen = q0.Open;
                    prevClose = q0.Close;
                }
                else
                {
                    prevOpen = item.Open;
                    prevClose = item.Close;
                }
            }
        }

        // close
        decimal close = (item.Open + item.High + item.Low + item.Close) / 4;

        // open
        decimal open = (prevOpen + prevClose) / 2;

        // high
        decimal high = Math.Max(item.High, Math.Max(open, close));

        // low
        decimal low = Math.Min(item.Low, Math.Min(open, close));

        // Candidate result
        HeikinAshiResult r = new(
            Timestamp: item.Timestamp,
            Open: open,
            High: high,
            Low: low,
            Close: close,
            Volume: item.Volume);

        // save for next iteration (used when building sequentially)
        _prevOpen = open;
        _prevClose = close;

        return (r, i);
    }

    /// <summary>
    /// Restores the state to the last result before or at the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // restore previous open/close markers
        if (Cache.Count != 0 && restoreIndex >= 0)
        {
            DateTime preserveTimestamp = ProviderCache[restoreIndex].Timestamp;

            for (int j = Cache.Count - 1; j >= 0; j--)
            {
                HeikinAshiResult c = Cache[j];
                if (c.Timestamp <= preserveTimestamp)
                {
                    _prevOpen = c.Open;
                    _prevClose = c.Close;
                    return;
                }
            }

            // else: fall through to seed from first bar/defaults below
        }

        // reset to first bar if no cache
        if (ProviderCache.Count > 0)
        {
            IBar q0 = ProviderCache[0];
            _prevOpen = q0.Open;
            _prevClose = q0.Close;
        }
        else
        {
            _prevOpen = decimal.MinValue;
            _prevClose = decimal.MinValue;
        }
    }
}

public static partial class HeikinAshi
{
    /// <summary>
    /// Creates a Heikin-Ashi hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <returns>A Heikin-Ashi hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provider is null.</exception>
    public static HeikinAshiHub ToHeikinAshiHub(
        this IBarProvider<IBar> barProvider)
        => new(barProvider);
}
