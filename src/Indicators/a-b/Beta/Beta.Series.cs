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

            evalReturns[i] = Return(eval.Value, prevE);
            mrktReturns[i] = Return(mrkt.Value, prevM);

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

            results.Add(Increment(
                eval.Timestamp,
                mrktReturns.AsSpan(i - lookbackPeriods + 1, lookbackPeriods),
                evalReturns.AsSpan(i - lookbackPeriods + 1, lookbackPeriods),
                type,
                evalReturns[i],
                mrktReturns[i]));
        }

        return results;
    }
}
