namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for correlation of two chain providers over a lookback window, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is series A and the other is series B. A result is emitted once both
/// providers hold a timestamp. A timestamp that only one provider holds produces no
/// result; when the missing value arrives late, the usual rebuild adds it. Both providers
/// must be driven from one thread at a time, as they are when both descend from one
/// <see cref="BarHub"/>.
/// </remarks>
public class CorrelationHub
    : ChainHub<PairedValue, CorrResult>, ICorrelation
{
    private readonly double[] _dataA;
    private readonly double[] _dataB;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationHub"/> class.
    /// </summary>
    /// <param name="provider">Hub that pairs series A and B.</param>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provider's cache cannot hold the lookback window.</exception>
    internal CorrelationHub(
        PairHub provider,
        int lookbackPeriods) : base(provider)
    {
        LookbackPeriods = lookbackPeriods;
        _dataA = new double[lookbackPeriods];
        _dataB = new double[lookbackPeriods];
        Name = $"CORR({lookbackPeriods})";

        ValidateCacheSize(lookbackPeriods, Name);

        // the pairing hub was built for this hub alone
        OwnProvider();

        Reinitialize();
    }

    /// <inheritdoc />
    public int LookbackPeriods { get; }

    /// <inheritdoc/>
    protected override (CorrResult result, int index)
        ToIndicator(PairedValue item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        if (i < LookbackPeriods - 1)
        {
            return (new CorrResult(item.Timestamp), i);
        }

        int z = 0;
        for (int p = i + 1 - LookbackPeriods; p <= i; p++)
        {
            PairedValue pair = ProviderCache[p];
            _dataA[z] = pair.Value;
            _dataB[z] = pair.Other;
            z++;
        }

        return (Correlation.PeriodCorrelation(item.Timestamp, _dataA, _dataB), i);
    }
}

public static partial class Correlation
{
    /// <summary>
    /// Creates a stream hub of correlation values between two chain providers.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one provider holds produces no result, where the Series method
    /// throws. Drive both providers from one thread at a time.
    /// </remarks>
    /// <param name="sourceA">Chain provider of series A.</param>
    /// <param name="sourceB">Chain provider of series B.</param>
    /// <param name="lookbackPeriods">The number of periods to look back for the calculation.</param>
    /// <returns>A stream hub of correlation results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0, or exceed what the
    /// provider's cache can hold.
    /// </exception>
    public static CorrelationHub ToCorrelationHub(
        this IChainProvider<IReusable> sourceA,
        IChainProvider<IReusable> sourceB,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceA);
        ArgumentNullException.ThrowIfNull(sourceB);

        Validate(lookbackPeriods);

        return PairHub.Build(sourceA, sourceB, pair => new CorrelationHub(pair, lookbackPeriods));
    }
}
