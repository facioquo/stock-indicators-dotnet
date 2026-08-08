namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Bar part selection from incremental bars.
/// </summary>
/// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
/// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="candlePart"/> invalid.</exception>
public class BarPartList(CandlePart candlePart) : BufferList<TimeValue>, IIncrementFromBar, IBarPart
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BarPartList"/> class with initial bars.
    /// </summary>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bars"/> list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="candlePart"/> invalid.</exception>
    public BarPartList(CandlePart candlePart, IReadOnlyList<IBar> bars)
        : this(candlePart) => Add(bars);

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is invalid.</exception>
    public CandlePart CandlePartSelection
    {
        get;
        init => field = BarParts.Validate(value);
    } = BarParts.Validate(candlePart);

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        TimeValue result = bar.ToBarPart(CandlePartSelection);
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

    /// <summary>
    /// Clears the list and resets internal state so the instance can be reused.
    /// </summary>
    public override void Clear() => base.Clear();
}

public static partial class BarParts
{
    /// <summary>
    /// Creates a buffer list for bar part selection.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="candlePart">The <see cref="CandlePart" /> element.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="bars"/> list is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="candlePart"/> invalid.</exception>
    public static BarPartList ToBarPartList(
        this IReadOnlyList<IBar> bars,
        CandlePart candlePart)
        => new(candlePart) { bars };
}
