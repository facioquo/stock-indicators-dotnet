namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hull Moving Average (HMA) from incremental reusable values.
/// </summary>
public class HmaList : BufferList<HmaResult>, IIncrementFromChain, IHma
{
    private readonly int wmaN1Periods;
    private readonly int wmaN2Periods;
    private readonly int sqrtPeriods;
    private readonly Queue<double> bufferN1;
    private readonly Queue<double> bufferN2;
    private readonly Queue<double> synthBuffer;
    private readonly double divisorN1;
    private readonly double divisorN2;
    private readonly double divisorSqrt;

    /// <summary>
    /// Initializes a new instance of the <see cref="HmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public HmaList(int lookbackPeriods)
    {
        Hma.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        // initialize periods for nested WMA calculations
        wmaN1Periods = lookbackPeriods;
        wmaN2Periods = lookbackPeriods / 2;
        sqrtPeriods = (int)Math.Sqrt(lookbackPeriods);

        // calculate divisors for WMA
        divisorN1 = (double)wmaN1Periods * (wmaN1Periods + 1) / 2d;
        divisorN2 = (double)wmaN2Periods * (wmaN2Periods + 1) / 2d;
        divisorSqrt = (double)sqrtPeriods * (sqrtPeriods + 1) / 2d;

        // initialize buffers for nested calculations
        bufferN1 = new Queue<double>(wmaN1Periods);
        bufferN2 = new Queue<double>(wmaN2Periods);
        synthBuffer = new Queue<double>(sqrtPeriods);

        Name = $"HMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HmaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public HmaList(int lookbackPeriods, IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // update buffers for WMA calculations using extension methods
        bufferN1.Update(wmaN1Periods, value);
        bufferN2.Update(wmaN2Periods, value);

        double? hma = null;
        int shiftQty = LookbackPeriods - 1;

        // HMA calculation can only start after we have enough periods
        // to calculate both WMA values and then the synthetic buffer
        if (Count >= shiftQty)
        {
            // calculate WMA(n/2) and WMA(n) for current period
            double? wmaN2 = Wma.ComputeWeightedMovingAverage(bufferN2, wmaN2Periods, divisorN2);
            double? wmaN1 = Wma.ComputeWeightedMovingAverage(bufferN1, wmaN1Periods, divisorN1);

            if (wmaN2.HasValue && wmaN1.HasValue)
            {
                // synthetic value: 2 * WMA(n/2) - WMA(n)
                double synthValue = (wmaN2.Value * 2d) - wmaN1.Value;

                // update synthetic buffer using extension method
                synthBuffer.Update(sqrtPeriods, synthValue);

                // calculate final HMA = WMA(sqrt(n)) of synthetic values
                // Need enough synthetic values for the final WMA calculation
                if (synthBuffer.Count == sqrtPeriods)
                {
                    hma = Wma.ComputeWeightedMovingAverage(synthBuffer, sqrtPeriods, divisorSqrt);
                }
            }
        }

        AddInternal(new HmaResult(timestamp, hma));
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
        bufferN1.Clear();
        bufferN2.Clear();
        synthBuffer.Clear();
    }
}

public static partial class Hma
{
    /// <summary>
    /// Creates a buffer list for Hull Moving Average (HMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static HmaList ToHmaList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods)
        => new(lookbackPeriods) { source };
}
