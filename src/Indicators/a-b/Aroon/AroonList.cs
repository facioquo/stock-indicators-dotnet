namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Aroon Oscillator from incremental bar values.
/// </summary>
public class AroonList : BufferList<AroonResult>, IIncrementFromBar, IAroon
{
    private readonly Queue<(DateTime Timestamp, double High, double Low)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AroonList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public AroonList(int lookbackPeriods = 25)
    {
        Aroon.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<(DateTime, double, double)>(lookbackPeriods + 1);

        Name = $"AROON({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AroonList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public AroonList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <summary>
    /// Adds a new bar to the Aroon list.
    /// </summary>
    /// <param name="bar">Bar to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bar is null.</exception>
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Use BufferListUtilities extension method for consistent buffer management
        _buffer.Update(LookbackPeriods + 1, (bar.Timestamp, (double)bar.High, (double)bar.Low));

        double? aroonUp = null;
        double? aroonDown = null;

        // Calculate when we have enough data
        if (_buffer.Count > LookbackPeriods)
        {
            double lastHighPrice = double.MinValue;
            double lastLowPrice = double.MaxValue;
            int lastHighIndex = 0;
            int lastLowIndex = 0;
            int currentIndex = 0;

            // Find the indices of highest high and lowest low in the buffer
            foreach ((DateTime Timestamp, double High, double Low) in _buffer)
            {
                if (High > lastHighPrice)
                {
                    lastHighPrice = High;
                    lastHighIndex = currentIndex;
                }

                if (Low < lastLowPrice)
                {
                    lastLowPrice = Low;
                    lastLowIndex = currentIndex;
                }

                currentIndex++;
            }

            // Calculate Aroon values
            // Index 0 is oldest, currentIndex-1 is newest
            int periodsSinceHigh = currentIndex - 1 - lastHighIndex;
            int periodsSinceLow = currentIndex - 1 - lastLowIndex;

            aroonUp = 100.0 * (LookbackPeriods - periodsSinceHigh) / LookbackPeriods;
            aroonDown = 100.0 * (LookbackPeriods - periodsSinceLow) / LookbackPeriods;
        }

        AddInternal(new AroonResult(
            Timestamp: bar.Timestamp,
            AroonUp: aroonUp,
            AroonDown: aroonDown,
            Oscillator: aroonUp - aroonDown
        ));
    }

    /// <summary>
    /// Adds a list of bars to the Aroon list.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class Aroon
{
    /// <summary>
    /// Creates a buffer list for Aroon calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An AroonList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static AroonList ToAroonList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 25)
        => new(lookbackPeriods) { bars };
}
