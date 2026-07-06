namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a Stochastic RSI stream hub that calculates Stochastic oscillator on RSI values.
/// </summary>
public sealed class StochRsiHub
    : ChainHub<IReusable, StochRsiResult>
{
    private CircularDoubleBuffer _rsiBuffer;

    /// <summary>
    /// Rolling window for %K smoothing
    /// </summary>
    private readonly Queue<double> kBuffer;
    /// <summary>
    /// Rolling window for signal line calculation
    /// </summary>
    private readonly Queue<double> signalBuffer;

    internal StochRsiHub(
        IChainProvider<IReusable> provider,
        int rsiPeriods = 14,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        : this(
            provider.ToRsiHub(rsiPeriods),
            stochPeriods,
            signalPeriods,
            smoothPeriods)
    { }

    internal StochRsiHub(
        RsiHub rsiHub,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        : base(rsiHub)
    {
        ArgumentNullException.ThrowIfNull(rsiHub);
        StochRsi.Validate(rsiHub.LookbackPeriods, stochPeriods, signalPeriods, smoothPeriods);

        RsiPeriods = rsiHub.LookbackPeriods;
        StochPeriods = stochPeriods;
        SignalPeriods = signalPeriods;
        SmoothPeriods = smoothPeriods;

        Name = $"STOCH-RSI({RsiPeriods},{stochPeriods},{signalPeriods},{smoothPeriods})";

        _rsiBuffer = new CircularDoubleBuffer(stochPeriods);

        // Buffers for rolling windows
        kBuffer = new Queue<double>(smoothPeriods);
        signalBuffer = new Queue<double>(signalPeriods);

        // Validate cache size for warmup requirements
        int requiredWarmup = StochRsi.WarmupPeriod(RsiPeriods, stochPeriods, signalPeriods, smoothPeriods);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int RsiPeriods { get; init; }

    /// <inheritdoc/>
    public int StochPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }

    /// <inheritdoc/>
    public int SmoothPeriods { get; init; }

    /// <inheritdoc/>
    protected override (StochRsiResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? stochRsi = null;
        double? signal = null;

        double rsiValue = item.Value;

        // Only process if we have a valid RSI value
        if (!double.IsNaN(rsiValue))
        {
            (double? oscillator, double? oscillatorSignal) = UpdateOscillatorState(rsiValue);

            if (oscillator.HasValue)
            {
                stochRsi = oscillator;
                signal = oscillatorSignal;
            }
        }

        // candidate result
        StochRsiResult result = new(
            Timestamp: item.Timestamp,
            StochRsi: stochRsi,
            Signal: signal);

        return (result, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset state and replay historical RSI values up to the rebuild index
        _rsiBuffer.Clear();
        kBuffer.Clear();
        signalBuffer.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        for (int i = 0; i <= restoreIndex; i++)
        {
            double rsiValue = ProviderCache[i].Value;
            if (!double.IsNaN(rsiValue))
            {
                _ = UpdateOscillatorState(rsiValue);
            }
        }
    }

    private (double? stochRsi, double? signal) UpdateOscillatorState(double rsiValue)
    {
        _rsiBuffer.Add(rsiValue);

        if (!_rsiBuffer.IsFull)
        {
            return (null, null);
        }

        double highRsi = _rsiBuffer.GetMax();
        double lowRsi = _rsiBuffer.GetMin();

        // Boundary detection to avoid floating-point precision errors at 0 and 100
        double k;
        if (lowRsi == highRsi)
        {
            k = 0d;
        }
        else if (rsiValue >= highRsi)
        {
            // Exact 100 when RSI equals or exceeds highRsi
            k = 100d;
        }
        else if (rsiValue <= lowRsi)
        {
            // Exact 0 when RSI equals or falls below lowRsi
            k = 0d;
        }
        else
        {
            k = 100d * (rsiValue - lowRsi) / (highRsi - lowRsi);
        }

        if (SmoothPeriods > 1)
        {
            kBuffer.Enqueue(k);
            if (kBuffer.Count > SmoothPeriods)
            {
                _ = kBuffer.Dequeue();
            }

            if (kBuffer.Count != SmoothPeriods)
            {
                return (null, null);
            }

            double sumK = 0d;
            foreach (double item in kBuffer)
            {
                sumK += item;
            }

            k = sumK / SmoothPeriods;
        }

        signalBuffer.Enqueue(k);
        if (signalBuffer.Count > SignalPeriods)
        {
            _ = signalBuffer.Dequeue();
        }

        double? signal = null;
        if (signalBuffer.Count == SignalPeriods)
        {
            double sumSignal = 0d;
            foreach (double item in signalBuffer)
            {
                sumSignal += item;
            }

            signal = sumSignal / SignalPeriods;
        }

        return (k, signal);
    }
}

public static partial class StochRsi
{
    /// <summary>
    /// Converts the chain provider to a Stochastic RSI hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation.</param>
    /// <param name="stochPeriods">Number of periods for the Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <returns>A Stochastic RSI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static StochRsiHub ToStochRsiHub(
        this IChainProvider<IReusable> chainProvider,
        int rsiPeriods = 14,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        => new(chainProvider, rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

    /// <summary>
    /// Creates a new Stochastic RSI hub, using RSI values from an existing RSI hub.
    /// </summary>
    /// <remarks>
    /// This extension overrides and enables a chain that specifically
    /// reuses the existing <see cref="RsiHub"/> as its internal construction.
    /// <para>IMPORTANT: This is not a normal chaining approach.</para>
    /// Do not use this interface if you want to instead want a StochRSI of an RSI hub.</remarks>
    /// <param name="rsiHub">Existing RSI hub provider.</param>
    /// <param name="stochPeriods">Number of periods for the Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <returns>A Stochastic RSI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static StochRsiHub ToStochRsiHub(
        this RsiHub rsiHub,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
        => new(rsiHub, stochPeriods, signalPeriods, smoothPeriods);
}
