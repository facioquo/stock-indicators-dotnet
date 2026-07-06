namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Triple Exponential Moving Average (TEMA) from incremental reusable values.
/// </summary>
public class TemaList : BufferList<TemaResult>, IIncrementFromChain, ITema
{
    private readonly Queue<double> _buffer;
    private double _bufferSum;

    /// <summary>
    /// State for triple EMA calculations
    /// </summary>
    private double _lastEma1 = double.NaN;
    private double _lastEma2 = double.NaN;
    private double _lastEma3 = double.NaN;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public TemaList(
        int lookbackPeriods
    )
    {
        Tema.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        K = 2d / (lookbackPeriods + 1);

        _buffer = new Queue<double>(lookbackPeriods);
        _bufferSum = 0;

        Name = $"TEMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TemaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public TemaList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values
    )
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double K { get; private init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use BufferListUtilities extension method for efficient buffer management
        double? dequeuedValue = _buffer.UpdateWithDequeue(LookbackPeriods, value);

        // Update running sum efficiently
        if (dequeuedValue.HasValue)
        {
            _bufferSum = _bufferSum - dequeuedValue.Value + value;
        }
        else
        {
            _bufferSum += value;
        }

        // add nulls for incalculable periods
        if (Count < LookbackPeriods - 1)
        {
            AddInternal(new TemaResult(timestamp));
            return;
        }

        double ema1;
        double ema2;
        double ema3;

        // when no prior EMA, reset as SMA
        if (double.IsNaN(_lastEma3))
        {
            ema1 = ema2 = ema3 = _bufferSum / LookbackPeriods;
        }
        // normal TEMA calculation
        else
        {
            ema1 = _lastEma1 + (K * (value - _lastEma1));
            ema2 = _lastEma2 + (K * (ema1 - _lastEma2));
            ema3 = _lastEma3 + (K * (ema2 - _lastEma3));
        }

        // calculate TEMA
        double tema = (3 * ema1) - (3 * ema2) + ema3;

        AddInternal(new TemaResult(
            timestamp,
            tema.NaN2Null()) {
            Ema1 = ema1,
            Ema2 = ema2,
            Ema3 = ema3
        });

        // store state for next calculation
        _lastEma1 = ema1;
        _lastEma2 = ema2;
        _lastEma3 = ema3;
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
        _buffer.Clear();
        _bufferSum = 0;
        _lastEma1 = double.NaN;
        _lastEma2 = double.NaN;
        _lastEma3 = double.NaN;
    }
}

public static partial class Tema
{
    /// <summary>
    /// Creates a buffer list for TEMA calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static TemaList ToTemaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
