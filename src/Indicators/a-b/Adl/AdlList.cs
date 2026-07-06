namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Accumulation/Distribution Line (ADL) from incremental bars.
/// </summary>
public class AdlList : BufferList<AdlResult>, IIncrementFromBar
{
    private double _previousAdl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdlList"/> class.
    /// </summary>
    public AdlList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdlList"/> class with initial bars.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public AdlList(IReadOnlyList<IBar> bars)
        : this() => Add(bars);

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Calculate ADL using the Increment method
        AdlResult result = Adl.Increment(
            timestamp,
            (double)bar.High,
            (double)bar.Low,
            (double)bar.Close,
            (double)bar.Volume,
            _previousAdl);

        // Update previous ADL for next calculation
        _previousAdl = result.Adl;

        AddInternal(result);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _previousAdl = 0;
    }
}

public static partial class Adl
{
    /// <summary>
    /// Creates a buffer list for Accumulation/Distribution Line calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public static AdlList ToAdlList(
        this IReadOnlyList<IBar> bars)
        => new() { bars };
}
