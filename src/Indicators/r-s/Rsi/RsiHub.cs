namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Relative Strength Index (RSI).
/// </summary>
/// <remarks>
/// This implementation uses O(1) incremental updates per bar via Wilder's smoothing.
/// State restoration after rollback is properly handled in RollbackState() to maintain O(1) complexity
/// for normal streaming while allowing O(n) rebuild when needed.
/// </remarks>
public class RsiHub
    : ChainHub<IReusable, RsiResult>, IRsi
{
    private double _avgGain = double.NaN;
    private double _avgLoss = double.NaN;

    // recent state snapshots for O(1) near-tail rollback
    private readonly RollbackRing<(double AvgGain, double AvgLoss)> _rollback = new();

    internal RsiHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods) : base(provider)
    {
        Rsi.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;
        Name = $"RSI({lookbackPeriods})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(lookbackPeriods * 2, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    /// <remarks>
    /// O(1) complexity per bar using Wilder's smoothing for incremental updates.
    /// The initial SMA calculation at lookback period is O(k) where k = lookbackPeriods.
    /// </remarks>
    protected override (RsiResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double? rsi = null;
        double currentValue = item.Value;

        // Get previous value for gain/loss calculation
        double prevValue = i > 0 ? ProviderCache[i - 1].Value : double.NaN;

        // Calculate current gain/loss - O(1)
        double gain;
        double loss;
        if (!double.IsNaN(currentValue) && !double.IsNaN(prevValue))
        {
            gain = currentValue > prevValue ? currentValue - prevValue : 0;
            loss = currentValue < prevValue ? prevValue - currentValue : 0;
        }
        else
        {
            gain = loss = double.NaN;
        }

        // Re/initialize average gain/loss when needed - O(k)
        // This happens at first calculable position OR when state is NaN
        if (i >= LookbackPeriods && (double.IsNaN(_avgGain) || double.IsNaN(_avgLoss)))
        {
            (double sumGain, double sumLoss) = CalculateInitialSums(i);

            _avgGain = sumGain / LookbackPeriods;
            _avgLoss = sumLoss / LookbackPeriods;

            rsi = CalculateRsi(_avgGain, _avgLoss);
        }
        // Incremental update using Wilder's smoothing - O(1)
        else if (i > LookbackPeriods)
        {
            if (!double.IsNaN(gain) && !double.IsNaN(_avgGain) && !double.IsNaN(_avgLoss))
            {
                // Wilder's smoothing: avgGain = ((prevAvgGain * (n-1)) + currentGain) / n
                _avgGain = ((_avgGain * (LookbackPeriods - 1)) + gain) / LookbackPeriods;
                _avgLoss = ((_avgLoss * (LookbackPeriods - 1)) + loss) / LookbackPeriods;

                rsi = CalculateRsi(_avgGain, _avgLoss);
            }
            else if (double.IsNaN(gain))
            {
                // NaN input contaminates state
                _avgGain = double.NaN;
                _avgLoss = double.NaN;
            }
            // else: state is NaN, will be re-initialized when valid data appears
        }

        // snapshot state for O(1) near-tail rollback
        _rollback.Add(item.Timestamp, (_avgGain, _avgLoss));

        // candidate result
        RsiResult r = new(
            Timestamp: item.Timestamp,
            Rsi: rsi);

        return (r, i);
    }

    /// <summary>
    /// Restores the RSI state (avgGain, avgLoss) up to the specified timestamp.
    /// Called during Add/Remove operations and explicit Rebuild() calls.
    /// </summary>
    /// <remarks>
    /// This method rebuilds the state from the FIRST calculable position (LookbackPeriods)
    /// up to the position just before the rollback timestamp. This ensures that when
    /// ToIndicator is subsequently called during replay, the state is correct for
    /// incremental updates.
    /// </remarks>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset state
        _avgGain = double.NaN;
        _avgLoss = double.NaN;

        // Not enough data to initialize state
        if (restoreIndex < LookbackPeriods)
        {
            return;
        }

        // O(1) fast path: restore the exact state snapshot recorded when the
        // restore-point item was processed (near-tail rollbacks, the common
        // case for live corrections and forming-bar updates)
        if (_rollback.TryGet(
            ProviderCache[restoreIndex].Timestamp,
            out (double AvgGain, double AvgLoss) snapshot))
        {
            _avgGain = snapshot.AvgGain;
            _avgLoss = snapshot.AvgLoss;
            return;
        }

        // Calculate initial averages at first calculable position (index = LookbackPeriods)
        (double sumGain, double sumLoss) = CalculateInitialSums(LookbackPeriods);
        _avgGain = sumGain / LookbackPeriods;
        _avgLoss = sumLoss / LookbackPeriods;

        // Apply Wilder's smoothing for subsequent positions up to restoreIndex
        for (int p = LookbackPeriods + 1; p <= restoreIndex; p++)
        {
            double pPrevVal = ProviderCache[p - 1].Value;
            double pCurrVal = ProviderCache[p].Value;

            if (!double.IsNaN(pCurrVal) && !double.IsNaN(pPrevVal))
            {
                double pGain = pCurrVal > pPrevVal ? pCurrVal - pPrevVal : 0;
                double pLoss = pCurrVal < pPrevVal ? pPrevVal - pCurrVal : 0;

                _avgGain = ((_avgGain * (LookbackPeriods - 1)) + pGain) / LookbackPeriods;
                _avgLoss = ((_avgLoss * (LookbackPeriods - 1)) + pLoss) / LookbackPeriods;
            }
            else
            {
                // NaN contaminates state - will be re-initialized in ToIndicator
                _avgGain = double.NaN;
                _avgLoss = double.NaN;
            }
        }
    }

    // Calculates the initial sum of gains and losses over the lookback period.
    private (double sumGain, double sumLoss) CalculateInitialSums(int endIndex)
    {
        double sumGain = 0;
        double sumLoss = 0;

        for (int p = endIndex - LookbackPeriods + 1; p <= endIndex; p++)
        {
            double pPrevVal = ProviderCache[p - 1].Value;
            double pCurrVal = ProviderCache[p].Value;

            if (!double.IsNaN(pCurrVal) && !double.IsNaN(pPrevVal))
            {
                sumGain += pCurrVal > pPrevVal ? pCurrVal - pPrevVal : 0;
                sumLoss += pCurrVal < pPrevVal ? pPrevVal - pCurrVal : 0;
            }
            else
            {
                // NaN contaminates the sum - exit early
                return (double.NaN, double.NaN);
            }
        }

        return (sumGain, sumLoss);
    }

    // Calculates RSI from average gain and loss.
    // Matches Series parity by checking division result for NaN.
    private static double? CalculateRsi(double avgGain, double avgLoss)
    {
        // Check if division would produce NaN (e.g., 0/0 case)
        // This matches Series parity which checks !double.IsNaN(avgGain / avgLoss)
        if (double.IsNaN(avgGain / avgLoss))
        {
            return null;
        }

        if (avgLoss > 0)
        {
            double rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        return 100;
    }
}

public static partial class Rsi
{
    /// <summary>
    /// Creates an RSI streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An RSI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are invalid.</exception>
    public static RsiHub ToRsiHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 14)
        => new(chainProvider, lookbackPeriods);
}
