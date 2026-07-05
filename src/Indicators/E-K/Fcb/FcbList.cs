namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fractal Chaos Bands (FCB) from incremental bar values.
/// </summary>
public class FcbList : BufferList<FcbResult>, IIncrementFromBar, IFcb
{
    private readonly Queue<IBar> _barBuffer;
    private readonly int _windowSpan;
    private decimal? _upperLine;
    private decimal? _lowerLine;

    /// <summary>
    /// Initializes a new instance of the <see cref="FcbList"/> class.
    /// </summary>
    /// <param name="windowSpan">Window span for the calculation. Default is 2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="windowSpan"/> is invalid.</exception>
    public FcbList(
        int windowSpan = 2
    )
    {
        Fcb.Validate(windowSpan);
        WindowSpan = windowSpan;

        _windowSpan = windowSpan;
        _barBuffer = new Queue<IBar>((2 * windowSpan) + 1);
        _upperLine = null;
        _lowerLine = null;

        Name = $"FCB({windowSpan})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FcbList"/> class with initial bars.
    /// </summary>
    /// <param name="windowSpan">Window span for the calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public FcbList(
        int windowSpan,
        IReadOnlyList<IBar> bars
    )
        : this(windowSpan) => Add(bars);

    /// <inheritdoc />
    public int WindowSpan { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // maintain buffer of bars needed for fractal calculation
        _barBuffer.Update((2 * _windowSpan) + 1, bar);

        // check if we can identify fractals
        if (_barBuffer.Count >= (2 * _windowSpan) + 1)
        {
            // locate middle bar without copying the buffer
            int midIndex = _windowSpan;
            IBar? midBar = null;
            int index = 0;

            foreach (IBar q in _barBuffer)
            {
                if (index == midIndex)
                {
                    midBar = q;
                    break;
                }

                index++;
            }

            decimal midHigh = midBar!.High;
            decimal midLow = midBar.Low;

            // check for bearish fractal (high point)
            bool isBearishFractal = true;
            index = 0;

            foreach (IBar q in _barBuffer)
            {
                if (index != midIndex && q.High >= midHigh)
                {
                    isBearishFractal = false;
                    break;
                }

                index++;
            }

            // check for bullish fractal (low point)
            bool isBullishFractal = true;
            index = 0;

            foreach (IBar q in _barBuffer)
            {
                if (index != midIndex && q.Low <= midLow)
                {
                    isBullishFractal = false;
                    break;
                }

                index++;
            }

            // update lines based on fractals detected windowSpan periods ago
            if (Count >= _windowSpan)
            {
                if (isBearishFractal)
                {
                    _upperLine = midHigh;
                }

                if (isBullishFractal)
                {
                    _lowerLine = midLow;
                }
            }
        }

        // add result
        AddInternal(new FcbResult(
            Timestamp: bar.Timestamp,
            UpperBand: _upperLine,
            LowerBand: _lowerLine));
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
        _barBuffer.Clear();
        _upperLine = null;
        _lowerLine = null;
    }
}

public static partial class Fcb
{
    /// <summary>
    /// Creates a buffer list for Fractal Chaos Bands (FCB) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="windowSpan">Time span for the window</param>
    public static FcbList ToFcbList(
        this IReadOnlyList<IBar> bars,
        int windowSpan = 2)
        => new(windowSpan) { bars };
}
