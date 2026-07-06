namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chande Momentum Oscillator (CMO) from incremental reusable values.
/// </summary>
public class CmoList : BufferList<CmoResult>, IIncrementFromChain, ICmo
{
    private readonly Queue<(bool? isUp, double value)> _tickBuffer;
    private double _prevValue = double.NaN;
    private bool _isInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="CmoList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CmoList(int lookbackPeriods)
    {
        Cmo.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _tickBuffer = new Queue<(bool? isUp, double value)>(lookbackPeriods);

        Name = $"CMO({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CmoList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CmoList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        double? cmo = null;

        // First value - just initialize
        if (!_isInitialized)
        {
            _prevValue = value;
            _isInitialized = true;
        }
        else
        {
            // Determine tick direction and size
            (bool? isUp, double tickValue) tick = (null, Math.Abs(value - _prevValue));

            tick.isUp = double.IsNaN(tick.tickValue) || value == _prevValue
                ? null
                : value > _prevValue;

            // Update buffer using universal buffer utilities
            _tickBuffer.Update(LookbackPeriods, tick);

            // Calculate CMO when we have enough data
            if (_tickBuffer.Count == LookbackPeriods)
            {
                cmo = Cmo.PeriodCalculation(_tickBuffer);
            }

            _prevValue = value;
        }

        AddInternal(new CmoResult(timestamp, cmo));
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
        _tickBuffer.Clear();
        _prevValue = double.NaN;
        _isInitialized = false;
    }
}

public static partial class Cmo
{
    /// <summary>
    /// Creates a buffer list for Chande Momentum Oscillator (CMO) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static CmoList ToCmoList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
