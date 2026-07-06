namespace FacioQuo.Stock.Indicators;

public static partial class Atr
{
    /// <summary>
    /// Calculates the Average True Range (ATR) incrementally.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="high">High price of the current period.</param>
    /// <param name="low">Low price of the current period.</param>
    /// <param name="prevClose">Close price of the previous period.</param>
    /// <param name="prevAtr">ATR value of the previous period.</param>
    /// <returns>ATR value for the current period.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Increment(
        int lookbackPeriods,
        double high,
        double low,
        double prevClose,
        double prevAtr)
    {
        double tr = Tr.Increment(high, low, prevClose);
        return ((prevAtr * (lookbackPeriods - 1)) + tr) / lookbackPeriods;
    }

    /// <summary>
    /// Calculates the Average True Range (ATR) incrementally for a given bar.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bar">Current bar.</param>
    /// <param name="prevClose">Close price of the previous period.</param>
    /// <param name="prevAtr">ATR value of the previous period.</param>
    /// <returns>An <see cref="AtrResult"/> containing the ATR values for the current period.</returns>
    public static AtrResult Increment(
        int lookbackPeriods,
        IBar bar,
        double prevClose,
        double? prevAtr)
    {
        ArgumentNullException.ThrowIfNull(bar);

        double high = (double)bar.High;
        double low = (double)bar.Low;
        double close = (double)bar.Close;

        double tr = Tr.Increment(high, low, prevClose);
        double atr = (((prevAtr ?? double.NaN) * (lookbackPeriods - 1)) + tr) / lookbackPeriods;
        double atrp = close == 0 ? double.NaN : atr / close * 100;

        return new AtrResult(
            bar.Timestamp,
            tr,
            atr.NaN2Null(),
            atrp.NaN2Null());
    }

    /// <summary>
    /// Validates the parameters for the Average True Range (ATR) calculation.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than or equal to 1.</exception>
    internal static void Validate(
        int lookbackPeriods)
    {
        // check parameter arguments
        if (lookbackPeriods <= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 1 for Average True Range.");
        }
    }
}
