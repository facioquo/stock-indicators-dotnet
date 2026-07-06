namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Simple Moving Average (SMA) with extended analysis from incremental reusable values.
/// </summary>
public class SmaAnalysisList : BufferList<SmaAnalysisResult>, IIncrementFromChain, ISma
{
    private readonly Queue<double> _buffer;
    private readonly int lookbackPeriods;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmaAnalysisList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SmaAnalysisList(int lookbackPeriods)
    {
        Sma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        this.lookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods);

        Name = $"SMAANALYSIS({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmaAnalysisList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SmaAnalysisList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update the rolling buffer
        _buffer.Update(lookbackPeriods, value);

        // Calculate when we have enough data
        double? sma = null;
        double? mad = null;
        double? mse = null;
        double? mape = null;

        if (_buffer.Count == lookbackPeriods)
        {
            // Calculate SMA
            double sum = 0;
            foreach (double val in _buffer)
            {
                sum += val;
            }

            sma = sum / _buffer.Count;

            // Calculate analysis metrics
            double sumMad = 0;
            double sumMse = 0;
            double sumMape = 0;

            foreach (double val in _buffer)
            {
                sumMad += Math.Abs(val - sma.Value);
                sumMse += (val - sma.Value) * (val - sma.Value);
                sumMape += val == 0 ? double.NaN : Math.Abs(val - sma.Value) / val;
            }

            mad = (sumMad / lookbackPeriods).NaN2Null();
            mse = (sumMse / lookbackPeriods).NaN2Null();
            mape = (sumMape / lookbackPeriods).NaN2Null();
        }

        AddInternal(new SmaAnalysisResult(
            Timestamp: timestamp,
            Sma: sma,
            Mad: mad,
            Mse: mse,
            Mape: mape));
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

public static partial class SmaAnalysis
{
    /// <summary>
    /// Creates a buffer list for Simple Moving Average (SMA) with extended analysis calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static SmaAnalysisList ToSmaAnalysisList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
