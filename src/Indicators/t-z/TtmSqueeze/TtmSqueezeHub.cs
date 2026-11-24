namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for TTM Squeeze on a series of bars.
/// </summary>
public class TtmSqueezeHub
    : ChainHub<IBar, TtmSqueezeResult>, ITtmSqueeze
{
    // windows rebuilt from the caches on each item, oldest first, for the shared kernels
    private readonly double[] _highs;
    private readonly double[] _lows;
    private readonly double[] _closes;
    private readonly double[] _trueRanges;
    private readonly double[] _deltas;

    internal TtmSqueezeHub(
        IBarProvider<IBar> provider,
        int lookbackPeriods,
        double standardDeviations,
        double multiplier) : base(provider)
    {
        TtmSqueeze.Validate(lookbackPeriods, standardDeviations, multiplier);
        LookbackPeriods = lookbackPeriods;
        StandardDeviations = standardDeviations;
        Multiplier = multiplier;

        _highs = new double[lookbackPeriods];
        _lows = new double[lookbackPeriods];
        _closes = new double[lookbackPeriods];
        _trueRanges = new double[lookbackPeriods];
        _deltas = new double[lookbackPeriods];

        Name = $"TTM-SQUEEZE({lookbackPeriods},{standardDeviations},{multiplier})";

        // momentum smooths N deltas, each from N bars
        ValidateCacheSize((2 * lookbackPeriods) - 1, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double StandardDeviations { get; init; }

    /// <inheritdoc/>
    public double Multiplier { get; init; }

    /// <inheritdoc/>
    protected override (TtmSqueezeResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        if (i < LookbackPeriods - 1)
        {
            return (new TtmSqueezeResult(item.Timestamp), i);
        }

        int start = i + 1 - LookbackPeriods;
        bool hasTrueRanges = i >= LookbackPeriods;

        for (int p = 0; p < LookbackPeriods; p++)
        {
            IBar b = ProviderCache[start + p];

            _highs[p] = (double)b.High;
            _lows[p] = (double)b.Low;
            _closes[p] = (double)b.Close;

            if (hasTrueRanges)
            {
                _trueRanges[p] = Tr.Increment(
                    _highs[p], _lows[p], (double)ProviderCache[start + p - 1].Close);
            }
        }

        (double delta, bool? isSqueezeOn) = TtmSqueeze.Increment(
            _highs,
            _lows,
            _closes,
            hasTrueRanges ? _trueRanges : [],
            StandardDeviations,
            Multiplier);

        double? momentum = null;

        if (i >= (2 * LookbackPeriods) - 2)
        {
            // earlier deltas are kept on the cached results
            for (int p = 0; p < LookbackPeriods - 1; p++)
            {
                _deltas[p] = Cache[start + p].Delta ?? double.NaN;
            }

            _deltas[^1] = delta;
            momentum = TtmSqueeze.LinearRegression(_deltas).NaN2Null();
        }

        TtmSqueezeResult r = new(
            Timestamp: item.Timestamp,
            Momentum: momentum,
            IsSqueezeOn: isSqueezeOn) {
            Delta = delta.NaN2Null()
        };

        return (r, i);
    }
}

public static partial class TtmSqueeze
{
    /// <summary>
    /// Creates a TTM Squeeze streaming hub from a bar provider.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="lookbackPeriods">Number of periods for the bands, channels, and momentum.</param>
    /// <param name="standardDeviations">Width of the Bollinger Bands, in standard deviations.</param>
    /// <param name="multiplier">Width of the Keltner Channels, in average true ranges.</param>
    /// <returns>A TtmSqueezeHub instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the bar provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are out of their valid range.</exception>
    public static TtmSqueezeHub ToTtmSqueezeHub(
        this IBarProvider<IBar> barProvider,
        int lookbackPeriods = 20,
        double standardDeviations = 2,
        double multiplier = 1.5)
        => new(barProvider, lookbackPeriods, standardDeviations, multiplier);
}
