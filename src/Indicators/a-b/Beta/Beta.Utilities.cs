namespace FacioQuo.Stock.Indicators;

public static partial class Beta
{
    /// <summary>
    /// parameter validation
    /// </summary>
    /// <typeparam name="T">Type of record</typeparam>
    /// <param name="sourceEval">Eval bar series</param>
    /// <param name="sourceMrkt">Market bar series</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    /// <exception cref="InvalidBarsException">Thrown when bars are invalid or insufficient</exception>
    internal static void Validate<T>(
        IReadOnlyList<T> sourceEval,
        IReadOnlyList<T> sourceMrkt,
        int lookbackPeriods)
        where T : ISeries
    {
        // check parameter arguments
        Validate(lookbackPeriods);

        // check bars
        if (sourceEval.Count != sourceMrkt.Count)
        {
            throw new InvalidBarsException(
                nameof(sourceEval),
                "Eval bars should have the same number of Market bars for Beta.");
        }
    }

    /// <summary>
    /// Validates the lookback periods for Beta calculations.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 0.</exception>
    internal static void Validate(int lookbackPeriods)
    {
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for Beta.");
        }
    }

    /// <summary>
    /// Calculates the period return of a value.
    /// </summary>
    /// <param name="value">Current value.</param>
    /// <param name="previous">Previous value, or 0 before the first.</param>
    /// <returns>The return; 0 when the previous value is 0.</returns>
    internal static double Return(double value, double previous)
        => previous != 0 ? (value / previous) - 1d : 0;

    /// <summary>
    /// Calculates a Beta result from a full lookback window of returns.
    /// </summary>
    /// <param name="timestamp">Timestamp of the result.</param>
    /// <param name="mrktWindow">Market returns in the lookback window, oldest first.</param>
    /// <param name="evalWindow">Evaluated asset returns in the lookback window, oldest first.</param>
    /// <param name="type">Type of Beta calculation.</param>
    /// <param name="returnsEval">Evaluated asset return at this timestamp.</param>
    /// <param name="returnsMrkt">Market return at this timestamp.</param>
    /// <returns>The Beta result.</returns>
    internal static BetaResult Increment(
        DateTime timestamp,
        ReadOnlySpan<double> mrktWindow,
        ReadOnlySpan<double> evalWindow,
        BetaType type,
        double returnsEval,
        double returnsMrkt)
    {
        double? beta = null;
        double? betaUp = null;
        double? betaDown = null;
        double? ratio = null;
        double? convexity = null;

        // calculate beta variants
        if (type is BetaType.All or BetaType.Standard)
        {
            beta = CalcBetaWindow(mrktWindow, evalWindow, BetaType.Standard);
        }

        if (type is BetaType.All or BetaType.Down)
        {
            betaDown = CalcBetaWindow(mrktWindow, evalWindow, BetaType.Down);
        }

        if (type is BetaType.All or BetaType.Up)
        {
            betaUp = CalcBetaWindow(mrktWindow, evalWindow, BetaType.Up);
        }

        // ratio and convexity
        if (type == BetaType.All && betaUp != null && betaDown != null)
        {
            ratio = betaDown != 0 ? betaUp / betaDown : null;
            convexity = (betaUp - betaDown) * (betaUp - betaDown);
        }

        return new(
            Timestamp: timestamp,
            Beta: beta,
            BetaUp: betaUp,
            BetaDown: betaDown,
            Ratio: ratio,
            Convexity: convexity,
            ReturnsEval: returnsEval,
            ReturnsMrkt: returnsMrkt);
    }

    /// <summary>
    /// Calculates the Beta value for a window of returns.
    /// </summary>
    /// <param name="mrktWindow">Market returns in the lookback window.</param>
    /// <param name="evalWindow">Evaluated asset returns in the lookback window.</param>
    /// <param name="type">Type of Beta calculation; <see cref="BetaType.All"/> is ineligible.</param>
    /// <returns>Calculated Beta value.</returns>
    private static double? CalcBetaWindow(
        ReadOnlySpan<double> mrktWindow,
        ReadOnlySpan<double> evalWindow,
        BetaType type)
    {
        // initialize
        double? beta = null;
        List<double> dataA = new(mrktWindow.Length);
        List<double> dataB = new(mrktWindow.Length);

        for (int p = 0; p < mrktWindow.Length; p++)
        {
            double a = mrktWindow[p];
            double b = evalWindow[p];

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
