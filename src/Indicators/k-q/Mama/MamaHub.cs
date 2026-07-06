namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for MAMA.
/// </summary>
public class MamaHub
    : ChainHub<IReusable, MamaResult>, IMama
{

    // State arrays for MESA algorithm
    // These arrays grow with each added value to support indexed lookback access
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

    internal MamaHub(
        IChainProvider<IReusable> provider,
        double fastLimit,
        double slowLimit) : base(provider)
    {
        Mama.Validate(fastLimit, slowLimit);
        FastLimit = fastLimit;
        SlowLimit = slowLimit;
        Name = $"MAMA({fastLimit},{slowLimit})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(50, Name);  // MAMA adaptive algorithm needs minimum history

        Reinitialize();
    }

    /// <inheritdoc/>
    public double FastLimit { get; init; }

    /// <inheritdoc/>
    public double SlowLimit { get; init; }
    /// <inheritdoc/>
    protected override (MamaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        // NOTE: MAMA has complex internal state.  We maintain state arrays that are
        // truncated on rebuild events.  See RollbackState() override below.
        // For correctness (matching StaticSeries), we re-derive values strictly
        // from prior cached state and provider items.  No partial recalculation shortcuts.

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Ensure arrays have enough capacity
        while (pr.Count <= i)
        {
            pr.Add(0);
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
        }

        // prefer HL2 when available (IBar), matching static series behavior
        pr[i] = item.Hl2OrValue();

        // Skip incalculable periods
        if (i < 5)
        {
            return (new MamaResult(item.Timestamp), i);
        }

        double mama;
        double fama;

        // Get previous values from cache
        double prevMama = double.NaN;
        double prevFama = double.NaN;

        if (i > 0 && Cache.Count > i - 1)
        {
            MamaResult prev = Cache[i - 1];
            if (prev.Mama.HasValue)
            {
                prevMama = prev.Mama.Value;
            }

            if (prev.Fama.HasValue)
            {
                prevFama = prev.Fama.Value;
            }
        }

        // Initialization
        if (double.IsNaN(prevMama))
        {
            double sum = 0;
            for (int p = i - 5; p <= i; p++)
            {
                pd[p] = 0;
                sm[p] = 0;
                dt[p] = 0;

                i1[p] = 0;
                q1[p] = 0;
                i2[p] = 0;
                q2[p] = 0;

                re[p] = 0;
                im[p] = 0;

                ph[p] = 0;

                sum += pr[p];
            }

            mama = fama = sum / 6;
        }

        // Normal MAMA calculation
        else
        {
            double adj = (0.075 * pd[i - 1]) + 0.54;

            // Smooth and detrender
            sm[i] = ((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10;
            dt[i] = ((0.0962 * sm[i]) + (0.5769 * sm[i - 2]) - (0.5769 * sm[i - 4]) - (0.0962 * sm[i - 6])) * adj;

            // In-phase and quadrature
            q1[i] = ((0.0962 * dt[i]) + (0.5769 * dt[i - 2]) - (0.5769 * dt[i - 4]) - (0.0962 * dt[i - 6])) * adj;
            i1[i] = dt[i - 3];

            // Advance the phases by 90 degrees
            double jI = ((0.0962 * i1[i]) + (0.5769 * i1[i - 2]) - (0.5769 * i1[i - 4]) - (0.0962 * i1[i - 6])) * adj;
            double jQ = ((0.0962 * q1[i]) + (0.5769 * q1[i - 2]) - (0.5769 * q1[i - 4]) - (0.0962 * q1[i - 6])) * adj;

            // Phasor addition for 3-bar averaging
            i2[i] = i1[i] - jQ;
            q2[i] = q1[i] + jI;

            i2[i] = (0.2 * i2[i]) + (0.8 * i2[i - 1]);  // smoothing it
            q2[i] = (0.2 * q2[i]) + (0.8 * q2[i - 1]);

            // Homodyne discriminator
            re[i] = (i2[i] * i2[i - 1]) + (q2[i] * q2[i - 1]);
            im[i] = (i2[i] * q2[i - 1]) - (q2[i] * i2[i - 1]);

            re[i] = (0.2 * re[i]) + (0.8 * re[i - 1]);  // smoothing it
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

            // Determine phase position
            ph[i] = i1[i] != 0 ? DeMath.Atan(q1[i] / i1[i]) * 180d / Math.PI : 0d;

            // Change in phase
            double delta = Math.Max(ph[i - 1] - ph[i], 1);

            // Adaptive alpha value
            double alpha = Math.Max(FastLimit / delta, SlowLimit);

            // Final indicators
            mama = (alpha * pr[i]) + ((1d - alpha) * prevMama);
            fama = (0.5 * alpha * mama) + ((1d - (0.5 * alpha)) * prevFama);
        }

        MamaResult result = new(
            Timestamp: item.Timestamp,
            Mama: mama.NaN2Null(),
            Fama: fama.NaN2Null());

        return (result, i);
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        if (restoreIndex < 0)
        {
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
            return;
        }

        int keepCount = restoreIndex + 1;
        if (keepCount < pr.Count)
        {
            int removeCount = pr.Count - keepCount;
            pr.RemoveRange(keepCount, removeCount);
            sm.RemoveRange(keepCount, removeCount);
            dt.RemoveRange(keepCount, removeCount);
            pd.RemoveRange(keepCount, removeCount);
            q1.RemoveRange(keepCount, removeCount);
            i1.RemoveRange(keepCount, removeCount);
            q2.RemoveRange(keepCount, removeCount);
            i2.RemoveRange(keepCount, removeCount);
            re.RemoveRange(keepCount, removeCount);
            im.RemoveRange(keepCount, removeCount);
            ph.RemoveRange(keepCount, removeCount);
        }
    }

    /// <inheritdoc/>
    protected override void PruneState(DateTime toTimestamp)
    {
        // Keep state arrays aligned with Cache by ensuring same count.
        // Use Cache.Count directly rather than relying on timestamps since
        // state arrays don't store timestamps.
        int targetSize = Cache.Count;
        if (pr.Count > targetSize)
        {
            int excessCount = pr.Count - targetSize;
            pr.RemoveRange(0, excessCount);
            sm.RemoveRange(0, excessCount);
            dt.RemoveRange(0, excessCount);
            pd.RemoveRange(0, excessCount);
            q1.RemoveRange(0, excessCount);
            i1.RemoveRange(0, excessCount);
            q2.RemoveRange(0, excessCount);
            i2.RemoveRange(0, excessCount);
            re.RemoveRange(0, excessCount);
            im.RemoveRange(0, excessCount);
            ph.RemoveRange(0, excessCount);
        }
    }
}

public static partial class Mama
{
    /// <summary>
    /// Creates a MAMA streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="fastLimit">Fast limit for the MAMA calculation.</param>
    /// <param name="slowLimit">Slow limit for the MAMA calculation.</param>
    /// <returns>A MAMA hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the limits are invalid.</exception>
    public static MamaHub ToMamaHub(
        this IChainProvider<IReusable> chainProvider,
        double fastLimit = 0.5,
        double slowLimit = 0.05)
        => new(chainProvider, fastLimit, slowLimit);
}
