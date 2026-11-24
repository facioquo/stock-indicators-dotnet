namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for the Beta coefficient of an evaluated provider against a market provider, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is the evaluated provider and the other is the market provider. A result is
/// emitted once both providers hold a timestamp. A timestamp that only one provider holds
/// produces no result; when the missing value arrives late, the usual rebuild adds it.
/// Both providers must be driven from one thread at a time, as they are when both descend
/// from one <see cref="BarHub"/>.
/// </remarks>
public class BetaHub
    : ChainHub<PairedValue, BetaResult>, IBeta
{
    private readonly double[] _evalWindow;
    private readonly double[] _mrktWindow;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetaHub"/> class.
    /// </summary>
    /// <param name="provider">Hub that pairs the evaluated and market series.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provider's cache cannot hold the lookback window.</exception>
    internal BetaHub(
        PairHub provider,
        int lookbackPeriods,
        BetaType type) : base(provider)
    {
        LookbackPeriods = lookbackPeriods;
        Type = type;
        Name = $"BETA({lookbackPeriods},{type.ToString().ToUpperInvariant()})";

        // each window return reads the pair before it; checked before allocating, and
        // clamped, so an oversized lookback is rejected rather than allocated or overflowed
        ValidateCacheSize(lookbackPeriods == int.MaxValue ? int.MaxValue : lookbackPeriods + 1, Name);

        _evalWindow = new double[lookbackPeriods];
        _mrktWindow = new double[lookbackPeriods];

        // the pairing hub was built for this hub alone
        OwnProvider();

        Reinitialize();
    }

    /// <inheritdoc />
    public int LookbackPeriods { get; }

    /// <inheritdoc />
    public BetaType Type { get; }

    /// <inheritdoc/>
    protected override (BetaResult result, int index)
        ToIndicator(PairedValue item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        PairedValue? prev = i > 0 ? ProviderCache[i - 1] : null;
        double returnsEval = Beta.Return(item.Value, prev?.Value ?? 0);
        double returnsMrkt = Beta.Return(item.Other, prev?.Other ?? 0);

        if (i < LookbackPeriods)
        {
            return (new BetaResult(
                Timestamp: item.Timestamp,
                ReturnsEval: returnsEval,
                ReturnsMrkt: returnsMrkt), i);
        }

        int z = 0;
        for (int p = i + 1 - LookbackPeriods; p <= i; p++)
        {
            PairedValue current = ProviderCache[p];
            PairedValue before = ProviderCache[p - 1];
            _evalWindow[z] = Beta.Return(current.Value, before.Value);
            _mrktWindow[z] = Beta.Return(current.Other, before.Other);
            z++;
        }

        return (Beta.Increment(
            item.Timestamp, _mrktWindow, _evalWindow, Type, returnsEval, returnsMrkt), i);
    }
}

public static partial class Beta
{
    /// <summary>
    /// Creates a stream hub of Beta coefficients of an evaluated provider against a market provider.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one provider holds produces no result, where the Series method
    /// throws. Drive both providers from one thread at a time.
    /// </remarks>
    /// <param name="sourceEval">Evaluated chain provider.</param>
    /// <param name="sourceMrkt">Market chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="type">Type of Beta calculation. Default is <see cref="BetaType.Standard"/>.</param>
    /// <returns>A stream hub of Beta results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0, or exceed what the
    /// provider's cache can hold.
    /// </exception>
    public static BetaHub ToBetaHub(
        this IChainProvider<IReusable> sourceEval,
        IChainProvider<IReusable> sourceMrkt,
        int lookbackPeriods,
        BetaType type = BetaType.Standard)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceMrkt);

        Validate(lookbackPeriods);

        return PairHub.Build(sourceEval, sourceMrkt, pair => new BetaHub(pair, lookbackPeriods, type));
    }
}
