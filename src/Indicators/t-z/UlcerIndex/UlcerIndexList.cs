namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Ulcer Index from incremental reusable values.
/// </summary>
public class UlcerIndexList : BufferList<UlcerIndexResult>, IIncrementFromChain
{
    private readonly Queue<double> _buffer;
    private readonly double[] _bufferArray;

    /// <summary>
    /// Initializes a new instance of the <see cref="UlcerIndexList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public UlcerIndexList(int lookbackPeriods = 14)
    {
        UlcerIndex.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<double>(lookbackPeriods);
        _bufferArray = new double[lookbackPeriods];

        Name = $"ULCERINDEX({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UlcerIndexList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public UlcerIndexList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update buffer
        _buffer.Update(LookbackPeriods, value);

        double? ui;

        if (_buffer.Count == LookbackPeriods)
        {
            // Calculate Ulcer Index
            double sumSquared = 0;

            // copy buffer into reusable array (avoids per-Add allocation)
            double[] bufferArray = _bufferArray;
            _buffer.CopyTo(bufferArray, 0);

            for (int p = 0; p < LookbackPeriods; p++)
            {
                // Find maximum close up to and including current position p
                double maxClose = 0;
                for (int z = 0; z <= p; z++)
                {
                    if (bufferArray[z] > maxClose)
                    {
                        maxClose = bufferArray[z];
                    }
                }

                // Calculate percent drawdown
                double percentDrawdown = maxClose == 0 ? double.NaN
                    : 100 * ((bufferArray[p] - maxClose) / maxClose);

                sumSquared += percentDrawdown * percentDrawdown;
            }

            ui = Math.Sqrt(sumSquared / LookbackPeriods).NaN2Null();
        }
        else
        {
            ui = null;
        }

        UlcerIndexResult result = new(
            Timestamp: timestamp,
            UlcerIndex: ui);

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
        _buffer.Clear();
    }
}

public static partial class UlcerIndex
{
    /// <summary>
    /// Creates a buffer list for Ulcer Index calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static UlcerIndexList ToUlcerIndexList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { source };
}
