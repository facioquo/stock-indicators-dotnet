namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams %R from incremental bar values.
/// </summary>
public class WilliamsRList : BufferList<WilliamsResult>, IIncrementFromBar, IWilliamsR
{
    private readonly Queue<(double High, double Low)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="WilliamsRList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public WilliamsRList(
        int lookbackPeriods = 14)
    {
        WilliamsR.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<(double, double)>(lookbackPeriods);

        Name = $"WILLIAMSR({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WilliamsRList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public WilliamsRList(
        int lookbackPeriods,
        IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);
        Add(bar.Timestamp, (double)bar.High, (double)bar.Low, (double)bar.Close);
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
    /// Adds a new bar data point for Williams %R calculation.
    /// </summary>
    /// <param name="timestamp">Timestamp of the data point.</param>
    /// <param name="high">High price.</param>
    /// <param name="low">Low price.</param>
    /// <param name="close">Close price.</param>
    public void Add(DateTime timestamp, double high, double low, double close)
    {
        // Update rolling buffer using BufferListUtilities with consolidated tuple
        _buffer.Update(LookbackPeriods, (high, low));

        // Calculate Williams %R when we have enough data
        double? williamsR = null;
        if (_buffer.Count == LookbackPeriods)
        {
            double highHigh = double.MinValue;
            double lowLow = double.MaxValue;

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

            // Apply boundary clamping to ensure -100 ≤ WilliamsR ≤ 0
            // This prevents floating-point precision errors at boundaries
            if (highHigh == lowLow)
            {
                williamsR = -100.0;
            }
            else if (close >= highHigh)
            {
                williamsR = 0.0;
            }
            else if (close <= lowLow)
            {
                williamsR = -100.0;
            }
            else
            {
                williamsR = (100.0 * (close - lowLow) / (highHigh - lowLow)) - 100.0;
            }
        }

        // Add result to the list
        AddInternal(new WilliamsResult(
            Timestamp: timestamp,
            WilliamsR: williamsR));
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }
}
