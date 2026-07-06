namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for generating Renko chart series.
/// </summary>
public static partial class Renko
{
    /// <summary>
    /// Converts a list of bars to a list of Renko chart results.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="brickSize">Size of each Renko brick.</param>
    /// <param name="endType">Price candle end type to use as the brick threshold.</param>
    /// <returns>A list of Renko chart results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    public static IReadOnlyList<RenkoResult> ToRenko(
        this IReadOnlyList<IBar> bars,
        decimal brickSize,
        EndType endType = EndType.Close)
    {
        // check parameter arguments
        ArgumentNullException.ThrowIfNull(bars);
        Validate(brickSize);

        // initialize
        int length = bars.Count;
        List<RenkoResult> results = new(length);

        if (length == 0)
        {
            return results;
        }

        // first brick baseline
        IBar q0 = bars[0];

        int decimals = brickSize.GetDecimalPlaces();
        decimal baseline = Math.Round(q0.Close, Math.Max(decimals - 1, 0));

        RenkoResult lastBrick = new(
            q0.Timestamp,
            Open: baseline, 0, 0,
            Close: baseline, 0, false);

        // initialize high/low/volume tracking
        decimal h = decimal.MinValue;
        decimal l = decimal.MaxValue;
        decimal sumV = 0;  // cumulative

        // roll through source values
        for (int i = 1; i < length; i++)
        {
            IBar q = bars[i];

            // track high/low/volume between bricks
            h = Math.Max(h, q.High);
            l = Math.Min(l, q.Low);
            sumV += q.Volume;

            // determine new brick quantity
            int newBrickQty = GetNewBrickQuantity(q, lastBrick, brickSize, endType);
            int absBrickQty = Math.Abs(newBrickQty);
            bool isUp = newBrickQty >= 0;

            // add new brick(s)
            // can add more than one brick!
            for (int b = 0; b < absBrickQty; b++)
            {
                decimal o;
                decimal c;
                decimal v = sumV / absBrickQty;

                if (isUp)
                {
                    o = Math.Max(lastBrick.Open, lastBrick.Close);
                    c = o + brickSize;
                }
                else
                {
                    o = Math.Min(lastBrick.Open, lastBrick.Close);
                    c = o - brickSize;
                }

                RenkoResult r
                    = new(q.Timestamp, o, h, l, c, v, isUp);

                results.Add(r);
                lastBrick = r;
            }

            // reset high/low/volume tracking
            if (absBrickQty != 0)
            {
                h = decimal.MinValue;
                l = decimal.MaxValue;
                sumV = 0;
            }
        }

        return results;
    }
}
