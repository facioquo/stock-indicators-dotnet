namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Stochastic RSI from incremental reusable values.
/// </summary>
public class StochRsiList : BufferList<StochRsiResult>, IIncrementFromChain
{
    private readonly RsiList _rsiList;
    private readonly StochList _stochList;

    /// <summary>
    /// Initializes a new instance of the <see cref="StochRsiList"/> class.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation.</param>
    /// <param name="stochPeriods">Number of periods for the Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="smoothPeriods"/> is invalid.</exception>
    public StochRsiList(
        int rsiPeriods,
        int stochPeriods,
        int signalPeriods,
        int smoothPeriods = 1)
    {
        StochRsi.Validate(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);

        RsiPeriods = rsiPeriods;
        StochPeriods = stochPeriods;
        SignalPeriods = signalPeriods;
        SmoothPeriods = smoothPeriods;

        _rsiList = new RsiList(rsiPeriods);
        _stochList = new StochList(stochPeriods, signalPeriods, smoothPeriods, 3, 2, MaType.SMA);

        Name = $"STOCHRSI({rsiPeriods}, {stochPeriods}, {signalPeriods}, {1})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StochRsiList"/> class with initial reusable values.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation.</param>
    /// <param name="stochPeriods">Number of periods for the Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public StochRsiList(
        int rsiPeriods,
        int stochPeriods,
        int signalPeriods,
        int smoothPeriods,
        IReadOnlyList<IReusable> values)
        : this(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods) => Add(values);

    /// <inheritdoc />
    public int RsiPeriods { get; init; }

    /// <inheritdoc />
    public int StochPeriods { get; init; }

    /// <inheritdoc />
    public int SignalPeriods { get; init; }

    /// <inheritdoc />
    public int SmoothPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Add to RSI list
        _rsiList.Add(timestamp, value);

        // Get the latest RSI result
        RsiResult rsiResult = _rsiList[^1];

        // If RSI has a value, feed it to the Stochastic calculation
        if (rsiResult.Rsi.HasValue)
        {
            double rsiValue = rsiResult.Rsi.Value;
            _stochList.Add(timestamp, rsiValue, rsiValue, rsiValue);
        }

        // Create StochRsi result from the Stochastic result
        StochRsiResult result;
        if (_stochList.Count > 0)
        {
            StochResult stochResult = _stochList[^1];
            result = new StochRsiResult(
                Timestamp: timestamp,
                StochRsi: stochResult.Oscillator,
                Signal: stochResult.Signal);
        }
        else
        {
            result = new StochRsiResult(timestamp);
        }

        AddInternal(result);
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _rsiList.Clear();
        _stochList.Clear();
    }
}

public static partial class StochRsi
{
    /// <summary>
    /// Creates a buffer list for Stochastic RSI calculations.
    /// </summary>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="rsiPeriods">Number of periods for RSI calculation.</param>
    /// <param name="stochPeriods">Number of periods for Stochastic calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal line.</param>
    /// <param name="smoothPeriods">Number of periods for smoothing.</param>
    /// <returns>A buffer list for Stochastic RSI calculations.</returns>
    public static StochRsiList ToStochRsiList(
        this IReadOnlyList<IReusable> source,
        int rsiPeriods,
        int stochPeriods,
        int signalPeriods,
        int smoothPeriods = 1)
        => new(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods) { source };
}
