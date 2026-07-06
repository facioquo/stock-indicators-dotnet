namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Williams Alligator from incremental reusable values.
/// </summary>
public class AlligatorList : BufferList<AlligatorResult>, IIncrementFromChain, IAlligator
{
    /// <summary>
    /// Use List for O(1) indexing instead of Queue which requires O(n) ElementAt()
    /// </summary>
    private readonly List<double> _inputBuffer;
    private readonly Queue<double> _jawBuffer;
    private readonly Queue<double> _teethBuffer;
    private readonly Queue<double> _lipsBuffer;

    private double _jawBufferSum;
    private double _teethBufferSum;
    private double _lipsBufferSum;

    private double? _lastJaw;
    private double? _lastTeeth;
    private double? _lastLips;

    /// <summary>
    /// Track how many total values we've seen
    /// </summary>
    private int _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlligatorList"/> class.
    /// </summary>
    /// <param name="jawPeriods">Lookback periods for the Jaw line.</param>
    /// <param name="jawOffset">Offset periods for the Jaw line.</param>
    /// <param name="teethPeriods">Lookback periods for the Teeth line.</param>
    /// <param name="teethOffset">Offset periods for the Teeth line.</param>
    /// <param name="lipsPeriods">Lookback periods for the Lips line.</param>
    /// <param name="lipsOffset">Offset periods for the Lips line.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lipsOffset"/> is invalid.</exception>
    public AlligatorList(
        int jawPeriods = 13,
        int jawOffset = 8,
        int teethPeriods = 8,
        int teethOffset = 5,
        int lipsPeriods = 5,
        int lipsOffset = 3)
    {
        Alligator.Validate(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset);

        JawPeriods = jawPeriods;
        JawOffset = jawOffset;
        TeethPeriods = teethPeriods;
        TeethOffset = teethOffset;
        LipsPeriods = lipsPeriods;
        LipsOffset = lipsOffset;

        // Input buffer needs to store enough history for the largest (periods + offset)
        // Using List for O(1) indexing instead of Queue
        int maxBuffer = Math.Max(
            jawPeriods + jawOffset,
            Math.Max(teethPeriods + teethOffset, lipsPeriods + lipsOffset));

        _inputBuffer = new List<double>(maxBuffer);
        _jawBuffer = new Queue<double>(jawPeriods);
        _teethBuffer = new Queue<double>(teethPeriods);
        _lipsBuffer = new Queue<double>(lipsPeriods);

        _jawBufferSum = 0;
        _teethBufferSum = 0;
        _lipsBufferSum = 0;
        _count = 0;

        Name = $"ALLIGATOR({jawPeriods}, {jawOffset}, {teethPeriods}, {teethOffset}, {lipsPeriods}, {lipsOffset})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlligatorList"/> class with initial reusable values.
    /// </summary>
    /// <param name="jawPeriods">Lookback periods for the Jaw line.</param>
    /// <param name="jawOffset">Offset periods for the Jaw line.</param>
    /// <param name="teethPeriods">Lookback periods for the Teeth line.</param>
    /// <param name="teethOffset">Offset periods for the Teeth line.</param>
    /// <param name="lipsPeriods">Lookback periods for the Lips line.</param>
    /// <param name="lipsOffset">Offset periods for the Lips line.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public AlligatorList(
        int jawPeriods,
        int jawOffset,
        int teethPeriods,
        int teethOffset,
        int lipsPeriods,
        int lipsOffset,
        IReadOnlyList<IReusable> values)
        : this(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset) => Add(values);

    /// <inheritdoc/>
    public int JawPeriods { get; init; }

    /// <inheritdoc/>
    public int JawOffset { get; init; }

    /// <inheritdoc/>
    public int TeethPeriods { get; init; }

    /// <inheritdoc/>
    public int TeethOffset { get; init; }

    /// <inheritdoc/>
    public int LipsPeriods { get; init; }

    /// <inheritdoc/>
    public int LipsOffset { get; init; }

