namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Arnaud Legoux Moving Average (ALMA) from incremental reusable values.
/// </summary>
public class AlmaList : BufferList<AlmaResult>, IIncrementFromChain, IAlma
{
    private readonly Queue<double> _buffer;
    private readonly double[] _weight;
    private readonly double _norm;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset for the ALMA calculation.</param>
    /// <param name="sigma">Sigma for the ALMA calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="sigma"/> is invalid.</exception>
    public AlmaList(
        int lookbackPeriods,
        double offset = 0.85,
        double sigma = 6)
    {
        Alma.Validate(lookbackPeriods, offset, sigma);

        LookbackPeriods = lookbackPeriods;
        Offset = offset;
        Sigma = sigma;

        _buffer = new Queue<double>(lookbackPeriods);

        // Pre-calculate weights and normalization factor for efficiency
        double m = offset * (lookbackPeriods - 1);
        double s = lookbackPeriods / sigma;

        _weight = new double[lookbackPeriods];
        double norm = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            double wt = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
            _weight[i] = wt;
            norm += wt;

            Name = $"ALMA({lookbackPeriods}, {0.85}, {6})";
        }

        _norm = norm;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AlmaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset for the ALMA calculation.</param>
    /// <param name="sigma">Sigma for the ALMA calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public AlmaList(
        int lookbackPeriods,
        double offset,
        double sigma,
        IReadOnlyList<IReusable> values)
        : this(lookbackPeriods, offset, sigma) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double Offset { get; init; }

    /// <inheritdoc />
    public double Sigma { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use universal buffer extension method for consistent buffer management
        _buffer.Update(LookbackPeriods, value);

        double? alma = null;

        // Calculate ALMA when we have enough values
        if (_buffer.Count == LookbackPeriods)
        {
            double weightedSum = 0;
            int n = 0;

            // Apply weights to buffer values
            foreach (double bufferValue in _buffer)
            {
                weightedSum += _weight[n] * bufferValue;
                n++;
            }

            alma = weightedSum / _norm;
        }

        AddInternal(new AlmaResult(timestamp, alma));
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

public static partial class Alma
{
    /// <summary>
    /// Creates a buffer list for Arnaud Legoux Moving Average (ALMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset value</param>
    /// <param name="sigma">Sigma value for Gaussian calculations</param>
    public static AlmaList ToAlmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double offset = 0.85,
        double sigma = 6)
        => new(lookbackPeriods, offset, sigma) { source };
}
