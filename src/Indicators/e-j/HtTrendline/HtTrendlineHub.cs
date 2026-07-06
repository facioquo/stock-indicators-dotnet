namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Hilbert Transform Instantaneous Trendline (HTL) indicator.
/// </summary>
public class HtTrendlineHub
    : ChainHub<IReusable, HtlResult>, IHtTrendline
{

    private readonly List<double> pr = [];   // price
    private readonly List<double> sp = [];   // smooth price
    private readonly List<double> dt = [];   // detrender
    private readonly List<double> pd = [];   // period

    private readonly List<double> q1 = [];   // quadrature
    private readonly List<double> i1 = [];   // in-phase

    private readonly List<double> q2 = [];   // adj. quadrature
    private readonly List<double> i2 = [];   // adj. in-phase

    private readonly List<double> re = [];
    private readonly List<double> im = [];

    private readonly List<double> sd = [];   // smooth period
    private readonly List<double> it = [];   // instantaneous trend (raw)

    internal HtTrendlineHub(
        IChainProvider<IReusable> provider) : base(provider)
    {
        Name = "HTL()";
        // Validate cache size for warmup requirements
        ValidateCacheSize(63, Name);  // Hilbert Transform needs 63-period minimum

        Reinitialize();
    }
    /// <inheritdoc/>
    protected override (HtlResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);
        double prValue = item.Hl2OrValue(); // Use HL2 for bars, Value for reusables

        pr.Add(prValue);

        if (i > 5)
        {
            double adj = (0.075 * pd[i - 1]) + 0.54;

            // smooth and detrender
            sp.Add(((4 * pr[i]) + (3 * pr[i - 1]) + (2 * pr[i - 2]) + pr[i - 3]) / 10);
            dt.Add(((0.0962 * sp[i]) + (0.5769 * sp[i - 2]) - (0.5769 * sp[i - 4]) - (0.0962 * sp[i - 6])) * adj);

            // in-phase and quadrature
            q1.Add(((0.0962 * dt[i]) + (0.5769 * dt[i - 2]) - (0.5769 * dt[i - 4]) - (0.0962 * dt[i - 6])) * adj);
            i1.Add(dt[i - 3]);

            // advance the phases by 90 degrees
            double jI = ((0.0962 * i1[i]) + (0.5769 * i1[i - 2]) - (0.5769 * i1[i - 4]) - (0.0962 * i1[i - 6])) * adj;
            double jQ = ((0.0962 * q1[i]) + (0.5769 * q1[i - 2]) - (0.5769 * q1[i - 4]) - (0.0962 * q1[i - 6])) * adj;

            // phasor addition for 3-bar averaging
            i2.Add(i1[i] - jQ);
            q2.Add(q1[i] + jI);

            i2[i] = (0.2 * i2[i]) + (0.8 * i2[i - 1]);  // smoothing it
            q2[i] = (0.2 * q2[i]) + (0.8 * q2[i - 1]);

            // homodyne discriminator
            re.Add((i2[i] * i2[i - 1]) + (q2[i] * q2[i - 1]));
            im.Add((i2[i] * q2[i - 1]) - (q2[i] * i2[i - 1]));

            re[i] = (0.2 * re[i]) + (0.8 * re[i - 1]);  // smoothing it
            im[i] = (0.2 * im[i]) + (0.8 * im[i - 1]);

            // calculate period
            pd.Add(im[i] != 0 && re[i] != 0
                ? 2 * Math.PI / DeMath.Atan(im[i] / re[i])
                : 0d);

            // adjust period to thresholds
            pd[i] = pd[i] > 1.5 * pd[i - 1] ? 1.5 * pd[i - 1] : pd[i];
            pd[i] = pd[i] < 0.67 * pd[i - 1] ? 0.67 * pd[i - 1] : pd[i];
            pd[i] = pd[i] < 6d ? 6d : pd[i];
            pd[i] = pd[i] > 50d ? 50d : pd[i];

            // smooth the period
            pd[i] = (0.2 * pd[i]) + (0.8 * pd[i - 1]);
            sd.Add((0.33 * pd[i]) + (0.67 * sd[i - 1]));

            // smooth dominant cycle period
            int dcPeriods = (int)(double.IsNaN(sd[i]) ? 0 : sd[i] + 0.5);
            double sumPr = 0;
            for (int d = i - dcPeriods + 1; d <= i; d++)
            {
                if (d >= 0)
                {
                    sumPr += pr[d];
                }

                // handle insufficient lookback bars (trim scope)
                else
                {
                    dcPeriods--;
                }
            }

            it.Add(dcPeriods > 0 ? sumPr / dcPeriods : pr[i]);

            // final indicators
            HtlResult result = new(

                Timestamp: item.Timestamp,
                DcPeriods: dcPeriods > 0 ? dcPeriods : null,

                Trendline: i >= 11 // 12th bar
                  ? (((4 * it[i]) + (3 * it[i - 1]) + (2 * it[i - 2]) + it[i - 3]) / 10d).NaN2Null()
                  : pr[i].NaN2Null(),

                SmoothPrice: sp[i].NaN2Null());

            return (result, i);
        }

        // initialization period
        else
        {
            HtlResult result = new(
                Timestamp: item.Timestamp,
                DcPeriods: null,
                Trendline: pr[i].NaN2Null(),
                SmoothPrice: null);

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
            it.Add(0); // Add it element in initialization period too

            return (result, i);
        }
    }

    /// <summary>
    /// Restores the state up to the specified timestamp by clearing and rebuilding from cache.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // clear all state arrays
        pr.Clear();
        sp.Clear();
        dt.Clear();
        q1.Clear();
        i1.Clear();
        q2.Clear();
        i2.Clear();
        re.Clear();
        im.Clear();
        pd.Clear();
        sd.Clear();
        it.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        // replay calculations to rebuild state
        for (int p = 0; p <= restoreIndex; p++)
        {
            IReusable reusable = ProviderCache[p];
            _ = ToIndicator(reusable, p);
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
            sp.RemoveRange(0, excessCount);
            dt.RemoveRange(0, excessCount);
            q1.RemoveRange(0, excessCount);
            i1.RemoveRange(0, excessCount);
            q2.RemoveRange(0, excessCount);
            i2.RemoveRange(0, excessCount);
            re.RemoveRange(0, excessCount);
            im.RemoveRange(0, excessCount);
            pd.RemoveRange(0, excessCount);
            sd.RemoveRange(0, excessCount);
            it.RemoveRange(0, excessCount);
        }
    }
}

public static partial class HtTrendline
{
    /// <summary>
    /// Creates an HtTrendline streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <returns>An HtTrendline hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    public static HtTrendlineHub ToHtTrendlineHub(
        this IChainProvider<IReusable> chainProvider)
    {
        ArgumentNullException.ThrowIfNull(chainProvider);
        return new(chainProvider);
    }
}
