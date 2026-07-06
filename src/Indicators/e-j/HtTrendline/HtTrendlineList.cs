namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Hilbert Transform Instantaneous Trendline (HTL) from incremental reusable values.
/// </summary>
/// <remarks>
/// <para>
/// <b>Exception to <see cref="BufferListUtilities"/> Pattern:</b> HtTrendline does not use the standard
/// <see cref="BufferListUtilities"/> extension methods (Update/UpdateWithDequeue) due to
/// the unique complexity of the Hilbert Transform algorithm.
/// </para>
/// <para>
/// <b>Algorithmic Requirements:</b>
/// <list type="bullet">
/// <item>Maintains 12 parallel state arrays for different phases of the Hilbert Transform calculation</item>
/// <item>Requires indexed lookback access up to 6 periods for phase calculations</item>
/// <item>Performs multi-stage calculations with cross-referencing between arrays</item>
/// </list>
/// </para>
/// <para>
/// <b>Why <see cref="Queue{T}"/> cannot be used:</b> The standard <see cref="Queue{T}"/> data structure
/// used by <see cref="BufferListUtilities"/> does not provide indexed access to historical values, which is
/// essential for the Hilbert Transform algorithm's phase and period calculations.
/// </para>
/// <para>
/// <b>Memory Management:</b> This implementation uses <see cref="List{T}"/> arrays to maintain
/// calculation state with automatic pruning to prevent unbounded growth:
/// <list type="bullet">
/// <item>State arrays are pruned at 1000 items, keeping minimum 12 periods for calculations</item>
/// <item>Result list is pruned at <see cref="BufferList{HtlResult}.MaxListSize"/> (default 90% of int.MaxValue)</item>
/// </list>
/// </para>
/// </remarks>
public class HtTrendlineList : BufferList<HtlResult>, IIncrementFromChain
{
    private readonly List<double> pr;  // price
    private readonly List<double> sp;  // smooth price
    private readonly List<double> dt;  // detrender
    private readonly List<double> pd;  // period

    private readonly List<double> q1;  // quadrature
    private readonly List<double> i1;  // in-phase

    private readonly List<double> q2;  // adj. quadrature
    private readonly List<double> i2;  // adj. in-phase

    private readonly List<double> re;  // real part
    private readonly List<double> im;  // imaginary part

    private readonly List<double> sd;  // smooth period
    private readonly List<double> it;  // instantaneous trend (raw)

    /// <summary>
    /// Minimum required for 11-period lookback (12th bar calculation)
    /// </summary>
    private const int MinBufferSize = 12;

