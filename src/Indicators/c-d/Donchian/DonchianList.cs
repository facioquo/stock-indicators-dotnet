namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Donchian Channels from incremental bars.
/// </summary>
public class DonchianList : BufferList<DonchianResult>, IIncrementFromBar
{
    private readonly Queue<(double High, double Low)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DonchianList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public DonchianList(int lookbackPeriods = 20)
    {
        Donchian.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<(double, double)>(lookbackPeriods);

        Name = $"DONCHIAN({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DonchianList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public DonchianList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        // Calculate Donchian when we have enough prior data
        // Note: Donchian looks at PRIOR periods (not including current)
        double? upperBand = null;
        double? lowerBand = null;
        double? centerline = null;
        double? width = null;

        if (_buffer.Count == LookbackPeriods)
        {
            double highHigh = double.MinValue;
            double lowLow = double.MaxValue;

            // Find highest high and lowest low in the buffer
            foreach ((double High, double Low) in _buffer)
            {
                if (High > highHigh)
                {
                    highHigh = High;
                }

                if (Low < lowLow)
                {
                    lowLow = Low;
                }
            }

            upperBand = highHigh;
            lowerBand = lowLow;
            centerline = (upperBand + lowerBand) / 2d;
            width = centerline == 0 ? null : (upperBand - lowerBand) / centerline;
        }

        // Update buffer AFTER calculating (since we look at prior periods)
        _buffer.Update(LookbackPeriods, ((double)bar.High, (double)bar.Low));

        AddInternal(new DonchianResult(
            Timestamp: timestamp,
            UpperBand: upperBand,
            Centerline: centerline,
            LowerBand: lowerBand,
            Width: width));
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
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }
}

public static partial class Donchian
{
    /// <summary>
    /// Creates a buffer list for Donchian Channels calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static DonchianList ToDonchianList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
        => new(lookbackPeriods) { bars };
}
