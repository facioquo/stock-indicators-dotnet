namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a Stochastic Momentum Index (SMI) stream hub that calculates SMI with signal line.
/// </summary>
public sealed class SmiHub
    : ChainHub<IBar, SmiResult>, ISmi
{

    // Circular buffers for high/low tracking — zero heap allocation per tick
    private CircularDoubleBuffer _highBuffer;
    private CircularDoubleBuffer _lowBuffer;

    // State for EMA smoothing
    private double _lastSmEma1 = double.NaN;
    private double _lastSmEma2 = double.NaN;
    private double _lastHlEma1 = double.NaN;
    private double _lastHlEma2 = double.NaN;
    private double _lastSignal = double.NaN;

    internal SmiHub(
        IStreamObservable<IBar> provider,
        int lookbackPeriods = 13,
        int firstSmoothPeriods = 25,
        int secondSmoothPeriods = 2,
        int signalPeriods = 3) : base(provider)
    {
        Smi.Validate(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);

        LookbackPeriods = lookbackPeriods;
        FirstSmoothPeriods = firstSmoothPeriods;
        SecondSmoothPeriods = secondSmoothPeriods;
        SignalPeriods = signalPeriods;

        K1 = 2d / (firstSmoothPeriods + 1);
        K2 = 2d / (secondSmoothPeriods + 1);
        KS = 2d / (signalPeriods + 1);

        Name = $"SMI({lookbackPeriods},{firstSmoothPeriods},{secondSmoothPeriods},{signalPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        // Allocate circular buffers — fixed size, no per-tick heap allocations
        _highBuffer = new CircularDoubleBuffer(lookbackPeriods);
        _lowBuffer = new CircularDoubleBuffer(lookbackPeriods);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public int FirstSmoothPeriods { get; init; }

    /// <inheritdoc/>
    public int SecondSmoothPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }

    /// <inheritdoc/>
    public double K1 { get; private init; }

    /// <inheritdoc/>
    public double K2 { get; private init; }

    /// <inheritdoc/>
    public double KS { get; private init; }
    /// <inheritdoc/>
    protected override (SmiResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        _highBuffer.Add((double)item.High);
        _lowBuffer.Add((double)item.Low);

        double smi = double.NaN;
        double signal = double.NaN;

        if (_highBuffer.IsFull)
        {
            (smi, signal) = CalculateSmi((double)item.Close);
        }

        SmiResult result = new(
            Timestamp: item.Timestamp,
            Smi: smi.NaN2Null(),
            Signal: signal.NaN2Null());

        return (result, i);
    }

    private (double smi, double signal) CalculateSmi(double close)
    {
        double hH = _highBuffer.GetMax();
        double lL = _lowBuffer.GetMin();

        // Calculate distance from midpoint and range
        double sm = close - (0.5d * (hH + lL));
        double hl = hH - lL;

        // Initialize last EMA values when no prior state exists
        if (double.IsNaN(_lastSmEma1))
        {
            _lastSmEma1 = sm;
            _lastSmEma2 = _lastSmEma1;
            _lastHlEma1 = hl;
            _lastHlEma2 = _lastHlEma1;
        }

        // First smoothing
        double smEma1 = _lastSmEma1 + (K1 * (sm - _lastSmEma1));
        double hlEma1 = _lastHlEma1 + (K1 * (hl - _lastHlEma1));

        // Second smoothing
        double smEma2 = _lastSmEma2 + (K2 * (smEma1 - _lastSmEma2));
        double hlEma2 = _lastHlEma2 + (K2 * (hlEma1 - _lastHlEma2));

        // Stochastic momentum index
        double smi = hlEma2 != 0 ? 100 * (smEma2 / (0.5 * hlEma2)) : double.NaN;

        // Initialize signal line when no prior state exists
        if (double.IsNaN(_lastSignal))
        {
            _lastSignal = smi;
        }

        // Signal line
        double signal = _lastSignal + (KS * (smi - _lastSignal));

        // Carryover values for next iteration
        _lastSmEma1 = smEma1;
        _lastSmEma2 = smEma2;
        _lastHlEma1 = hlEma1;
        _lastHlEma2 = hlEma2;
        _lastSignal = signal;

        return (smi, signal);
    }

    /// <summary>
    /// Restores the SMI calculation state up to the specified timestamp.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset state variables
        _lastSmEma1 = double.NaN;
        _lastSmEma2 = double.NaN;
        _lastHlEma1 = double.NaN;
        _lastHlEma2 = double.NaN;
        _lastSignal = double.NaN;

        // Reset circular buffers
        _highBuffer.Clear();
        _lowBuffer.Clear();

        if (restoreIndex < LookbackPeriods - 1)
        {
            return;
        }

        // Rebuild state from cache up to the rollback point
        for (int p = 0; p <= restoreIndex; p++)
        {
            IBar q = ProviderCache[p];
            _highBuffer.Add((double)q.High);
            _lowBuffer.Add((double)q.Low);

            // Calculate EMA state only after warmup
            if (p >= LookbackPeriods - 1)
            {
                CalculateSmi((double)q.Close);
            }
        }
    }

}

public static partial class Smi
{
    /// <summary>
    /// Creates a Stochastic Momentum Index (SMI) streaming hub from a bars provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of periods for the lookback window.</param>
    /// <param name="firstSmoothPeriods">Number of periods for the first smoothing.</param>
    /// <param name="secondSmoothPeriods">Number of periods for the second smoothing.</param>
    /// <param name="signalPeriods">Number of periods for the signal line smoothing.</param>
    /// <returns>A Stochastic Momentum Index hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static SmiHub ToSmiHub(
        this IStreamObservable<IBar> barProvider,
        int lookbackPeriods = 13,
        int firstSmoothPeriods = 25,
        int secondSmoothPeriods = 2,
        int signalPeriods = 3)
        => new(barProvider, lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);
}
