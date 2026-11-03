namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Correlation of two incremental series over a lookback window, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is series A and the other is series B. See
/// <see cref="PairBufferList{TResult}"/> for how the two series are added and matched.
/// </remarks>
public class CorrelationList : PairBufferList<CorrResult>, ICorrelation
{
    private readonly Queue<PairedValue> _buffer;
    private readonly double[] _dataA;
    private readonly double[] _dataB;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public CorrelationList(int lookbackPeriods)
    {
        Correlation.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<PairedValue>(lookbackPeriods);
        _dataA = new double[lookbackPeriods];
        _dataB = new double[lookbackPeriods];
        Name = $"CORR({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationList"/> class with initial values of both series.
    /// </summary>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <param name="sourceA">Initial values of series A, time sorted.</param>
    /// <param name="sourceB">Initial values of series B, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public CorrelationList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> sourceA,
        IReadOnlyList<IReusable> sourceB)
        : this(lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceA);
        ArgumentNullException.ThrowIfNull(sourceB);

        AddSource(sourceA);
        AddOther(sourceB);
    }

    /// <inheritdoc />
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }

    /// <inheritdoc />
    private protected override void AddPair(PairedValue pair)
    {
        _buffer.Update(LookbackPeriods, pair);

        if (_buffer.Count < LookbackPeriods)
        {
            AddInternal(new CorrResult(pair.Timestamp));
            return;
        }

        int z = 0;
        foreach (PairedValue p in _buffer)
        {
            _dataA[z] = p.Value;
            _dataB[z] = p.Other;
            z++;
        }

        AddInternal(Correlation.PeriodCorrelation(pair.Timestamp, _dataA, _dataB));
    }
}

public static partial class Correlation
{
    /// <summary>
    /// Creates a buffer list of correlation values between two series.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one series holds produces no result, where the Series method
    /// throws. Adding an older or equal timestamp to either side of the returned list throws
    /// <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <param name="sourceA">Values of series A, time sorted.</param>
    /// <param name="sourceB">Values of series B, time sorted.</param>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <returns>A buffer list of correlation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public static CorrelationList ToCorrelationList(
        this IReadOnlyList<IReusable> sourceA,
        IReadOnlyList<IReusable> sourceB,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceA);
        ArgumentNullException.ThrowIfNull(sourceB);

        return new(lookbackPeriods, sourceA, sourceB);
    }
}
