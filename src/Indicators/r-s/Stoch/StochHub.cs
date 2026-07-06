namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Stochastic Oscillator.
/// </summary>
public class StochHub
    : StreamHub<IBar, StochResult>, IStoch
{

    private CircularDoubleBuffer _highBuffer;
    private CircularDoubleBuffer _lowBuffer;
    private readonly Queue<double> _rawKBuffer;

    internal StochHub(
        IStreamObservable<IBar> provider,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods) : this(provider, lookbackPeriods, signalPeriods, smoothPeriods, 3, 2, MaType.SMA)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StochHub"/> class with extended parameters.
    /// </summary>
    /// <param name="provider">Bar provider.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <param name="kFactor">K factor for the Stochastic calculation.</param>
    /// <param name="dFactor">D factor for the Stochastic calculation.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    internal StochHub(
        IStreamObservable<IBar> provider,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType) : base(provider)
    {
        Stoch.Validate(lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType);

        LookbackPeriods = lookbackPeriods;
        SignalPeriods = signalPeriods;
        SmoothPeriods = smoothPeriods;
        KFactor = kFactor;
        DFactor = dFactor;
        MovingAverageType = movingAverageType;

        Name = $"STOCH({lookbackPeriods},{signalPeriods},{smoothPeriods})";

        _highBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _lowBuffer = new CircularDoubleBuffer(lookbackPeriods);

        // Initialize buffer for raw K values (needed for SMA smoothing)
        _rawKBuffer = new Queue<double>(smoothPeriods);

        // Validate cache size for warmup requirements
        // Signal SMA reads Cache[p].Oscillator for p = (i - SignalPeriods + 1) to (i - 1);
        // cache must retain at least Max(lookbackPeriods, signalPeriods, smoothPeriods) items.
        int requiredWarmup = Math.Max(Math.Max(lookbackPeriods, signalPeriods), smoothPeriods);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

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
    /// <inheritdoc/>
    protected override (StochResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double high = (double)item.High;
        double low = (double)item.Low;
        double close = (double)item.Close;

        // Normal incremental update - O(1) amortized operation
        // Using circular buffer eliminates monotonic deque overhead
        // NaN values are allowed and will propagate naturally through calculations
        _highBuffer.Add(high);
        _lowBuffer.Add(low);

        // Calculate raw %K oscillator
        double rawK = double.NaN;
        if (i >= LookbackPeriods - 1)
        {
            // Use O(1) max/min retrieval from circular buffers
            double highHigh = _highBuffer.GetMax();
            double lowLow = _lowBuffer.GetMin();

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

        // Add raw K to buffer for smoothing calculation
        // Buffering eliminates O(n²) recalculation in SMA smoothing
        _rawKBuffer.Enqueue(rawK);
        if (_rawKBuffer.Count > SmoothPeriods)
        {
            _rawKBuffer.Dequeue();
        }

        // Calculate smoothed %K (oscillator) - matches StaticSeries logic
        double oscillator = double.NaN;
        if (SmoothPeriods <= 1)
        {
            oscillator = rawK;
        }
        else if (i >= SmoothPeriods)
        {
            switch (MovingAverageType)
            {
                case MaType.SMA:
                    // Use buffered raw K values for O(n) smoothing instead of O(n²) recalculation
                    double sum = 0;
                    foreach (double rawKValue in _rawKBuffer)
                    {
                        sum += rawKValue;
                    }

                    oscillator = sum / SmoothPeriods;
                    break;

                case MaType.SMMA:
                    // Get previous smoothed K from cache
                    double prevSmoothK;
                    if (i > SmoothPeriods && Cache.Count >= i && Cache[i - 1].Oscillator.HasValue)
                    {
                        prevSmoothK = Cache[i - 1].Oscillator!.Value;
                    }
                    else
                    {
                        // Re/initialize with SMA of raw K buffer
                        // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                        if (_rawKBuffer.Count == SmoothPeriods)
                        {
                            double initSum = 0;
                            foreach (double rawKValue in _rawKBuffer)
                            {
                                initSum += rawKValue;
                            }

                            prevSmoothK = initSum / SmoothPeriods;
                        }
                        else
                        {
                            prevSmoothK = double.NaN;
                        }
                    }

                    if (!double.IsNaN(prevSmoothK))
                    {
                        oscillator = ((prevSmoothK * (SmoothPeriods - 1)) + rawK) / SmoothPeriods;
                    }

                    break;

                default:
                    throw new InvalidOperationException("Invalid Stochastic moving average type.");
            }
        }

        // Calculate %D signal line - matches StaticSeries logic
        double signal = double.NaN;
        if (SignalPeriods <= 1)
        {
            signal = oscillator;
        }
        else if (i >= SignalPeriods)
        {
            switch (MovingAverageType)
            {
                case MaType.SMA:
                    double sum = 0;
                    // Get smoothed K values from cache for the signal window
                    for (int p = i - SignalPeriods + 1; p <= i; p++)
                    {
                        double smoothKAtP = double.NaN;
                        if (p < i && Cache.Count > p && Cache[p].Oscillator.HasValue)
                        {
                            // Get from cache for previous positions
                            smoothKAtP = Cache[p].Oscillator!.Value;
                        }
                        else if (p == i)
                        {
                            // Use current oscillator for position i
                            smoothKAtP = oscillator;
                        }

                        sum += smoothKAtP;
                    }

                    signal = sum / SignalPeriods;
                    break;

                case MaType.SMMA:
                    // Get previous signal from cache
                    double prevSignal;
                    if (i > SignalPeriods && Cache.Count >= i && Cache[i - 1].Signal.HasValue)
                    {
                        prevSignal = Cache[i - 1].Signal!.Value;
                    }
                    else
                    {
                        // Re/initialize with SMA of smoothed K from cache
                        // This matches standard SMMA pattern (see Alligator, SMMA indicators)
                        double initSum = 0;
                        bool canCalculate = true;

                        for (int p = i - SignalPeriods + 1; p <= i; p++)
                        {
                            double smoothKAtP = double.NaN;
                            if (p < i && Cache.Count > p && Cache[p].Oscillator.HasValue)
                            {
                                smoothKAtP = Cache[p].Oscillator!.Value;
                            }
                            else if (p == i)
                            {
                                smoothKAtP = oscillator;
                            }

                            if (double.IsNaN(smoothKAtP))
                            {
                                canCalculate = false;
                                break;
                            }

                            initSum += smoothKAtP;
                        }

                        prevSignal = canCalculate ? initSum / SignalPeriods : double.NaN;
                    }

                    if (!double.IsNaN(prevSignal))
                    {
                        signal = ((prevSignal * (SignalPeriods - 1)) + oscillator) / SignalPeriods;
                    }

                    break;

                default:
                    throw new InvalidOperationException("Invalid Stochastic moving average type.");
            }
        }

        // Calculate %J only when both oscillator and signal are available
        double percentJ = double.NaN;
        if (!double.IsNaN(oscillator) && !double.IsNaN(signal))
        {
            percentJ = (KFactor * oscillator) - (DFactor * signal);
        }

        StochResult result = new(
            Timestamp: item.Timestamp,
            Oscillator: oscillator.NaN2Null(),
            Signal: signal.NaN2Null(),
            PercentJ: percentJ.NaN2Null());

        return (result, i);
    }

    /// <summary>
    /// Restores the rolling window state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear rolling buffers and buffer
        _highBuffer.Clear();
        _lowBuffer.Clear();
        _rawKBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // Rebuild high/low windows
        int startIdx = Math.Max(0, restoreIndex + 1 - LookbackPeriods);
        for (int p = startIdx; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            double cachedHigh = (double)bar.High;
            double cachedLow = (double)bar.Low;

            _highBuffer.Add(cachedHigh);
            _lowBuffer.Add(cachedLow);
        }

        // Prefill raw-%K buffer for SMA smoothing so the next tick uses a full window
        if (SmoothPeriods > 1 && restoreIndex >= LookbackPeriods - 1)
        {
            int kStart = Math.Max(LookbackPeriods - 1, restoreIndex + 1 - SmoothPeriods);
            for (int p = kStart; p <= restoreIndex; p++)
            {
                int rStart = Math.Max(0, p + 1 - LookbackPeriods);
                double hh = double.NegativeInfinity;
                double ll = double.PositiveInfinity;

                for (int r = rStart; r <= p; r++)
                {
                    IBar q = ProviderCache[r];
                    double h = (double)q.High;
                    double l = (double)q.Low;
                    if (h > hh)
                    {
                        hh = h;
                    }

                    if (l < ll)
                    {
                        ll = l;
                    }
                }

                double c = (double)ProviderCache[p].Close;

                // Boundary detection for consistent precision with ToIndicator
                double rawAtP;
                if (hh == ll)
                {
                    rawAtP = 0d;
                }
                else if (c >= hh)
                {
                    rawAtP = 100d;
                }
                else if (c <= ll)
                {
                    rawAtP = 0d;
                }
                else
                {
                    rawAtP = 100d * (c - ll) / (hh - ll);
                }

                _rawKBuffer.Enqueue(rawAtP);
            }
        }
    }

}

public static partial class Stoch
{
    /// <summary>
    /// Converts the bar provider to a Stochastic Oscillator hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <returns>A Stochastic Oscillator hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static StochHub ToStochHub(
        this IStreamObservable<IBar> barProvider,
        int lookbackPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 3)
             => new(barProvider, lookbackPeriods, signalPeriods, smoothPeriods);

    /// <summary>
    /// Converts the bar provider to a Stochastic Oscillator hub with extended parameters.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Lookback period for the oscillator.</param>
    /// <param name="signalPeriods">Signal period for the oscillator.</param>
    /// <param name="smoothPeriods">Smoothing period for the oscillator.</param>
    /// <param name="kFactor">K factor for the Stochastic calculation.</param>
    /// <param name="dFactor">D factor for the Stochastic calculation.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <returns>A Stochastic Oscillator hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static StochHub ToStoch(
        this IStreamObservable<IBar> barProvider,
        int lookbackPeriods,
        int signalPeriods,
        int smoothPeriods,
        double kFactor,
        double dFactor,
        MaType movingAverageType)
             => new(barProvider, lookbackPeriods, signalPeriods, smoothPeriods, kFactor, dFactor, movingAverageType);
}
