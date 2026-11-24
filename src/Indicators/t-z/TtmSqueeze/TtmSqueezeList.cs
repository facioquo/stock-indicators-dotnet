namespace FacioQuo.Stock.Indicators;

/// <summary>
/// TTM Squeeze from incremental bars.
/// </summary>
public class TtmSqueezeList : BufferList<TtmSqueezeResult>, IIncrementFromBar, ITtmSqueeze
{
    private readonly Queue<(double High, double Low, double Close, double TrueRange)> _buffer;
    private readonly Queue<double> _deltaBuffer;

    // windows copied out of the buffers, oldest first, for the shared kernels
    private readonly double[] _highs;
    private readonly double[] _lows;
    private readonly double[] _closes;
    private readonly double[] _trueRanges;
    private readonly double[] _deltas;

    // rows seen since construction or Clear; Count shrinks when the list prunes
    private int _rowsSeen;
    private double _prevClose;

    /// <summary>
    /// Initializes a new instance of the <see cref="TtmSqueezeList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of their valid range.</exception>
    public TtmSqueezeList(
        int lookbackPeriods = 20,
        double standardDeviations = 2,
        double multiplier = 1.5)
    {
        TtmSqueeze.Validate(lookbackPeriods, standardDeviations, multiplier);
        LookbackPeriods = lookbackPeriods;
        StandardDeviations = standardDeviations;
        Multiplier = multiplier;

        _buffer = new Queue<(double, double, double, double)>(lookbackPeriods);
        _deltaBuffer = new Queue<double>(lookbackPeriods);

        _highs = new double[lookbackPeriods];
        _lows = new double[lookbackPeriods];
        _closes = new double[lookbackPeriods];
        _trueRanges = new double[lookbackPeriods];
        _deltas = new double[lookbackPeriods];

        Name = $"TTM-SQUEEZE({lookbackPeriods},{standardDeviations},{multiplier})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TtmSqueezeList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public TtmSqueezeList(
        int lookbackPeriods,
        double standardDeviations,
        double multiplier,
        IReadOnlyList<IBar> bars)
        : this(lookbackPeriods, standardDeviations, multiplier) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double StandardDeviations { get; init; }

    /// <inheritdoc />
    public double Multiplier { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        int i = _rowsSeen++;

        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;
        double trueRange = i > 0
            ? Tr.Increment(high, low, _prevClose)
            : double.NaN;

        _prevClose = close;
        _buffer.Update(LookbackPeriods, (high, low, close, trueRange));

        if (i < LookbackPeriods - 1)
        {
            AddInternal(new TtmSqueezeResult(bar.Timestamp));
            return;
        }

        int p = 0;

        foreach ((double High, double Low, double Close, double TrueRange) in _buffer)
        {
            _highs[p] = High;
            _lows[p] = Low;
            _closes[p] = Close;
            _trueRanges[p] = TrueRange;
            p++;
        }

        (double delta, bool? isSqueezeOn) = TtmSqueeze.Increment(
            _highs,
            _lows,
            _closes,
            i >= LookbackPeriods ? _trueRanges : [],
            StandardDeviations,
            Multiplier);

        _deltaBuffer.Update(LookbackPeriods, delta);

        double? momentum = null;

        if (i >= (2 * LookbackPeriods) - 2)
        {
            _deltaBuffer.CopyTo(_deltas, 0);
            momentum = TtmSqueeze.LinearRegression(_deltas).NaN2Null();
        }

        AddInternal(new TtmSqueezeResult(
            Timestamp: bar.Timestamp,
            Momentum: momentum,
            IsSqueezeOn: isSqueezeOn) {
            Delta = delta.NaN2Null()
        });
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
        _buffer.Clear();
        _deltaBuffer.Clear();
        _rowsSeen = 0;
        _prevClose = 0;
    }
}

public static partial class TtmSqueeze
{
    /// <summary>
    /// Creates a buffer list for TTM Squeeze calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    public static TtmSqueezeList ToTtmSqueezeList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20,
        double standardDeviations = 2,
        double multiplier = 1.5)
        => new(lookbackPeriods, standardDeviations, multiplier) { bars };
}
