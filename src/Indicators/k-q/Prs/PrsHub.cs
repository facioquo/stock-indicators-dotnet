namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Price Relative Strength (PRS) of two chain providers, matched by timestamp.
/// </summary>
/// <remarks>
/// The source is the evaluated provider and the other is the base provider. A result is
/// emitted once both providers hold a timestamp. A timestamp that only one provider holds
/// produces no result; when the missing value arrives late, the usual rebuild adds it.
/// Both providers must be driven from one thread at a time, as they are when both descend
/// from one <see cref="BarHub"/>.
/// </remarks>
public class PrsHub
    : ChainHub<PairedValue, PrsResult>, IPrs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrsHub"/> class.
    /// </summary>
    /// <param name="provider">Hub that pairs the evaluated and base series.</param>
    /// <param name="lookbackPeriods">
    /// The number of periods for the PRS% lookback calculation, or 0 for no PRS%;
    /// the factories validate it.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provider's cache cannot hold the lookback window.</exception>
    internal PrsHub(
        PairHub provider,
        int lookbackPeriods) : base(provider)
    {
        LookbackPeriods = lookbackPeriods;

        if (lookbackPeriods == 0)
        {
            Name = "PRS";
        }
        else
        {
            Name = $"PRS({lookbackPeriods})";

            // clamped, so the largest lookback fails the cache check rather than overflowing
            ValidateCacheSize(lookbackPeriods == int.MaxValue ? int.MaxValue : lookbackPeriods + 1, Name);
        }

        // the pairing hub was built for this hub alone
        OwnProvider();

        Reinitialize();
    }

    /// <inheritdoc />
    /// <remarks>0 when the hub calculates no <see cref="PrsResult.PrsPercent"/>.</remarks>
    public int LookbackPeriods { get; }

    /// <inheritdoc/>
    protected override (PrsResult result, int index)
        ToIndicator(PairedValue item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? prsPercent = null;

        if (LookbackPeriods > 0 && i >= LookbackPeriods)
        {
            PairedValue back = ProviderCache[i - LookbackPeriods];
            prsPercent = Prs.Percent(item.Value, item.Other, back.Value, back.Other);
        }

        return (new PrsResult(
            Timestamp: item.Timestamp,
            Prs: Prs.Ratio(item.Value, item.Other),
            PrsPercent: prsPercent), i);
    }
}

public static partial class Prs
{
    /// <summary>
    /// Creates a stream hub of Price Relative Strength values without <see cref="PrsResult.PrsPercent"/>.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one provider holds produces no result, where the Series method
    /// throws. Drive both providers from one thread at a time.
    /// </remarks>
    /// <param name="sourceEval">Evaluated chain provider.</param>
    /// <param name="sourceBase">Base chain provider.</param>
    /// <returns>A stream hub of PRS results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    public static PrsHub ToPrsHub(
        this IChainProvider<IReusable> sourceEval,
        IChainProvider<IReusable> sourceBase)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceBase);

        return PairHub.Build(sourceEval, sourceBase, static pair => new PrsHub(pair, 0));
    }

    /// <summary>
    /// Creates a stream hub of Price Relative Strength values.
    /// </summary>
    /// <remarks>
    /// A timestamp that only one provider holds produces no result, where the Series method
    /// throws. Drive both providers from one thread at a time.
    /// </remarks>
    /// <param name="sourceEval">Evaluated chain provider.</param>
    /// <param name="sourceBase">Base chain provider.</param>
    /// <param name="lookbackPeriods">The number of periods for the PRS% lookback calculation.</param>
    /// <returns>A stream hub of PRS results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the lookback periods are less than or equal to 0, or exceed what the
    /// provider's cache can hold.
    /// </exception>
    public static PrsHub ToPrsHub(
        this IChainProvider<IReusable> sourceEval,
        IChainProvider<IReusable> sourceBase,
        int lookbackPeriods)
    {
        ArgumentNullException.ThrowIfNull(sourceEval);
        ArgumentNullException.ThrowIfNull(sourceBase);
        Validate(lookbackPeriods);

        return PairHub.Build(sourceEval, sourceBase, pair => new PrsHub(pair, lookbackPeriods));
    }
}
