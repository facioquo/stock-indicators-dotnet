using FacioQuo.Stock.Indicators;

namespace Custom.Stock.Indicators
{

    [Serializable]
    public record AtrWmaResult
    (
        DateTime Timestamp,
        double? AtrWma = null
    ) : IReusable
    {
        /// <inheritdoc/>
        double IReusable.Value => AtrWma.Null2NaN();
    }

    public static partial class CustomIndicators
    {
        /// <summary>
        /// Calculates an ATR-weighted moving average of closing prices.
        /// </summary>
        /// <param name="bars">Historical price bars in ascending order.</param>
        /// <param name="lookbackPeriods">Number of periods in the weighted average.</param>
        /// <returns>ATR-weighted moving-average results aligned to the input.</returns>
        public static IReadOnlyList<AtrWmaResult> ToAtrWma(
            this IReadOnlyList<IBar> bars,
            int lookbackPeriods)
        {
            ArgumentNullException.ThrowIfNull(bars);
            if (lookbackPeriods <= 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lookbackPeriods),
                    "Lookback periods must be greater than 1.");
            }

            List<AtrWmaResult> results = new(bars.Count);

            IReadOnlyList<AtrResult> atrResults = bars.ToAtr(lookbackPeriods);

            for (int i = 0; i < bars.Count; i++)
            {
                IBar b = bars[i];
                double atrWma = double.NaN;

                if (i >= lookbackPeriods - 1)
                {
                    double sumWma = 0;
                    double sumAtr = 0;

                    for (int p = i - lookbackPeriods + 1; p <= i; p++)
                    {
                        double close = (double)bars[p].Close;
                        double atr = atrResults[p].Atr ?? double.NaN;

                        sumWma += atr * close;
                        sumAtr += atr;
                    }

                    atrWma = sumAtr != 0 ? sumWma / sumAtr : double.NaN;
                }

                results.Add(new AtrWmaResult(
                    Timestamp: b.Timestamp,
                    AtrWma: atrWma.NaN2Null()));
            }

            return results;
        }
    }

}
