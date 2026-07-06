namespace FacioQuo.Stock.Indicators;

/// <summary>
/// MESA Adaptive Moving Average (MAMA) from incremental reusable values.
/// </summary>
/// <remarks>
/// <para>
/// <b>Exception to <see cref="BufferListUtilities"/> Pattern:</b> MAMA does not use the standard
/// <see cref="BufferListUtilities"/> extension methods (Update/UpdateWithDequeue) due to
/// the unique complexity of the MESA (Maximum Entropy Spectrum Analysis) algorithm.
/// </para>
/// <para>
/// <b>Algorithmic Requirements:</b>
/// <list type="bullet">
/// <item>Maintains 11 parallel state arrays (pr, sm, dt, pd, q1, i1, q2, i2, re, im, ph)
/// for different phases of the MESA calculation</item>
/// <item>Requires indexed lookback access up to 6 periods (e.g., <c>sm[i - 6]</c>)
/// for phase calculations</item>
/// <item>Performs multi-stage phasor calculations with cross-referencing between arrays</item>
/// </list>
/// </para>
/// <para>
/// <b>Why <see cref="Queue{T}"/> cannot be used:</b> The standard <see cref="Queue{T}"/> data structure
/// used by <see cref="BufferListUtilities"/> does not provide indexed access to historical values, which is
/// essential for the MESA algorithm's phase and period calculations.
/// </para>
/// <para>
/// <b>Memory Management:</b> This implementation uses <see cref="List{T}"/> arrays to maintain
/// calculation state with automatic pruning to prevent unbounded growth:
/// <list type="bullet">
/// <item>State arrays are pruned at 1000 items, keeping minimum 7 periods for calculations</item>
/// <item>Result list is pruned at <see cref="BufferList{MamaResult}.MaxListSize"/> (default 90% of int.MaxValue)</item>
/// </list>
/// </para>
/// </remarks>
public class MamaList : BufferList<MamaResult>, IIncrementFromChain, IMama
{
    /// <summary>
    /// State arrays for MESA algorithm
    /// These arrays grow with each added value to support indexed lookback access
    /// </summary>
    private readonly List<double> pr = []; // price
    private readonly List<double> sm = []; // smooth
    private readonly List<double> dt = []; // detrender
    private readonly List<double> pd = []; // period

    private readonly List<double> q1 = []; // quadrature
    private readonly List<double> i1 = []; // in-phase

    private readonly List<double> q2 = []; // adj. quadrature
    private readonly List<double> i2 = []; // adj. in-phase

    private readonly List<double> re = []; // real part
    private readonly List<double> im = []; // imaginary part

    private readonly List<double> ph = []; // phase

    private double prevMama = double.NaN;
    private double prevFama = double.NaN;

    /// <summary>
    /// Minimum required for 6-period lookback
    /// </summary>
    private const int MinBufferSize = 7;
    /// <summary>
    /// Trigger point to prune buffers to MinBufferSize
    /// </summary>
    private const int MaxBufferSize = 1000;
    /// <summary>
    /// Initializes a new instance of the <see cref="MamaList"/> class.
    /// </summary>
    /// <param name="fastLimit">Fast limit for the MAMA calculation.</param>
    /// <param name="slowLimit">Slow limit for the MAMA calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="slowLimit"/> is invalid.</exception>
    public MamaList(
        double fastLimit = 0.5,
        double slowLimit = 0.05)
    {
        Mama.Validate(fastLimit, slowLimit);

        FastLimit = fastLimit;
        SlowLimit = slowLimit;

        Name = $"MAMA({fastLimit}, {slowLimit})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MamaList"/> class with initial reusable values.
    /// </summary>
    /// <param name="fastLimit">Fast limit for the MAMA calculation.</param>
    /// <param name="slowLimit">Slow limit for the MAMA calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public MamaList(
        double fastLimit,
        double slowLimit,
        IReadOnlyList<IReusable> values)
        : this(fastLimit, slowLimit) => Add(values);

    /// <inheritdoc />
    public double FastLimit { get; init; }

