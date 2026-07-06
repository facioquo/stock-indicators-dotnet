namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Arnaud Legoux Moving Average (ALMA).
/// </summary>
public class AlmaHub
    : ChainHub<IReusable, AlmaResult>, IAlma
{
    private readonly double[] weights;
    private readonly double normalizationFactor;

    internal AlmaHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        double offset,
        double sigma) : base(provider)
    {
        Alma.Validate(lookbackPeriods, offset, sigma);
        LookbackPeriods = lookbackPeriods;
        Offset = offset;
        Sigma = sigma;
        Name = $"ALMA({lookbackPeriods},{offset},{sigma})";

        // Pre-calculate weights once for O(1) application per bar
        double m = offset * (lookbackPeriods - 1);
        double s = lookbackPeriods / sigma;

        weights = new double[lookbackPeriods];
        double norm = 0;

        for (int i = 0; i < lookbackPeriods; i++)
        {
            double wt = Math.Exp(-((i - m) * (i - m)) / (2 * s * s));
            weights[i] = wt;
            norm += wt;
        }

        normalizationFactor = norm;

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double Offset { get; init; }

    /// <inheritdoc/>
    public double Sigma { get; init; }
    /// <inheritdoc/>
    protected override (AlmaResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // Calculate ALMA efficiently using pre-calculated weights and a rolling window
        // This is O(lookbackPeriods) which is constant for a given configuration
        // Weight calculation is done once in constructor, not on every bar
        double? alma = null;

        if (i >= LookbackPeriods - 1)
        {
            double weightedSum = 0;
            bool hasNaN = false;

            for (int p = 0; p < LookbackPeriods; p++)
            {
                int sourceIndex = i - LookbackPeriods + 1 + p;
                double value = ProviderCache[sourceIndex].Value;

                if (double.IsNaN(value))
                {
                    hasNaN = true;
                    break;
                }

                weightedSum += weights[p] * value;
            }

            alma = hasNaN ? null : weightedSum / normalizationFactor;
        }

        // candidate result
        AlmaResult r = new(
            Timestamp: item.Timestamp,
            Alma: alma);

        return (r, i);
    }
}

public static partial class Alma
{
    /// <summary>
    /// Creates an ALMA streaming hub with a chain provider source.
    /// </summary>
    /// <remarks>If providers contain historical data, this hub will fast-forward its cache.</remarks>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="offset">Offset for the ALMA calculation. Default is 0.85.</param>
    /// <param name="sigma">Sigma for the ALMA calculation. Default is 6.</param>
    /// <returns>A chain-sourced instance of <see cref="AlmaHub"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="chainProvider"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static AlmaHub ToAlmaHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 9,
        double offset = 0.85,
        double sigma = 6)
        => new(chainProvider, lookbackPeriods, offset, sigma);
}
