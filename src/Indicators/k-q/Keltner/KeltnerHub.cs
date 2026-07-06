namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a stream hub for calculating Keltner Channels.
/// </summary>
public class KeltnerHub
    : StreamHub<IBar, KeltnerResult>, IKeltner
{

    private readonly int _lookbackPeriods;

    internal KeltnerHub(
        IBarProvider<IBar> provider,
        int emaPeriods,
        double multiplier,
        int atrPeriods) : base(provider)
    {
        Keltner.Validate(emaPeriods, multiplier, atrPeriods);

        EmaPeriods = emaPeriods;
        Multiplier = multiplier;
        AtrPeriods = atrPeriods;
        _lookbackPeriods = Math.Max(emaPeriods, atrPeriods);
        EmaK = 2d / (emaPeriods + 1);
        Name = $"KELTNER({emaPeriods},{multiplier},{atrPeriods})";

        // Validate cache size for warmup requirements
        // ATR requires a prior close for TR, so atrPeriods needs +1
        int requiredWarmup = Math.Max(emaPeriods, atrPeriods + 1);
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int EmaPeriods { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    public int AtrPeriods { get; init; }

    /// <inheritdoc/>
    public double EmaK { get; private init; }
    /// <summary>
    /// Calculates the simple moving average of Close prices.
    /// </summary>
    /// <param name="endIndex">Ending index for calculation</param>
    /// <param name="periods">Number of periods</param>
    private double CalculateSmaOfClose(int endIndex, int periods)
    {
        if (endIndex < periods - 1 || endIndex + 1 > ProviderCache.Count)
        {
            return double.NaN;
        }

        double sum = 0;
        for (int i = endIndex - periods + 1; i <= endIndex; i++)
        {
            sum += (double)ProviderCache[i].Close;
        }

        return sum / periods;
    }

    /// <inheritdoc/>
    protected override (KeltnerResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate EMA of Close
        double ema;
        if (i >= EmaPeriods - 1 && Cache[i - 1].Centerline is not null)
        {
            // Calculate EMA normally
            ema = Ema.Increment(EmaK, Cache[i - 1].Centerline!.Value, (double)item.Close);
        }
        else if (i >= EmaPeriods - 1)
        {
            // Initialize as SMA of Close prices
            ema = CalculateSmaOfClose(i, EmaPeriods);
        }
        else
        {
            // warmup periods are never calculable
            ema = double.NaN;
        }

        // Calculate ATR
        double atr;

        if (i == 0)
        {
            atr = double.NaN;
        }
        else if (Cache[i - 1].Atr is not null)
        {
            // Calculate ATR normally using previous ATR
            AtrResult atrResult = Atr.Increment(AtrPeriods, item, (double)ProviderCache[i - 1].Close, Cache[i - 1].Atr);
            atr = atrResult.Atr ?? double.NaN;
        }
        else if (i >= AtrPeriods)
        {
            // Initialize ATR using same method as Series:
            // Sum TR from index 1 to AtrPeriods, then incrementally update to current index
            double sumTr = 0;

            // Initial sum from index 1 to AtrPeriods (matching Series behavior)
            for (int p = 1; p <= AtrPeriods; p++)
            {
                sumTr += Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);
            }

            double prevAtr = sumTr / AtrPeriods;

            // Incrementally update ATR from AtrPeriods+1 to i
            for (int p = AtrPeriods + 1; p <= i; p++)
            {
                double tr = Tr.Increment(
                    (double)ProviderCache[p].High,
                    (double)ProviderCache[p].Low,
                    (double)ProviderCache[p - 1].Close);

                prevAtr = ((prevAtr * (AtrPeriods - 1)) + tr) / AtrPeriods;
            }

            atr = prevAtr;
        }
        else
        {
            atr = double.NaN;
        }

        // Only calculate bands when both EMA and ATR are ready
        if (i >= _lookbackPeriods - 1)
        {
            double? atrSpan = atr.NaN2Null() * Multiplier;

            KeltnerResult r = new(
                Timestamp: item.Timestamp,
                UpperBand: ema.NaN2Null() + atrSpan,
                Centerline: ema.NaN2Null(),
                LowerBand: ema.NaN2Null() - atrSpan,
                Width: ema == 0 ? null : 2 * atrSpan / ema) {
                Atr = atr.NaN2Null()
            };

            return (r, i);
        }
        else
        {
            // During warmup, return empty result with just timestamp
            KeltnerResult r = new(Timestamp: item.Timestamp);

            return (r, i);
        }
    }

}

public static partial class Keltner
{
    /// <summary>
    /// Creates a Keltner Channels streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="emaPeriods">Number of periods for the EMA.</param>
    /// <param name="multiplier">Multiplier for the ATR.</param>
    /// <param name="atrPeriods">Number of periods for the ATR.</param>
    /// <returns>An instance of <see cref="KeltnerHub"/>.</returns>
    public static KeltnerHub ToKeltnerHub(
        this IBarProvider<IBar> barProvider,
        int emaPeriods = 20,
        double multiplier = 2,
        int atrPeriods = 10)
        => new(barProvider, emaPeriods, multiplier, atrPeriods);
}
