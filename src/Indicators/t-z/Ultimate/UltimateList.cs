namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Ultimate Oscillator from incremental bars.
/// </summary>
public class UltimateList : BufferList<UltimateResult>, IIncrementFromBar, IUltimate
{
    private readonly Queue<(double Bp, double Tr)> _buffer;
    private readonly (double Bp, double Tr)[] _bufferArray;
    private double _previousClose;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="UltimateList"/> class.
    /// </summary>
    /// <param name="shortPeriods">Number of short periods.</param>
    /// <param name="middlePeriods">Number of middle periods.</param>
    /// <param name="longPeriods">Number of long periods.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="longPeriods"/> is invalid.</exception>
    public UltimateList(
        int shortPeriods = 7,
        int middlePeriods = 14,
        int longPeriods = 28)
    {
        Ultimate.Validate(shortPeriods, middlePeriods, longPeriods);
        ShortPeriods = shortPeriods;
        MiddlePeriods = middlePeriods;
        LongPeriods = longPeriods;

        _buffer = new Queue<(double, double)>(longPeriods);
        _bufferArray = new (double Bp, double Tr)[longPeriods];
        _isInitialized = false;

        Name = $"ULTIMATE({shortPeriods}, {middlePeriods}, {longPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UltimateList"/> class with initial bars.
    /// </summary>
    /// <param name="shortPeriods">Number of short periods.</param>
    /// <param name="middlePeriods">Number of middle periods.</param>
    /// <param name="longPeriods">Number of long periods.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public UltimateList(
        int shortPeriods,
        int middlePeriods,
        int longPeriods,
        IReadOnlyList<IBar> bars)
        : this(shortPeriods, middlePeriods, longPeriods) => Add(bars);

    /// <inheritdoc />
    public int ShortPeriods { get; init; }

    /// <inheritdoc />
    public int MiddlePeriods { get; init; }

    /// <inheritdoc />
    public int LongPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;

        // Handle first period - no calculation possible
        if (!_isInitialized)
        {
            _previousClose = close;
            _isInitialized = true;

            AddInternal(new UltimateResult(timestamp, null));
            return;
        }

        // Calculate buying pressure and true range
        double bp = close - Math.Min(low, _previousClose);
        double tr = Math.Max(high, _previousClose) - Math.Min(low, _previousClose);

        // Update buffer with consolidated tuple
        _buffer.Update(LongPeriods, (bp, tr));

        double? ultimate = null;

        // Calculate Ultimate Oscillator when we have enough data
        if (_buffer.Count >= LongPeriods)
        {
            double sumBp1 = 0;  // short period
            double sumBp2 = 0;  // middle period
            double sumBp3 = 0;  // long period

            double sumTr1 = 0;  // short period
            double sumTr2 = 0;  // middle period
            double sumTr3 = 0;  // long period

            // Copy buffer into reusable array for indexed access (avoids per-Add allocation)
            (double Bp, double Tr)[] bufferArray = _bufferArray;
            _buffer.CopyTo(bufferArray, 0);
            int bufferLength = _buffer.Count;

            // Calculate sums for all three periods
            for (int i = 0; i < bufferLength; i++)
            {
                // Long period includes all values
                sumBp3 += bufferArray[i].Bp;
                sumTr3 += bufferArray[i].Tr;

                // Middle period includes more recent values
                if (i >= bufferLength - MiddlePeriods)
                {
                    sumBp2 += bufferArray[i].Bp;
                    sumTr2 += bufferArray[i].Tr;
                }

                // Short period includes most recent values
                if (i >= bufferLength - ShortPeriods)
                {
                    sumBp1 += bufferArray[i].Bp;
                    sumTr1 += bufferArray[i].Tr;
                }
            }

            // Calculate averages (avoid division by zero)
            double avg1 = sumTr1 == 0 ? double.NaN : sumBp1 / sumTr1;
            double avg2 = sumTr2 == 0 ? double.NaN : sumBp2 / sumTr2;
            double avg3 = sumTr3 == 0 ? double.NaN : sumBp3 / sumTr3;

            // Calculate Ultimate Oscillator with weighted average
            ultimate = (100d * ((4d * avg1) + (2d * avg2) + avg3) / 7d).NaN2Null();
        }

        AddInternal(new UltimateResult(timestamp, ultimate));
        _previousClose = close;
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
        _previousClose = 0;
        _isInitialized = false;
    }
}

public static partial class Ultimate
{
    /// <summary>
    /// Creates a buffer list for Ultimate Oscillator calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="shortPeriods">Number of periods for short calculation</param>
    /// <param name="middlePeriods">Number of periods for middle calculation</param>
    /// <param name="longPeriods">Number of periods for long calculation</param>
    public static UltimateList ToUltimateList(
        this IReadOnlyList<IBar> bars,
        int shortPeriods = 7,
        int middlePeriods = 14,
        int longPeriods = 28)
        => new(shortPeriods, middlePeriods, longPeriods) { bars };
}