    /// <inheritdoc />
    public double SlowLimit { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        int i = pr.Count; // new index position

        // append base arrays with placeholders
        pr.Add(value);
        sm.Add(0);
        dt.Add(0);
        pd.Add(0);
        q1.Add(0);
        i1.Add(0);
        q2.Add(0);
        i2.Add(0);
        re.Add(0);
        im.Add(0);
        ph.Add(0);

        double mama;
        double fama;

        // skip until we have 6 price points (index 0..5)
        if (i < 5)
        {
            AddInternal(new MamaResult(timestamp));
            return;
        }

        // initialization at first calculable index
        if (double.IsNaN(prevMama))
        {
            double sum = 0;
            for (int p = i - 5; p <= i; p++) { sum += pr[p]; }

            mama = fama = sum / 6d;
            prevMama = mama;
            prevFama = fama;
            AddInternal(new MamaResult(timestamp, mama, fama));
            return;
        }

        // normal MAMA calculation (mirrors StaticSeries & StreamHub)
        double adj = (0.075 * pd[i - 1]) + 0.54;

        // Smooth & detrender
        sm[i] = ((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10d;
        dt[i] = ((0.0962 * sm[i]) + (0.5769 * sm[i - 2]) - (0.5769 * sm[i - 4]) - (0.0962 * sm[i - 6])) * adj;

        // In-phase & quadrature
        q1[i] = ((0.0962 * dt[i]) + (0.5769 * dt[i - 2]) - (0.5769 * dt[i - 4]) - (0.0962 * dt[i - 6])) * adj;
        i1[i] = dt[i - 3];

        // Advance phases by 90 degrees
        double jI = ((0.0962 * i1[i]) + (0.5769 * i1[i - 2]) - (0.5769 * i1[i - 4]) - (0.0962 * i1[i - 6])) * adj;
        double jQ = ((0.0962 * q1[i]) + (0.5769 * q1[i - 2]) - (0.5769 * q1[i - 4]) - (0.0962 * q1[i - 6])) * adj;

        // Phasor addition for 3-bar averaging
        i2[i] = i1[i] - jQ;
        q2[i] = q1[i] + jI;
        i2[i] = (0.2 * i2[i]) + (0.8 * i2[i - 1]);
        q2[i] = (0.2 * q2[i]) + (0.8 * q2[i - 1]);

        // Homodyne discriminator
        re[i] = (i2[i] * i2[i - 1]) + (q2[i] * q2[i - 1]);
        im[i] = (i2[i] * q2[i - 1]) - (q2[i] * i2[i - 1]);
        re[i] = (0.2 * re[i]) + (0.8 * re[i - 1]);
        im[i] = (0.2 * im[i]) + (0.8 * im[i - 1]);

        // calculate period
        pd[i] = im[i] != 0 && re[i] != 0
            ? 2 * Math.PI / DeMath.Atan(im[i] / re[i])
            : 0d;

        // adjust period to thresholds
        pd[i] = pd[i] > 1.5 * pd[i - 1] ? 1.5 * pd[i - 1] : pd[i];
        pd[i] = pd[i] < 0.67 * pd[i - 1] ? 0.67 * pd[i - 1] : pd[i];
        pd[i] = pd[i] < 6 ? 6 : pd[i];
        pd[i] = pd[i] > 50 ? 50 : pd[i];

        // smooth the period
        pd[i] = (0.2 * pd[i]) + (0.8 * pd[i - 1]);

        // Phase & delta
        ph[i] = i1[i] != 0 ? DeMath.Atan(q1[i] / i1[i]) * 180d / Math.PI : 0d;
        double delta = Math.Max(ph[i - 1] - ph[i], 1d);

        // Adaptive alpha & final outputs
        double alpha = Math.Max(FastLimit / delta, SlowLimit);
        mama = (alpha * pr[i]) + ((1d - alpha) * prevMama);
        fama = (0.5 * alpha * mama) + ((1d - (0.5 * alpha)) * prevFama);

        prevMama = mama;
        prevFama = fama;

        AddInternal(new MamaResult(timestamp, mama.NaN2Null(), fama.NaN2Null()));

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
        // Keep at least MinBufferSize (7) elements for lookback calculations
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
        sm.Clear();
        dt.Clear();
        pd.Clear();
        q1.Clear();
        i1.Clear();
        q2.Clear();
        i2.Clear();
        re.Clear();
        im.Clear();
        ph.Clear();
        prevMama = double.NaN;
        prevFama = double.NaN;
    }
    private void RemoveStateRange(int count)
    {
        pr.RemoveRange(0, count);
        sm.RemoveRange(0, count);
        dt.RemoveRange(0, count);
        pd.RemoveRange(0, count);
        q1.RemoveRange(0, count);
        i1.RemoveRange(0, count);
        q2.RemoveRange(0, count);
        i2.RemoveRange(0, count);
        re.RemoveRange(0, count);
        im.RemoveRange(0, count);
        ph.RemoveRange(0, count);
    }
}

public static partial class Mama
{
    /// <summary>
    /// Creates a buffer list for MESA Adaptive Moving Average (MAMA) calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="fastLimit">Fast limit parameter</param>
    /// <param name="slowLimit">Slow limit parameter</param>
    public static MamaList ToMamaList(
        this IReadOnlyList<IReusable> source,
        double fastLimit = 0.5,
        double slowLimit = 0.05)
        => new(fastLimit, slowLimit) { source };
}
