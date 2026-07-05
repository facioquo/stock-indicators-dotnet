namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for creating Bollinger Bands hubs.
/// </summary>
public class BollingerBandsHub
    : ChainHub<IReusable, BollingerBandsResult>, IBollingerBands
{
    internal BollingerBandsHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        double standardDeviations) : base(provider)
    {
        BollingerBands.Validate(lookbackPeriods, standardDeviations);
        LookbackPeriods = lookbackPeriods;
        StandardDeviations = standardDeviations;
        Name = $"BB({lookbackPeriods},{standardDeviations})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double StandardDeviations { get; }
    /// <inheritdoc/>
    protected override (BollingerBandsResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // candidate result using Increment method like SMA
        BollingerBandsResult r = BollingerBands.Increment(
            ProviderCache,
            LookbackPeriods,
            StandardDeviations,
            i);

        return (r, i);
    }

}

public static partial class BollingerBands
{
    /// <summary>
    /// Converts the chain provider to a Bollinger Bands hub.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="standardDeviations">Number of standard deviations.</param>
    /// <returns>A Bollinger Bands hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the parameters are invalid.</exception>
    public static BollingerBandsHub ToBollingerBandsHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 20,
        double standardDeviations = 2)
             => new(chainProvider, lookbackPeriods, standardDeviations);
}
