namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Beta coefficient indicator.
/// </summary>
public static partial class Beta
{
    /// <summary>
    /// Calculates the Beta coefficient for a series of data.
    /// </summary>
    /// <param name="sourceEval">Source data for the evaluated asset.</param>
    /// <param name="sourceMrkt">Source data for the market.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation. Default is <see cref="BetaType.Standard"/>.</param>
    /// <returns>A list of Beta results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when sourceEval or sourceMrkt is null.</exception>
    /// <exception cref="InvalidBarsException">Thrown when the timestamps of sourceEval and sourceMrkt do not match.</exception>
    public static IReadOnlyList<BetaResult> ToBeta(
        this IReadOnlyList<IReusable> sourceEval,
        IReadOnlyList<IReusable> sourceMrkt,
        int lookbackPeriods,
        BetaType type = BetaType.Standard)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceMrkt);
        Validate(sourceEval, sourceMrkt, lookbackPeriods);

        // initialize
        int length = sourceEval.Count;
        List<BetaResult> results = new(length);

        bool calcSd = type is BetaType.All or BetaType.Standard;
        bool calcUp = type is BetaType.All or BetaType.Up;
        bool calcDn = type is BetaType.All or BetaType.Down;

        // convert bars to returns
        double[] evalReturns = new double[length];
        double[] mrktReturns = new double[length];
        double prevE = 0;
        double prevM = 0;

        for (int i = 0; i < length; i++)
        {
            IReusable eval = sourceEval[i];
            IReusable mrkt = sourceMrkt[i];

            if (eval.Timestamp != mrkt.Timestamp)
            {
                throw new InvalidBarsException(
                    nameof(sourceEval), eval.Timestamp,
                    "Timestamp sequence does not match.  " +
                    "Beta requires matching dates in provided bars.");
            }

            evalReturns[i] = prevE != 0 ? (eval.Value / prevE) - 1d : 0;
            mrktReturns[i] = prevM != 0 ? (mrkt.Value / prevM) - 1d : 0;

            prevE = eval.Value;
            prevM = mrkt.Value;
        }

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            IReusable eval = sourceEval[i];

            // skip warmup periods
            if (i < lookbackPeriods)
            {
                results.Add(new(
                    Timestamp: eval.Timestamp,
                    ReturnsEval: evalReturns[i],
                    ReturnsMrkt: mrktReturns[i]));

                continue;
            }

            double? beta = null;
            double? betaUp = null;
            double? betaDown = null;
            double? ratio = null;
            double? convexity = null;

            // calculate beta variants
            if (calcSd)
            {
                beta = CalcBetaWindow(i, lookbackPeriods,
                    mrktReturns, evalReturns, BetaType.Standard);
            }

            if (calcDn)
            {
                betaDown = CalcBetaWindow(i, lookbackPeriods,
                    mrktReturns, evalReturns, BetaType.Down);
            }

            if (calcUp)
            {
                betaUp = CalcBetaWindow(i, lookbackPeriods,
                    mrktReturns, evalReturns, BetaType.Up);
            }

            // ratio and convexity
            if (type == BetaType.All && betaUp != null && betaDown != null)
            {
                ratio = betaDown != 0 ? betaUp / betaDown : null;
                convexity = (betaUp - betaDown) * (betaUp - betaDown);
            }

            results.Add(
            new(Timestamp: eval.Timestamp,
                Beta: beta,
                BetaUp: betaUp,
                BetaDown: betaDown,
                Ratio: ratio,
                Convexity: convexity,
                ReturnsEval: evalReturns[i],
                ReturnsMrkt: mrktReturns[i]));
        }

        return results;
    }

    /// <summary>
    /// Calculates the Beta value for a specific window of data.
    /// </summary>
    /// <param name="i">Current index in the data.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="mrktReturns">Market returns data.</param>
    /// <param name="evalReturns">Evaluated asset returns data.</param>
    /// <param name="type">Type of Beta calculation.</param>
    /// <returns>Calculated Beta value.</returns>
    private static double? CalcBetaWindow(
        int i,
        int lookbackPeriods,
        double[] mrktReturns,
        double[] evalReturns,
        BetaType type)
    {
        // note: BetaType.All is ineligible for this method

        // initialize
        double? beta = null;
        List<double> dataA = new(lookbackPeriods);
        List<double> dataB = new(lookbackPeriods);

        for (int p = i - lookbackPeriods + 1; p <= i; p++)
        {
            double a = mrktReturns[p];
            double b = evalReturns[p];

            if (type is BetaType.Standard
            || (type is BetaType.Down && a < 0)
            || (type is BetaType.Up && a > 0))
            {
                dataA.Add(a);
                dataB.Add(b);
            }
        }

        if (dataA.Count == 0)
        {
            return beta;
        }

        // calculate correlation, covariance, and variance
        CorrResult c = Correlation.PeriodCorrelation(
            default,
            [.. dataA],
            [.. dataB]);

        // calculate beta
        if (c.VarianceA != 0)
        {
            beta = (c.Covariance / c.VarianceA).NaN2Null();
        }

        return beta;
    }
}
