namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Price Relative Strength (PRS) from two incremental series, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is the evaluated series and the other is the base series. See
/// <see cref="PairBufferList{TResult}"/> for how the two series are added and matched.
/// </remarks>
public class PrsList : PairBufferList<PrsResult>, IPrs
{
    private readonly Queue<PairedValue>? _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrsList"/> class without <see cref="PrsResult.PrsPercent"/>.
    /// </summary>
    public PrsList() => Name = "PRS";

    /// <summary>
    /// Initializes a new instance of the <see cref="PrsList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">The number of periods for the PRS% lookback calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public PrsList(int lookbackPeriods)
    {
        Prs.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<PairedValue>(lookbackPeriods + 1);
        Name = $"PRS({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrsList"/> class with initial values of both series.
    /// </summary>
    /// <param name="lookbackPeriods">The number of periods for the PRS% lookback calculation.</param>
    /// <param name="sourceEval">Initial evaluated values, time sorted.</param>
    /// <param name="sourceBase">Initial base values, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public PrsList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceBase)
        : this(lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceBase);

        AddSource(sourceEval);
        AddOther(sourceBase);
    }

    /// <inheritdoc />
    /// <remarks>0 when the list calculates no <see cref="PrsResult.PrsPercent"/>.</remarks>
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer?.Clear();
    }

    /// <inheritdoc />
    private protected override void AddPair(PairedValue pair)
    {
        double? prsPercent = null;

        if (_buffer is not null)
        {
            _buffer.Update(LookbackPeriods + 1, pair);

            if (_buffer.Count > LookbackPeriods)
            {
                PairedValue back = _buffer.Peek();
                prsPercent = Prs.Percent(pair.Value, pair.Other, back.Value, back.Other);
            }
        }

        AddInternal(new PrsResult(
            Timestamp: pair.Timestamp,
            Prs: Prs.Ratio(pair.Value, pair.Other),
            PrsPercent: prsPercent));
    }
}

public static partial class Prs
{
    /// <summary>
    /// Creates a buffer list of Price Relative Strength values without <see cref="PrsResult.PrsPercent"/>.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one series holds produces no result, where the Series method
    /// throws. Adding an older or equal timestamp to either side of the returned list throws
    /// <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <param name="sourceEval">Evaluated values, time sorted.</param>
    /// <param name="sourceBase">Base values, time sorted.</param>
    /// <returns>A buffer list of PRS results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public static PrsList ToPrsList(
        this IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceBase)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceBase);

        PrsList list = new();
        list.AddSource(sourceEval);
        list.AddOther(sourceBase);
        return list;
    }

    /// <summary>
    /// Creates a buffer list of Price Relative Strength values.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one series holds produces no result, where the Series method
    /// throws. Adding an older or equal timestamp to either side of the returned list throws
    /// <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <param name="sourceEval">Evaluated values, time sorted.</param>
    /// <param name="sourceBase">Base values, time sorted.</param>
    /// <param name="lookbackPeriods">The number of periods for the PRS% lookback calculation.</param>
    /// <returns>A buffer list of PRS results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public static PrsList ToPrsList(
        this IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceBase,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceBase);

        return new(lookbackPeriods, sourceEval, sourceBase);
    }
}
