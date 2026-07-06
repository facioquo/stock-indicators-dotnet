namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Elder Ray from incremental bar values.
/// </summary>
public class ElderRayList : BufferList<ElderRayResult>, IIncrementFromBar, IElderRay
{
    private readonly EmaList _emaList;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElderRayList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public ElderRayList(
        int lookbackPeriods = 13
    )
    {
        ElderRay.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _emaList = new EmaList(lookbackPeriods);

        Name = $"ELDERRAY({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElderRayList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public ElderRayList(
        int lookbackPeriods,
        IReadOnlyList<IBar> bars
    )
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // update EMA
        _emaList.Add(bar);
        EmaResult emaResult = _emaList[^1];

        // calculate Elder Ray
        AddInternal(new ElderRayResult(
            Timestamp: bar.Timestamp,
            Ema: emaResult.Ema,
            BullPower: (double)bar.High - emaResult.Ema,
            BearPower: (double)bar.Low - emaResult.Ema));
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
        _emaList.Clear();
    }
}

public static partial class ElderRay
{
    /// <summary>
    /// Creates a buffer list for Elder Ray calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static ElderRayList ToElderRayList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 13)
        => new(lookbackPeriods) { bars };
}
