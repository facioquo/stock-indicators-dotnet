namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Stochastic Oscillator from incremental bar values.
/// </summary>
public class StochList : BufferList<StochResult>, IIncrementFromBar, IStoch
{
    private readonly Queue<(double High, double Low, double Close)> _hlcBuffer;
    private readonly Queue<double> _rawKBuffer;
    private readonly Queue<double> _smoothKBuffer;

    private double _prevSmoothK = double.NaN;
    private double _prevSignal = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="StochList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the oscillator calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing the oscillator.</param>
    /// <param name="kFactor">K factor for the Stochastic calculation.</param>
    /// <param name="dFactor">D factor for the Stochastic calculation.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="movingAverageType"/> is invalid.</exception>
    public StochList(
        int lookbackPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 3,
        double kFactor = 3,
        double dFactor = 2,
        MaType movingAverageType = MaType.SMA)
    {
        Stoch.Validate(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType);

        LookbackPeriods = lookbackPeriods;
        SignalPeriods = signalPeriods;
        SmoothPeriods = smoothPeriods;
        KFactor = kFactor;
        DFactor = dFactor;
        MovingAverageType = movingAverageType;

        _hlcBuffer = new Queue<(double, double, double)>(lookbackPeriods);
        _rawKBuffer = new Queue<double>(smoothPeriods);
        _smoothKBuffer = new Queue<double>(signalPeriods);

        Name = $"STOCH({14}, {3}, {3}, {3}, {2}, {MaType.SMA})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StochList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods to look back for the oscillator calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing the oscillator.</param>
    /// <param name="kFactor">K factor for the Stochastic calculation.</param>
    /// <param name="dFactor">D factor for the Stochastic calculation.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public StochList(
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType,
        IReadOnlyList<IBar> bars)
        : this(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public int SignalPeriods { get; init; }

    /// <inheritdoc />
    public int SmoothPeriods { get; init; }

    /// <inheritdoc />
    public double KFactor { get; init; }

    /// <inheritdoc />
    public double DFactor { get; init; }

    /// <inheritdoc />
    public MaType MovingAverageType { get; init; }

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
    /// Adds a new bar data point for Stochastic calculation.
    /// </summary>
    /// <param name="timestamp">Timestamp of the data point.</param>
    /// <param name="high">High price.</param>
    /// <param name="low">Low price.</param>
    /// <param name="close">Close price.</param>
    /// <exception cref="InvalidOperationException">Thrown when the operation is invalid for the current state</exception>
    public void Add(DateTime timestamp, double high, double low, double close)
    {
        // Update rolling buffer using BufferListUtilities with consolidated tuple
        _hlcBuffer.Update(LookbackPeriods, (high, low, close));

        // Calculate raw %K oscillator when we have enough data
        double rawK = double.NaN;
        if (_hlcBuffer.Count == LookbackPeriods)
        {
            double highHigh = double.MinValue;
            double lowLow = double.MaxValue;

            foreach ((double High, double Low, double Close) in _hlcBuffer)
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

            // Boundary detection to avoid floating-point precision errors at 0 and 100
            if (highHigh == lowLow)
            {
                rawK = 0d;
            }
            else if (close >= highHigh)
            {
                // Exact 100 when close equals or exceeds highHigh
                rawK = 100d;
            }
            else if (close <= lowLow)
            {
                // Exact 0 when close equals or falls below lowLow
                rawK = 0d;
            }
            else
            {
                rawK = 100d * (close - lowLow) / (highHigh - lowLow);
            }
        }

        // Calculate smoothed %K (final oscillator) - logic matches StaticSeries
        double smoothK = double.NaN;
        if (SmoothPeriods <= 1)
        {
            // No smoothing needed - keep original
            smoothK = rawK;
        }
        else if (Count >= SmoothPeriods && !double.IsNaN(rawK)) // This matches StaticSeries: i >= smoothPeriods
        {
            // Update raw K buffer for smoothing using BufferListUtilities
            _rawKBuffer.Update(SmoothPeriods, rawK);

            switch (MovingAverageType)
            {
                case MaType.SMA:
                    if (_rawKBuffer.Count == SmoothPeriods)
                    {
                        // Average buffered values (sum then divide, same as LINQ Average)
                        double sum = 0;
                        foreach (double val in _rawKBuffer)
                        {
                            sum += val;
                        }

                        smoothK = sum / _rawKBuffer.Count;
                    }

                    break;

                case MaType.SMMA:
                    // Re/initialize with SMA when _prevSmoothK is NaN
                    // (happens at start or after NaN input values)
                    // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                    if (double.IsNaN(_prevSmoothK) && _rawKBuffer.Count == SmoothPeriods)
                    {
                        // Average buffered values (sum then divide, same as LINQ Average)
                        double sum = 0;
                        foreach (double val in _rawKBuffer)
                        {
                            sum += val;
                        }

                        _prevSmoothK = sum / _rawKBuffer.Count;
                    }

                    if (!double.IsNaN(_prevSmoothK))
                    {
                        smoothK = ((_prevSmoothK * (SmoothPeriods - 1)) + rawK) / SmoothPeriods;
                        _prevSmoothK = smoothK;
                    }

                    break;

                default:
                    throw new InvalidOperationException("Invalid Stochastic moving average type.");
            }
        }

        // Calculate %D signal line - logic matches StaticSeries
        double signal = double.NaN;
        if (SignalPeriods <= 1)
        {
            signal = smoothK;
        }
        else if (Count >= SignalPeriods && !double.IsNaN(smoothK)) // This matches StaticSeries: i >= signalPeriods
        {
            // Update smooth K buffer for signal calculation using BufferListUtilities
            _smoothKBuffer.Update(SignalPeriods, smoothK);

            switch (MovingAverageType)
            {
                case MaType.SMA:
                    if (_smoothKBuffer.Count == SignalPeriods)
                    {
                        // Average buffered values (sum then divide, same as LINQ Average)
                        double sum = 0;
                        foreach (double val in _smoothKBuffer)
                        {
                            sum += val;
                        }

                        signal = sum / _smoothKBuffer.Count;
                    }

                    break;

                case MaType.SMMA:
                    // Re/initialize with SMA when _prevSignal is NaN
                    // (happens at start or after NaN input values)
                    // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                    if (double.IsNaN(_prevSignal) && _smoothKBuffer.Count == SignalPeriods)
                    {
                        // Average buffered values (sum then divide, same as LINQ Average)
                        double sum = 0;
                        foreach (double val in _smoothKBuffer)
                        {
                            sum += val;
                        }

                        _prevSignal = sum / _smoothKBuffer.Count;
                    }

                    if (!double.IsNaN(_prevSignal))
                    {
                        signal = ((_prevSignal * (SignalPeriods - 1)) + smoothK) / SignalPeriods;
                        _prevSignal = signal;
                    }

                    break;

                default:
                    throw new InvalidOperationException("Invalid Stochastic moving average type.");
            }
        }

        // Calculate %J
        double percentJ = (KFactor * smoothK) - (DFactor * signal);

        // Add result to the list
        AddInternal(new StochResult(
            Timestamp: timestamp,
            Oscillator: smoothK.NaN2Null(),
            Signal: signal.NaN2Null(),
            PercentJ: percentJ.NaN2Null()));
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _hlcBuffer.Clear();
        _rawKBuffer.Clear();
        _smoothKBuffer.Clear();
        _prevSmoothK = double.NaN;
        _prevSignal = double.NaN;
    }
}