    /// <summary>
    /// Trigger point to prune buffers to MinBufferSize
    /// </summary>
    private const int MaxBufferSize = 1000;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtTrendlineList"/> class.
    /// </summary>
    public HtTrendlineList()
    {
        pr = [];
        sp = [];
        dt = [];
        pd = [];
        q1 = [];
        i1 = [];
        q2 = [];
        i2 = [];
        re = [];
        im = [];
        sd = [];
        it = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HtTrendlineList"/> class with initial reusable values.
    /// </summary>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public HtTrendlineList(IReadOnlyList<IReusable> values)
        : this() => Add(values);

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Use pr.Count instead of Count to align with state arrays after pruning
        pr.Add(value);
        int i = pr.Count - 1;

        if (i > 5)
        {
            double adj = (0.075 * pd[i - 1]) + 0.54;

            // smooth and detrender
            double spValue = ((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10;
            sp.Add(spValue);

            double dtValue = ((0.0962 * sp[i]) + (0.5769 * sp[i - 2]) - (0.5769 * sp[i - 4]) - (0.0962 * sp[i - 6])) * adj;
            dt.Add(dtValue);

            // in-phase and quadrature
            double q1Value = ((0.0962 * dt[i]) + (0.5769 * dt[i - 2]) - (0.5769 * dt[i - 4]) - (0.0962 * dt[i - 6])) * adj;
            q1.Add(q1Value);

            double i1Value = dt[i - 3];
            i1.Add(i1Value);

            // advance the phases by 90 degrees
            double jI = ((0.0962 * i1[i]) + (0.5769 * i1[i - 2]) - (0.5769 * i1[i - 4]) - (0.0962 * i1[i - 6])) * adj;
            double jQ = ((0.0962 * q1[i]) + (0.5769 * q1[i - 2]) - (0.5769 * q1[i - 4]) - (0.0962 * q1[i - 6])) * adj;

            // phasor addition for 3-bar averaging
            double i2Value = i1[i] - jQ;
            double q2Value = q1[i] + jI;

            i2Value = (0.2 * i2Value) + (0.8 * i2[i - 1]);  // smoothing it
            q2Value = (0.2 * q2Value) + (0.8 * q2[i - 1]);

            i2.Add(i2Value);
            q2.Add(q2Value);

            // homodyne discriminator
            double reValue = (i2[i] * i2[i - 1]) + (q2[i] * q2[i - 1]);
            double imValue = (i2[i] * q2[i - 1]) - (q2[i] * i2[i - 1]);

            reValue = (0.2 * reValue) + (0.8 * re[i - 1]);  // smoothing it
            imValue = (0.2 * imValue) + (0.8 * im[i - 1]);

            re.Add(reValue);
            im.Add(imValue);

            // calculate period
            double pdValue = im[i] != 0 && re[i] != 0
                ? 2 * Math.PI / DeMath.Atan(im[i] / re[i])
                : 0d;

            // adjust period to thresholds
            pdValue = pdValue > 1.5 * pd[i - 1] ? 1.5 * pd[i - 1] : pdValue;
            pdValue = pdValue < 0.67 * pd[i - 1] ? 0.67 * pd[i - 1] : pdValue;
            pdValue = pdValue < 6d ? 6d : pdValue;
            pdValue = pdValue > 50d ? 50d : pdValue;

            // smooth the period
            pdValue = (0.2 * pdValue) + (0.8 * pd[i - 1]);
            pd.Add(pdValue);

            double sdValue = (0.33 * pd[i]) + (0.67 * sd[i - 1]);
            sd.Add(sdValue);

            // smooth dominant cycle period
            int dcPeriods = (int)(double.IsNaN(sd[i]) ? 0 : sd[i] + 0.5);
            double sumPr = 0;
            int effectiveDcPeriods = dcPeriods;

            for (int d = i - dcPeriods + 1; d <= i; d++)
            {
                if (d >= 0)
                {
                    sumPr += pr[d];
                }
                else
                {
                    effectiveDcPeriods--;
                }
            }

            double itValue = effectiveDcPeriods > 0 ? sumPr / effectiveDcPeriods : pr[i];
            it.Add(itValue);

            // final indicators
            double? trendline = i >= 11 // 12th bar
                ? (((4 * it[i]) + (3 * it[i - 1]) + (2 * it[i - 2]) + it[i - 3]) / 10d).NaN2Null()
                : pr[i].NaN2Null();

            double? smoothPrice = sp[i].NaN2Null();

            AddInternal(new HtlResult(
                Timestamp: timestamp,
                DcPeriods: effectiveDcPeriods > 0 ? effectiveDcPeriods : null,
                Trendline: trendline,
                SmoothPrice: smoothPrice));
        }
        else
        {
            // initialization period
            AddInternal(new HtlResult(
                Timestamp: timestamp,
                DcPeriods: null,
                Trendline: pr[i].NaN2Null(),
                SmoothPrice: null));

            pd.Add(0);
            sp.Add(0);
            dt.Add(0);
            i1.Add(0);
            q1.Add(0);
            i2.Add(0);
            q2.Add(0);
            re.Add(0);
            im.Add(0);
            sd.Add(0);
            it.Add(0);
        }

        // Prune state arrays if they exceed MaxBufferSize
        PruneStateArrays();
    }

    /// <summary>
    /// Prunes the internal state arrays to prevent unbounded memory growth.
    /// Removes older data while preserving the minimum required periods for calculations.
    /// </summary>
    private void PruneStateArrays()
    {
        if (pr.Count <= MaxBufferSize)
        {
            return;
        }

        // Calculate how many items to remove
        // Keep at least MinBufferSize (12) elements for lookback calculations
        int removeCount = pr.Count - MinBufferSize;

        if (removeCount > 0)
        {
            RemoveStateRange(removeCount);
        }
    }

    /// <inheritdoc />
    protected override void PruneList()
    {
        int overflow = Count - MaxListSize;

        if (overflow > 0)
        {
            int removable = Math.Min(overflow, Math.Max(0, pr.Count - MinBufferSize));

            if (removable > 0)
            {
                RemoveStateRange(removable);
            }
        }

        base.PruneList();
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        // prefer HL2 when source is an IBar to match StaticSeries behavior
        Add(value.Timestamp, value.Hl2OrValue());
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable v = values[i];
            Add(v.Timestamp, v.Hl2OrValue());
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        pr.Clear();
        sp.Clear();
        dt.Clear();
        pd.Clear();
        q1.Clear();
        i1.Clear();
        q2.Clear();
        i2.Clear();
        re.Clear();
        im.Clear();
        sd.Clear();
        it.Clear();
    }
    private void RemoveStateRange(int count)
    {
        pr.RemoveRange(0, count);
        sp.RemoveRange(0, count);
        dt.RemoveRange(0, count);
        pd.RemoveRange(0, count);
        q1.RemoveRange(0, count);
        i1.RemoveRange(0, count);
        q2.RemoveRange(0, count);
        i2.RemoveRange(0, count);
        re.RemoveRange(0, count);
        im.RemoveRange(0, count);
        sd.RemoveRange(0, count);
        it.RemoveRange(0, count);
    }
}

public static partial class HtTrendline
{
    /// <summary>
    /// Creates a buffer list for Hilbert Transform Instantaneous Trendline (HTL) calculations.
    /// </summary>
    /// <param name="source">Source reusable values.</param>
    public static HtTrendlineList ToHtTrendlineList(
        this IReadOnlyList<IReusable> source)
        => new() { source };
}