    /// <summary>
    /// Adds a new value to the Alligator list.
    /// </summary>
    /// <param name="timestamp">Timestamp of the value.</param>
    /// <param name="value">Value to add.</param>
    public void Add(DateTime timestamp, double value)
    {
        // Add to input buffer (List for O(1) indexing)
        int maxNeeded = Math.Max(
            JawPeriods + JawOffset,
            Math.Max(TeethPeriods + TeethOffset, LipsPeriods + LipsOffset));

        _inputBuffer.Add(value);

        // Keep buffer size bounded - remove oldest value if we exceed capacity
        if (_inputBuffer.Count > maxNeeded)
        {
            _inputBuffer.RemoveAt(0);
        }

        _count++;

        double? jaw = null;
        double? teeth = null;
        double? lips = null;

        // Calculate Jaw (SMMA with offset)
        // Start adding offset values to buffer when we have enough history
        if (_count > JawOffset)
        {
            // Get the value from JawOffset positions ago (O(1) indexing with List)
            double offsetValue = _inputBuffer[_inputBuffer.Count - 1 - JawOffset];

            // Use BufferListUtilities extension method with dequeue tracking for running sum maintenance
            double? removed = _jawBuffer.UpdateWithDequeue(JawPeriods, offsetValue);

            if (removed.HasValue)
            {
                _jawBufferSum -= removed.Value;
            }

            // Calculate when we have enough values in the jaw buffer
            if (_jawBuffer.Count == JawPeriods)
            {
                _jawBufferSum += offsetValue;

                jaw = !_lastJaw.HasValue
                    // First value: SMA
                    ? _jawBufferSum / JawPeriods
                    // Subsequent values: SMMA
                    : ((_lastJaw.Value * (JawPeriods - 1)) + offsetValue) / JawPeriods;

                _lastJaw = jaw;
            }
            else
            {
                // Still accumulating for initial SMA
                _jawBufferSum += offsetValue;
            }
        }

        // Calculate Teeth (SMMA with offset)
        if (_count > TeethOffset)
        {
            double offsetValue = _inputBuffer[_inputBuffer.Count - 1 - TeethOffset];

            // Use BufferListUtilities extension method with dequeue tracking for running sum maintenance
            double? removed = _teethBuffer.UpdateWithDequeue(TeethPeriods, offsetValue);

            if (removed.HasValue)
            {
                _teethBufferSum -= removed.Value;
            }

            if (_teethBuffer.Count == TeethPeriods)
            {
                _teethBufferSum += offsetValue;

                teeth = !_lastTeeth.HasValue ? _teethBufferSum / TeethPeriods : ((_lastTeeth.Value * (TeethPeriods - 1)) + offsetValue) / TeethPeriods;

                _lastTeeth = teeth;
            }
            else
            {
                _teethBufferSum += offsetValue;
            }
        }

        // Calculate Lips (SMMA with offset)
        if (_count > LipsOffset)
        {
            double offsetValue = _inputBuffer[_inputBuffer.Count - 1 - LipsOffset];

            // Use BufferListUtilities extension method with dequeue tracking for running sum maintenance
            double? removed = _lipsBuffer.UpdateWithDequeue(LipsPeriods, offsetValue);

            if (removed.HasValue)
            {
                _lipsBufferSum -= removed.Value;
            }

            if (_lipsBuffer.Count == LipsPeriods)
            {
                _lipsBufferSum += offsetValue;

                lips = !_lastLips.HasValue ? _lipsBufferSum / LipsPeriods : ((_lastLips.Value * (LipsPeriods - 1)) + offsetValue) / LipsPeriods;

                _lastLips = lips;
            }
            else
            {
                _lipsBufferSum += offsetValue;
            }
        }

        AddInternal(new AlligatorResult(timestamp, jaw, teeth, lips));
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // Prefer HL2 when source is IBar (Alligator specification)
        Add(value.Timestamp, value.Hl2OrValue());
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable v = values[i];
            // Prefer HL2 when source is IBar (Alligator specification)
            Add(v.Timestamp, v.Hl2OrValue());
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();

        _inputBuffer.Clear();
        _jawBuffer.Clear();
        _teethBuffer.Clear();
        _lipsBuffer.Clear();

        _jawBufferSum = 0;
        _teethBufferSum = 0;
        _lipsBufferSum = 0;

        _lastJaw = null;
        _lastTeeth = null;
        _lastLips = null;

        _count = 0;
    }
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class Alligator
{
    /// <summary>
    /// Creates a buffer list for Alligator calculations.
    /// </summary>
    /// <param name="source">Time-series values to transform.</param>
    /// <param name="jawPeriods">Lookback periods for the Jaw line. Default is 13.</param>
    /// <param name="jawOffset">Offset periods for the Jaw line. Default is 8.</param>
    /// <param name="teethPeriods">Lookback periods for the Teeth line. Default is 8.</param>
    /// <param name="teethOffset">Offset periods for the Teeth line. Default is 5.</param>
    /// <param name="lipsPeriods">Lookback periods for the Lips line. Default is 5.</param>
    /// <param name="lipsOffset">Offset periods for the Lips line. Default is 3.</param>
    /// <returns>An AlligatorList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static AlligatorList ToAlligatorList(
        this IReadOnlyList<IReusable> source,
        int jawPeriods = 13,
        int jawOffset = 8,
        int teethPeriods = 8,
        int teethOffset = 5,
        int lipsPeriods = 5,
        int lipsOffset = 3)
        => new(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset) { source };
}
