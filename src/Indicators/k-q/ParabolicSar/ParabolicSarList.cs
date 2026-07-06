namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Parabolic SAR (Stop and Reverse) from incremental bars.
/// </summary>
public class ParabolicSarList : BufferList<ParabolicSarResult>, IIncrementFromBar, IParabolicSar
{
    private readonly Queue<(double High, double Low)> _buffer;
    private readonly double _accelerationStep;
    private readonly double _maxAccelerationFactor;
    private readonly double _initialFactor;

    private double _accelerationFactor;
    private double _extremePoint;
    private double _priorSar;
    private bool _isRising;
    private bool _isInitialized;
    private bool _firstReversalFound;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParabolicSarList"/> class.
    /// </summary>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation. Default is 0.02.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation. Default is 0.2.</param>
    public ParabolicSarList(
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)
        : this(accelerationStep, maxAccelerationFactor, accelerationStep)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParabolicSarList"/> class.
    /// </summary>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialFactor"/> is invalid.</exception>
    public ParabolicSarList(
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
    {
        ParabolicSar.Validate(accelerationStep, maxAccelerationFactor, initialFactor);
        _accelerationStep = accelerationStep;
        _maxAccelerationFactor = maxAccelerationFactor;
        _initialFactor = initialFactor;
        AccelerationStep = accelerationStep;
        MaxAccelerationFactor = maxAccelerationFactor;
        InitialFactor = initialFactor;

        _buffer = new Queue<(double, double)>(2);
        _isInitialized = false;
        _firstReversalFound = false;

        Name = $"PARABOLICSAR({accelerationStep}, {maxAccelerationFactor})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParabolicSarList"/> class with initial bars.
    /// </summary>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public ParabolicSarList(
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor,
        IReadOnlyList<IBar> bars)
        : this(accelerationStep, maxAccelerationFactor, initialFactor) => Add(bars);

    /// <inheritdoc />
    public double AccelerationStep { get; init; }

    /// <inheritdoc />
    public double MaxAccelerationFactor { get; init; }

    /// <inheritdoc />
    public double InitialFactor { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        double high = (double)bar.High;
        double low = (double)bar.Low;

        // Skip first bar (initialize state only)
        if (!_isInitialized)
        {
            _accelerationFactor = _initialFactor;
            _extremePoint = high;
            _priorSar = low;
            _isRising = true;  // initial guess
            _isInitialized = true;

            AddInternal(new ParabolicSarResult(timestamp));
            // Ensure prior-bar buffer contains the first bar for next-bar clamps
            _buffer.Update(2, (high, low));
            return;
        }

        bool? isReversal;
        double psar;

        // Was rising
        if (_isRising)
        {
            double sar = _priorSar + (_accelerationFactor * (_extremePoint - _priorSar));

            // SAR cannot be higher than last two lows
            // _buffer contains the PREVIOUS bars (not including current)
            if (_buffer.Count >= 2)
            {
                (double _, double l1) = _buffer.ElementAt(1);  // i-1
                (double _, double l2) = _buffer.ElementAt(0);  // i-2
                double minLastTwo = Math.Min(l1, l2);
                sar = Math.Min(sar, minLastTwo);
            }

            // Turn down
            if (low < sar)
            {
                isReversal = true;
                psar = _extremePoint;

                _isRising = false;
                _accelerationFactor = _initialFactor;
                _extremePoint = low;
            }
            // Continue rising
            else
            {
                isReversal = false;
                psar = sar;

                // New high extreme point
                if (high > _extremePoint)
                {
                    _extremePoint = high;
                    _accelerationFactor = Math.Min(
                        _accelerationFactor + _accelerationStep,
                        _maxAccelerationFactor);
                }
            }
        }
        // Was falling
        else
        {
            double sar = _priorSar - (_accelerationFactor * (_priorSar - _extremePoint));

            // SAR cannot be lower than last two highs
            // _buffer contains the PREVIOUS bars (not including current)
            if (_buffer.Count >= 2)
            {
                (double h1, double _) = _buffer.ElementAt(1);  // i-1
                (double h2, double _) = _buffer.ElementAt(0);  // i-2
                double maxLastTwo = Math.Max(h1, h2);
                sar = Math.Max(sar, maxLastTwo);
            }

            // Turn up
            if (high > sar)
            {
                isReversal = true;
                psar = _extremePoint;

                _isRising = true;
                _accelerationFactor = _initialFactor;
                _extremePoint = high;
            }
            // Continue falling
            else
            {
                isReversal = false;
                psar = sar;

                // New low extreme point
                if (low < _extremePoint)
                {
                    _extremePoint = low;
                    _accelerationFactor = Math.Min(
                        _accelerationFactor + _accelerationStep,
                        _maxAccelerationFactor);
                }
            }
        }

        // Add result - initially add all results with values
        ParabolicSarResult result = new(
            Timestamp: timestamp,
            Sar: psar.NaN2Null(),
            IsReversal: isReversal);

        AddInternal(result);
        _priorSar = psar;

        // Update buffer for last two bars AFTER using it for calculations
        _buffer.Update(2, (high, low));

        // If this is the first reversal, nullify all previous results (including this one)
        if (isReversal == true && !_firstReversalFound)
        {
            _firstReversalFound = true;
            NullifyResultsBeforeFirstReversal();
        }
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
        _accelerationFactor = _initialFactor;
        _extremePoint = 0;
        _priorSar = 0;
        _isRising = true;
        _isInitialized = false;
        _firstReversalFound = false;
    }
    private void NullifyResultsBeforeFirstReversal()
    {
        // Only nullify if we found a reversal
        if (!_firstReversalFound)
        {
            return;
        }

        // Find the index of the first reversal
        int firstReversalIndex = -1;
        for (int i = 0; i < Count; i++)
        {
            if (this[i].IsReversal == true)
            {
                firstReversalIndex = i;
                break;
            }
        }

        // If no reversal found, nullify everything
        if (firstReversalIndex < 0)
        {
            firstReversalIndex = Count - 1;
        }

        // Update all results from start to first reversal (inclusive)
        for (int i = 0; i <= firstReversalIndex; i++)
        {
            ParabolicSarResult existing = this[i];
            ParabolicSarResult updated = existing with {
                Sar = null,
                IsReversal = null
            };
            UpdateInternal(i, updated);
        }
    }
}

/// <summary>
/// Parabolic SAR interface.
/// </summary>
public interface IParabolicSar
{
    /// <inheritdoc />
    double AccelerationStep { get; }

    /// <inheritdoc />
    double MaxAccelerationFactor { get; }

    /// <inheritdoc />
    double InitialFactor { get; }
}

public static partial class ParabolicSar
{
    /// <summary>
    /// Creates a buffer list for Parabolic SAR calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="accelerationStep">Acceleration step increment</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor</param>
    public static ParabolicSarList ToParabolicSarList(
        this IReadOnlyList<IBar> bars,
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)
        => new(accelerationStep, maxAccelerationFactor) { bars };

    /// <summary>
    /// Creates a buffer list for Parabolic SAR calculations with custom initial factor.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="accelerationStep">Acceleration step increment</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor</param>
    /// <param name="initialFactor">Initial acceleration factor</param>
    public static ParabolicSarList ToParabolicSarList(
        this IReadOnlyList<IBar> bars,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
        => new(accelerationStep, maxAccelerationFactor, initialFactor) { bars };
}
