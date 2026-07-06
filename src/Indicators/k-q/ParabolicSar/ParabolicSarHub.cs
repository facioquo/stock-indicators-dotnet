namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Parabolic SAR.
/// </summary>
public class ParabolicSarHub
    : ChainHub<IBar, ParabolicSarResult>, IParabolicSar
{
    private readonly Queue<(double High, double Low)> _buffer;

    // State variables
    private double _accelerationFactor;
    private double _extremePoint;
    private double _priorSar;
    private bool _isRising;
    private bool _isInitialized;
    private bool _firstReversalFound;

    // recent state snapshots for O(1) near-tail rollback
    private readonly RollbackRing<SarState> _rollback = new();

    // snapshot of mutable scalar/flag state recorded after each processed item
    private readonly record struct SarState(
        double AccelerationFactor,
        double ExtremePoint,
        double PriorSar,
        bool IsRising,
        bool IsInitialized,
        bool FirstReversalFound);

    internal ParabolicSarHub(
        IBarProvider<IBar> provider,
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)
        : this(provider, accelerationStep, maxAccelerationFactor, accelerationStep)
    {
    }
    internal ParabolicSarHub(
        IBarProvider<IBar> provider,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor) : base(provider)
    {
        ParabolicSar.Validate(accelerationStep, maxAccelerationFactor, initialFactor);

        AccelerationStep = accelerationStep;
        MaxAccelerationFactor = maxAccelerationFactor;
        InitialFactor = initialFactor;
        Name = $"PSAR({accelerationStep},{maxAccelerationFactor},{initialFactor})";

        _buffer = new Queue<(double, double)>(2);
        _isInitialized = false;
        _firstReversalFound = false;

        // Validate cache size for warmup requirements
        ValidateCacheSize(2, Name);  // SAR needs at least 2 periods to establish trend

        Reinitialize();
    }

    /// <inheritdoc/>
    public double AccelerationStep { get; init; }

    /// <inheritdoc/>
    public double MaxAccelerationFactor { get; init; }

    /// <inheritdoc/>
    public double InitialFactor { get; init; }
    /// <inheritdoc/>
    protected override (ParabolicSarResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double high = (double)item.High;
        double low = (double)item.Low;

        // Skip first bar (initialize state only)
        if (!_isInitialized)
        {
            _accelerationFactor = InitialFactor;
            _extremePoint = high;
            _priorSar = low;
            _isRising = true;  // initial guess
            _isInitialized = true;

            // Ensure prior-bar buffer contains the first bar for next-bar clamps
            _buffer.Update(2, (high, low));

            // snapshot state for O(1) near-tail rollback
            _rollback.Add(item.Timestamp, new SarState(
                _accelerationFactor, _extremePoint, _priorSar,
                _isRising, _isInitialized, _firstReversalFound));

            return (new ParabolicSarResult(item.Timestamp), i);
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
                ((double _, double l2), (double _, double l1)) = PeekLastTwo();  // i-2, i-1
                double minLastTwo = Math.Min(l1, l2);
                sar = Math.Min(sar, minLastTwo);
            }

            // Turn down
            if (low < sar)
            {
                isReversal = true;
                psar = _extremePoint;

                _isRising = false;
                _accelerationFactor = InitialFactor;
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
                        _accelerationFactor + AccelerationStep,
                        MaxAccelerationFactor);
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
                ((double h2, double _), (double h1, double _)) = PeekLastTwo();  // i-2, i-1
                double maxLastTwo = Math.Max(h1, h2);
                sar = Math.Max(sar, maxLastTwo);
            }

            // Turn up
            if (high > sar)
            {
                isReversal = true;
                psar = _extremePoint;

                _isRising = true;
                _accelerationFactor = InitialFactor;
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
                        _accelerationFactor + AccelerationStep,
                        MaxAccelerationFactor);
                }
            }
        }

        // Add result
        // Only output SAR values after the first reversal has been found
        ParabolicSarResult result;

        if (!_firstReversalFound)
        {
            // Before first reversal, output null values
            result = new ParabolicSarResult(item.Timestamp);

            // Check if this IS the first reversal
            if (isReversal == true)
            {
                _firstReversalFound = true;
            }
        }
        else
        {
            // After first reversal, output actual values
            result = new ParabolicSarResult(
                Timestamp: item.Timestamp,
                Sar: psar.NaN2Null(),
                IsReversal: isReversal);
        }

        _priorSar = psar;

        // Update buffer for last two bars AFTER using it for calculations
        _buffer.Update(2, (high, low));

        // snapshot state for O(1) near-tail rollback
        _rollback.Add(item.Timestamp, new SarState(
            _accelerationFactor, _extremePoint, _priorSar,
            _isRising, _isInitialized, _firstReversalFound));

        return (result, i);
    }

    /// <summary>
    /// Gets the two buffered prior bars (oldest first) without
    /// boxing the queue enumerator, unlike LINQ <c>ElementAt</c>.
    /// </summary>
    private ((double High, double Low) Oldest, (double High, double Low) Latest) PeekLastTwo()
    {
        Queue<(double High, double Low)>.Enumerator e = _buffer.GetEnumerator();
        e.MoveNext();
        (double High, double Low) oldest = e.Current;
        e.MoveNext();
        return (oldest, e.Current);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Clear state
        _buffer.Clear();
        _isInitialized = false;
        _firstReversalFound = false;

        if (restoreIndex < 0)
        {
            return;
        }

        // O(1) fast path: restore the exact state snapshot recorded when the
        // restore-point item was processed (near-tail rollbacks, the common
        // case for live corrections and forming-bar updates)
        if (_rollback.TryGet(ProviderCache[restoreIndex].Timestamp, out SarState snapshot))
        {
            _accelerationFactor = snapshot.AccelerationFactor;
            _extremePoint = snapshot.ExtremePoint;
            _priorSar = snapshot.PriorSar;
            _isRising = snapshot.IsRising;
            _isInitialized = snapshot.IsInitialized;
            _firstReversalFound = snapshot.FirstReversalFound;

            // Rebuild the prior-bar buffer exactly as sequential processing
            // would leave it: the last two bars up to restoreIndex (only the
            // first bar when restoreIndex is 0)
            if (restoreIndex >= 1)
            {
                IBar prior = ProviderCache[restoreIndex - 1];
                _buffer.Update(2, ((double)prior.High, (double)prior.Low));
            }

            IBar restore = ProviderCache[restoreIndex];
            _buffer.Update(2, ((double)restore.High, (double)restore.Low));

            return;
        }

        // Rebuild state by replaying history up to restoreIndex
        for (int p = 0; p <= restoreIndex; p++)
        {
            IBar bar = ProviderCache[p];
            double high = (double)bar.High;
            double low = (double)bar.Low;

            if (p == 0)
            {
                // Initialize state with first bar
                _accelerationFactor = InitialFactor;
                _extremePoint = high;
                _priorSar = low;
                _isRising = true;
                _isInitialized = true;
                _buffer.Update(2, (high, low));
                continue;
            }

            // Replay the calculation logic for this bar
            bool isReversal;

            if (_isRising)
            {
                double sar = _priorSar + (_accelerationFactor * (_extremePoint - _priorSar));

                if (_buffer.Count >= 2)
                {
                    ((double _, double l2), (double _, double l1)) = PeekLastTwo();
                    double minLastTwo = Math.Min(l1, l2);
                    sar = Math.Min(sar, minLastTwo);
                }

                if (low < sar)
                {
                    isReversal = true;
                    _priorSar = _extremePoint;
                    _isRising = false;
                    _accelerationFactor = InitialFactor;
                    _extremePoint = low;
                }
                else
                {
                    isReversal = false;
                    _priorSar = sar;

                    if (high > _extremePoint)
                    {
                        _extremePoint = high;
                        _accelerationFactor = Math.Min(
                            _accelerationFactor + AccelerationStep,
                            MaxAccelerationFactor);
                    }
                }
            }
            else
            {
                double sar = _priorSar - (_accelerationFactor * (_priorSar - _extremePoint));

                if (_buffer.Count >= 2)
                {
                    ((double h2, double _), (double h1, double _)) = PeekLastTwo();
                    double maxLastTwo = Math.Max(h1, h2);
                    sar = Math.Max(sar, maxLastTwo);
                }

                if (high > sar)
                {
                    isReversal = true;
                    _priorSar = _extremePoint;
                    _isRising = true;
                    _accelerationFactor = InitialFactor;
                    _extremePoint = high;
                }
                else
                {
                    isReversal = false;
                    _priorSar = sar;

                    if (low < _extremePoint)
                    {
                        _extremePoint = low;
                        _accelerationFactor = Math.Min(
                            _accelerationFactor + AccelerationStep,
                            MaxAccelerationFactor);
                    }
                }
            }

            _buffer.Update(2, (high, low));

            // Track first reversal
            if (isReversal && !_firstReversalFound)
            {
                _firstReversalFound = true;
            }
        }
    }
}

public static partial class ParabolicSar
{
    /// <summary>
    /// Creates a Parabolic SAR streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation. Default is 0.02.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation. Default is 0.2.</param>
    /// <returns>A Parabolic SAR hub.</returns>
    public static ParabolicSarHub ToParabolicSarHub(
        this IBarProvider<IBar> barProvider,
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)
        => new(barProvider, accelerationStep, maxAccelerationFactor);

    /// <summary>
    /// Creates a Parabolic SAR streaming hub from a bar provider with custom initial factor.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="accelerationStep">Acceleration step for the SAR calculation.</param>
    /// <param name="maxAccelerationFactor">Maximum acceleration factor for the SAR calculation.</param>
    /// <param name="initialFactor">Initial acceleration factor for the SAR calculation.</param>
    /// <returns>A Parabolic SAR hub.</returns>
    public static ParabolicSarHub ToParabolicSarHub(
        this IBarProvider<IBar> barProvider,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
        => new(barProvider, accelerationStep, maxAccelerationFactor, initialFactor);
}
