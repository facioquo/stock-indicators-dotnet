namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Beta coefficient of an evaluated series against a market series, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is the evaluated series and the other is the market series. See
/// <see cref="PairBufferList{TResult}"/> for how the two series are added and matched.
/// </remarks>
public class BetaList : PairBufferList<BetaResult>, IBeta
{
    private readonly Queue<double> _evalReturns;
    private readonly Queue<double> _mrktReturns;
    private readonly double[] _evalWindow;
    private readonly double[] _mrktWindow;
    private double _prevEval;
    private double _prevMrkt;
    private int _pairCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation. Default is <see cref="BetaType.Standard"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public BetaList(int lookbackPeriods, BetaType type = BetaType.Standard)
    {
        Beta.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Type = type;
        _evalReturns = new Queue<double>(lookbackPeriods);
        _mrktReturns = new Queue<double>(lookbackPeriods);
        _evalWindow = new double[lookbackPeriods];
        _mrktWindow = new double[lookbackPeriods];
        Name = $"BETA({lookbackPeriods},{type.ToString().ToUpperInvariant()})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BetaList"/> class with initial values of both series.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation.</param>
    /// <param name="sourceEval">Initial evaluated values, time sorted.</param>
    /// <param name="sourceMrkt">Initial market values, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    public BetaList(
        int lookbackPeriods,
        BetaType type,
        IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceMrkt)
        : this(lookbackPeriods, type)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceMrkt);

        AddSource(sourceEval);
        AddOther(sourceMrkt);
    }

    /// <inheritdoc />
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public BetaType Type { get; }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _evalReturns.Clear();
        _mrktReturns.Clear();
        _prevEval = 0;
        _prevMrkt = 0;
        _pairCount = 0;
    }

    /// <inheritdoc />
    private protected override void AddPair(PairedValue pair)
    {
        double returnsEval = Beta.Return(pair.Value, _prevEval);
        double returnsMrkt = Beta.Return(pair.Other, _prevMrkt);
        _prevEval = pair.Value;
        _prevMrkt = pair.Other;

        _evalReturns.Update(LookbackPeriods, returnsEval);
        _mrktReturns.Update(LookbackPeriods, returnsMrkt);

        // the first return has no previous value, so the window is full one pair later
        if (_pairCount <= LookbackPeriods)
        {
            _pairCount++;
        }

        if (_pairCount <= LookbackPeriods)
        {
            AddInternal(new BetaResult(
                Timestamp: pair.Timestamp,
                ReturnsEval: returnsEval,
                ReturnsMrkt: returnsMrkt));

            return;
        }

        _evalReturns.CopyTo(_evalWindow, 0);
        _mrktReturns.CopyTo(_mrktWindow, 0);

        AddInternal(Beta.Increment(
            pair.Timestamp, _mrktWindow, _evalWindow, Type, returnsEval, returnsMrkt));
    }
}

public static partial class Beta
{
    /// <summary>
    /// Creates a buffer list of Beta coefficients of an evaluated series against a market series.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one series holds produces no result, where the Series method
    /// throws. Adding an older or equal timestamp to either side of the returned list throws
    /// <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <param name="sourceEval">Evaluated values, time sorted.</param>
    /// <param name="sourceMrkt">Market values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation. Default is <see cref="BetaType.Standard"/>.</param>
    /// <returns>A buffer list of Beta results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either series is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    public static BetaList ToBetaList(
        this IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceMrkt,
        int lookbackPeriods,
        BetaType type = BetaType.Standard)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceMrkt);

        return new(lookbackPeriods, type, sourceEval, sourceMrkt);
    }
}
