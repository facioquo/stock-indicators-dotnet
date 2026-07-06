using System.Globalization;

namespace FacioQuo.Stock.Indicators;

#pragma warning disable IDE0072 // Missing cases in switch statement

/// <summary>
/// Streaming hub for Moving Average Envelopes.
/// </summary>
public static partial class MaEnvelopes
{
    /// <summary>
    /// Creates a Moving Average Envelopes streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Number of periods for the moving average.</param>
    /// <param name="percentOffset">Percentage offset for the envelopes. Default is 2.5.</param>
    /// <param name="movingAverageType">Type of moving average to use. Default is SMA.</param>
    /// <returns>A Moving Average Envelopes hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid or the moving average type is not supported.</exception>
    public static MaEnvelopesHub ToMaEnvelopesHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods,
        double percentOffset = 2.5,
        MaType movingAverageType = MaType.SMA)
        => new(chainProvider, lookbackPeriods, percentOffset, movingAverageType);
}

/// <summary>
/// Streaming hub for Moving Average Envelopes calculations.
/// </summary>
public class MaEnvelopesHub
    : StreamHub<IReusable, MaEnvelopeResult>
{
    private readonly double offsetRatio;
    private readonly MaType movingAverageType;
    /// <summary>
    /// for EMA-based types
    /// </summary>
    private readonly double k;
    private readonly int lookbackPeriods;
    /// <summary>
    /// rolling window of raw values, maintained only for the per-tick SMA centerline
    /// </summary>
    private readonly Queue<double>? smaBuffer;

    /// <summary>
    /// for DEMA
    /// </summary>
    private double lastEma1 = double.NaN;
    /// <summary>
    /// for DEMA
    /// </summary>
    private double lastEma2 = double.NaN;
    /// <summary>
    /// for TEMA
    /// </summary>
    private double lastEma3 = double.NaN;
    /// <summary>
    /// for TEMA
    /// </summary>
    private double lastEma4 = double.NaN;
    /// <summary>
    /// for TEMA
    /// </summary>
    private double lastEma5 = double.NaN;

    internal MaEnvelopesHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        double percentOffset,
        MaType movingAverageType) : base(provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        MaEnvelopes.Validate(percentOffset);

        // Validate lookbackPeriods based on MA type
        switch (movingAverageType)
        {
            // TODO: implement remaining moving average types

            case MaType.ALMA:
                throw new NotImplementedException(
                    "ALMA-based Moving Average Envelopes are not yet supported in streaming mode. " +
                    "Use Series-style calculation instead.");
            case MaType.DEMA:
                Dema.Validate(lookbackPeriods);
                break;
            case MaType.EMA:
                Ema.Validate(lookbackPeriods);
                break;
            case MaType.EPMA:
                throw new NotImplementedException(
                    "EPMA-based Moving Average Envelopes are not yet supported in streaming mode. " +
                    "Use Series-style calculation instead.");
            case MaType.HMA:
                throw new NotImplementedException(
                    "HMA-based Moving Average Envelopes are not yet supported in streaming mode. " +
                    "Use Series-style calculation instead.");
            case MaType.SMA:
                Sma.Validate(lookbackPeriods);
                break;
            case MaType.SMMA:
                Smma.Validate(lookbackPeriods);
                break;
            case MaType.TEMA:
                Tema.Validate(lookbackPeriods);
                break;
            case MaType.WMA:
                Wma.Validate(lookbackPeriods);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(movingAverageType), movingAverageType,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Moving Average Envelopes does not support {0}.",
                        Enum.GetName(movingAverageType)));
        }

        this.lookbackPeriods = lookbackPeriods;
        LookbackPeriods = lookbackPeriods;
        PercentOffset = percentOffset;
        MovingAverageType = movingAverageType;

        this.movingAverageType = movingAverageType;
        offsetRatio = percentOffset / 100d;
        k = 2d / (lookbackPeriods + 1); // for EMA-based types
        smaBuffer = movingAverageType == MaType.SMA ? new Queue<double>(lookbackPeriods) : null;
        Name = $"MAENV({lookbackPeriods},{percentOffset},{Enum.GetName(movingAverageType)})";

        // Validate cache size for warmup requirements using type-specific multipliers:
        // TEMA needs 3x, DEMA needs 2x, all other types need 1x
        int warmupMultiplier = movingAverageType switch {
            MaType.TEMA => 3,
            MaType.DEMA => 2,
            _ => 1
        };
        ValidateCacheSize(lookbackPeriods * warmupMultiplier, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public double PercentOffset { get; init; }

    /// <inheritdoc/>
    public MaType MovingAverageType { get; init; }
    /// <inheritdoc/>
    protected override (MaEnvelopeResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // advance the rolling window of raw values (SMA centerline only)
        smaBuffer?.Update(lookbackPeriods, item.Value);

        // Calculate MA value based on type
        double? ma = movingAverageType switch {
            MaType.DEMA => CalculateDema(i),
            MaType.EMA => CalculateEma(i),
            MaType.SMA => CalculateSma(),
            MaType.SMMA => CalculateSmma(i),
            MaType.TEMA => CalculateTema(i),
            MaType.WMA => CalculateWma(i),
            _ => throw new ArgumentOutOfRangeException(nameof(item))
        };

        // Calculate envelopes
        double? upperEnvelope = ma.HasValue && !double.IsNaN(ma.Value) ? ma + (ma * offsetRatio) : null;
        double? lowerEnvelope = ma.HasValue && !double.IsNaN(ma.Value) ? ma - (ma * offsetRatio) : null;

        MaEnvelopeResult r = new(
            Timestamp: item.Timestamp,
            Centerline: ma,
            UpperEnvelope: upperEnvelope,
            LowerEnvelope: lowerEnvelope);

        return (r, i);
    }

    private double? CalculateSma()
        => smaBuffer!.Average(lookbackPeriods).NaN2Null();

    private double? CalculateEma(int index)
    {
        if (index < lookbackPeriods - 1)
        {
            return null;
        }

        double val = ProviderCache[index].Value;
        if (double.IsNaN(val))
        {
            return null;
        }

        double ema = index >= lookbackPeriods - 1
            ? Cache[index - 1].Centerline is not null
                // normal
                ? Ema.Increment(k, Cache[index - 1].Centerline!.Value, val)
                // re/initialize as SMA
                : Sma.Increment(ProviderCache, lookbackPeriods, index)
            // warmup periods are never calculable
            : double.NaN;

        return ema.NaN2Null();
    }

    private double? CalculateDema(int index)
    {
        if (index < lookbackPeriods - 1)
        {
            return null;
        }

        double val = ProviderCache[index].Value;
        if (double.IsNaN(val))
        {
            return null;
        }

        // if out-of-order change (insertion/deletion before current index) occurred
        // invalidate state and backfill from previous cached EMA layers
        if (index > 0 && Cache.Count > index && Cache[index - 1].Centerline is not null && double.IsNaN(lastEma1))
        {
            // Backfill from cache if available - for now, reinitialize
            lastEma1 = lastEma2 = double.NaN;
        }

        double dema = index >= lookbackPeriods - 1
            ? Cache[index - 1].Centerline is not null
                // normal
                ? CalculateDemaIncrement(val)
                // re/initialize as SMA
                : InitializeDema(index)
            // warmup periods are never calculable
            : double.NaN;

        return dema.NaN2Null();
    }

    private double InitializeDema(int index)
    {
        double sma = Sma.Increment(ProviderCache, lookbackPeriods, index);
        if (double.IsNaN(sma))
        {
            lastEma1 = lastEma2 = double.NaN;
            return double.NaN;
        }

        lastEma1 = lastEma2 = sma;
        return (2 * lastEma1) - lastEma2;
    }

    private double CalculateDemaIncrement(double value)
    {
        if (double.IsNaN(value))
        {
            return double.NaN;
        }

        lastEma1 = Ema.Increment(k, lastEma1, value);
        lastEma2 = Ema.Increment(k, lastEma2, lastEma1);
        return (2 * lastEma1) - lastEma2;
    }

    private double? CalculateTema(int index)
    {
        if (index < lookbackPeriods - 1)
        {
            return null;
        }

        double val = ProviderCache[index].Value;
        if (double.IsNaN(val))
        {
            return null;
        }

        double tema = index >= lookbackPeriods - 1
            ? Cache[index - 1].Centerline is not null
                // normal
                ? CalculateTemaIncrement(val)
                // re/initialize as SMA
                : InitializeTema(index)
            // warmup periods are never calculable
            : double.NaN;

        return tema.NaN2Null();
    }

    private double InitializeTema(int index)
    {
        double sma = Sma.Increment(ProviderCache, lookbackPeriods, index);
        if (double.IsNaN(sma))
        {
            lastEma3 = lastEma4 = lastEma5 = double.NaN;
            return double.NaN;
        }

        lastEma3 = lastEma4 = lastEma5 = sma;
        return (3 * lastEma3) - (3 * lastEma4) + lastEma5;
    }

    private double CalculateTemaIncrement(double value)
    {
        if (double.IsNaN(value))
        {
            return double.NaN;
        }

        lastEma3 = Ema.Increment(k, lastEma3, value);
        lastEma4 = Ema.Increment(k, lastEma4, lastEma3);
        lastEma5 = Ema.Increment(k, lastEma5, lastEma4);
        return (3 * lastEma3) - (3 * lastEma4) + lastEma5;
    }

    private double? CalculateSmma(int index)
    {
        if (index < lookbackPeriods - 1)
        {
            return null;
        }

        double val = ProviderCache[index].Value;
        if (double.IsNaN(val))
        {
            return null;
        }

        double smma = index >= lookbackPeriods - 1
            ? Cache[index - 1].Centerline is not null
                // normal
                ? ((Cache[index - 1].Centerline!.Value * (lookbackPeriods - 1)) + val) / lookbackPeriods
                // re/initialize as SMA
                : Sma.Increment(ProviderCache, lookbackPeriods, index)
            // warmup periods are never calculable
            : double.NaN;

        return smma.NaN2Null();
    }

    private double? CalculateWma(int index)
    {
        if (index < lookbackPeriods - 1)
        {
            return null;
        }

        double sumWeight = 0;
        double sumValue = 0;
        for (int p = index - lookbackPeriods + 1, weight = 1; p <= index; p++, weight++)
        {
            double val = ProviderCache[p].Value;
            if (double.IsNaN(val))
            {
                return null; // Not enough valid data yet
            }

            sumValue += val * weight;
            sumWeight += weight;
        }

        return sumValue / sumWeight;
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        if (restoreIndex >= lookbackPeriods)
        {
            // Reset state variables based on MA type
            if (movingAverageType == MaType.DEMA)
            {
                lastEma1 = lastEma2 = double.NaN;
            }
            else if (movingAverageType == MaType.TEMA)
            {
                lastEma3 = lastEma4 = lastEma5 = double.NaN;
            }
        }
        else
        {
            lastEma1 = lastEma2 = lastEma3 = lastEma4 = lastEma5 = double.NaN;
        }

        // rebuild the SMA rolling window from the last LookbackPeriods preserved
        // values; the item at restoreIndex is preserved (not replayed through
        // ToIndicator), so it is included here
        if (smaBuffer is not null)
        {
            smaBuffer.Clear();

            if (restoreIndex >= 0)
            {
                int startIdx = Math.Max(0, restoreIndex + 1 - lookbackPeriods);

                for (int p = startIdx; p <= restoreIndex; p++)
                {
                    smaBuffer.Update(lookbackPeriods, ProviderCache[p].Value);
                }
            }
        }
    }
}
