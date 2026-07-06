namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the True Strength Index (TSI) indicator.
/// </summary>
public class TsiHub
    : ChainHub<IReusable, TsiResult>, ITsi
{
    private readonly double mult1;  // smoothing constant for first EMA (lookbackPeriods)
    private readonly double mult2;  // smoothing constant for second EMA (smoothPeriods)
    private readonly double multS;  // smoothing constant for signal EMA (signalPeriods)

    // State variables for incremental calculation
    private bool _isFirstPeriod;
    private double _prevValue;
    private double _prevCs1;
    private double _prevAs1;
    private double _prevCs2;
    private double _prevAs2;
    private double _prevSignal;

    // History lists for second smoothing initialization
    private readonly List<double> _cs1History;
    private readonly List<double> _as1History;

    internal TsiHub(
        IChainProvider<IReusable> provider,
        int lookbackPeriods,
        int smoothPeriods,
        int signalPeriods) : base(provider)
    {
        Tsi.Validate(lookbackPeriods, smoothPeriods, signalPeriods);
        LookbackPeriods = lookbackPeriods;
        SmoothPeriods = smoothPeriods;
        SignalPeriods = signalPeriods;

        mult1 = 2d / (lookbackPeriods + 1);
        mult2 = 2d / (smoothPeriods + 1);
        multS = 2d / (signalPeriods + 1);

        Name = $"TSI({lookbackPeriods},{smoothPeriods},{signalPeriods})";

        _isFirstPeriod = true;
        _prevValue = double.NaN;
        _prevCs1 = double.NaN;
        _prevAs1 = double.NaN;
        _prevCs2 = double.NaN;
        _prevAs2 = double.NaN;
        _prevSignal = double.NaN;

        _cs1History = [];
        _as1History = [];

        // Validate cache size for warmup requirements
        // First valid TSI signal at lookbackPeriods + smoothPeriods + signalPeriods - 1
        // due to sequential EMA of EMA (cs1 then cs2) followed by signal EMA.
        int requiredWarmup = lookbackPeriods + smoothPeriods + signalPeriods - 1;
        ValidateCacheSize(requiredWarmup, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc/>
    public int SmoothPeriods { get; init; }

    /// <inheritdoc/>
    public int SignalPeriods { get; init; }
    /// <inheritdoc/>
    protected override (TsiResult result, int index)
        ToIndicator(IReusable item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);
        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        double currentValue = item.Value;

        // Handle first period
        if (_isFirstPeriod)
        {
            _prevValue = currentValue;
            _isFirstPeriod = false;
            _cs1History.Add(double.NaN);
            _as1History.Add(double.NaN);
            return (new TsiResult(item.Timestamp), i);
        }

        // Price change
        double change = currentValue - _prevValue;
        double absChange = Math.Abs(change);
        _prevValue = currentValue;

        // Calculate first smoothing (EMA of price change)
        double cs1;
        double as1;

        // re/initialize first smoothing
        if (double.IsNaN(_prevCs1) && i >= LookbackPeriods)
        {
            // Initialize first smoothing with SMA
            double sumC = 0;
            double sumA = 0;

            for (int p = i - LookbackPeriods + 1; p <= i; p++)
            {
                double pValue = ProviderCache[p].Value;
                double pPrevValue = ProviderCache[p - 1].Value;
                double pChange = pValue - pPrevValue;
                sumC += pChange;
                sumA += Math.Abs(pChange);
            }

            cs1 = sumC / LookbackPeriods;
            as1 = sumA / LookbackPeriods;
            _prevCs1 = cs1;
            _prevAs1 = as1;
        }
        // normal first smoothing
        else if (!double.IsNaN(_prevCs1))
        {
            cs1 = ((change - _prevCs1) * mult1) + _prevCs1;
            as1 = ((absChange - _prevAs1) * mult1) + _prevAs1;
            _prevCs1 = cs1;
            _prevAs1 = as1;
        }
        else
        {
            cs1 = double.NaN;
            as1 = double.NaN;
        }

        // Store in history for second smoothing initialization
        _cs1History.Add(cs1);
        _as1History.Add(as1);

        // Calculate second smoothing (EMA of first EMA)
        double cs2;
        double as2;

        // re/initialize second smoothing
        if (double.IsNaN(_prevCs2) && i >= SmoothPeriods && !double.IsNaN(cs1))
        {
            // Initialize second smoothing with SMA from history
            double sumCs = 0;
            double sumAs = 0;

            for (int p = i - SmoothPeriods + 1; p <= i; p++)
            {
                sumCs += _cs1History[p];
                sumAs += _as1History[p];
            }

            cs2 = sumCs / SmoothPeriods;
            as2 = sumAs / SmoothPeriods;
            _prevCs2 = cs2;
            _prevAs2 = as2;
        }
        // normal second smoothing
        else if (!double.IsNaN(_prevCs2) && !double.IsNaN(cs1))
        {
            cs2 = ((cs1 - _prevCs2) * mult2) + _prevCs2;
            as2 = ((as1 - _prevAs2) * mult2) + _prevAs2;
            _prevCs2 = cs2;
            _prevAs2 = as2;
        }
        else
        {
            cs2 = double.NaN;
            as2 = double.NaN;
        }

        // Calculate TSI
        double tsi = as2 != 0
            ? 100d * (cs2 / as2)
            : double.NaN;

        // Calculate signal line
        double signal = CalculateSignal(i, tsi);

        // Candidate result
        TsiResult r = new(
            Timestamp: item.Timestamp,
            Tsi: tsi.NaN2Null(),
            Signal: signal.NaN2Null());

        return (r, i);
    }

    private double CalculateSignal(int index, double tsi)
    {
        if (SignalPeriods > 1)
        {
            // re/initialize signal
            if (double.IsNaN(_prevSignal) && index > SignalPeriods)
            {
                double sum = tsi;
                for (int p = index - SignalPeriods + 1; p < index; p++)
                {
                    sum += Cache[p].Tsi.Null2NaN();
                }

                _prevSignal = sum / SignalPeriods;
                return _prevSignal;
            }
            // normal signal
            else if (!double.IsNaN(_prevSignal) && !double.IsNaN(tsi))
            {
                _prevSignal = ((tsi - _prevSignal) * multS) + _prevSignal;
                return _prevSignal;
            }
        }
        else if (SignalPeriods == 1)
        {
            return tsi;
        }

        return double.NaN;
    }

    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // Reset all state
        _isFirstPeriod = true;
        _prevValue = double.NaN;
        _prevCs1 = double.NaN;
        _prevAs1 = double.NaN;
        _prevCs2 = double.NaN;
        _prevAs2 = double.NaN;
        _prevSignal = double.NaN;

        _cs1History.Clear();
        _as1History.Clear();

        if (restoreIndex < 0)
        {
            return;
        }

        for (int i = 0; i <= restoreIndex; i++)
        {
            IReusable item = ProviderCache[i];
            double currentValue = item.Value;

            // Handle first period
            if (_isFirstPeriod)
            {
                _prevValue = currentValue;
                _isFirstPeriod = false;
                _cs1History.Add(double.NaN);
                _as1History.Add(double.NaN);
                continue;
            }

            // Price change
            double change = currentValue - _prevValue;
            double absChange = Math.Abs(change);
            _prevValue = currentValue;

            // Calculate first smoothing (EMA of price change)
            double cs1;
            double as1;

            // re/initialize first smoothing
            if (double.IsNaN(_prevCs1) && i >= LookbackPeriods)
            {
                double sumC = 0;
                double sumA = 0;

                for (int p = i - LookbackPeriods + 1; p <= i; p++)
                {
                    double pValue = ProviderCache[p].Value;
                    double pPrevValue = ProviderCache[p - 1].Value;
                    double pChange = pValue - pPrevValue;
                    sumC += pChange;
                    sumA += Math.Abs(pChange);
                }

                cs1 = sumC / LookbackPeriods;
                as1 = sumA / LookbackPeriods;
                _prevCs1 = cs1;
                _prevAs1 = as1;
            }
            // normal first smoothing
            else if (!double.IsNaN(_prevCs1))
            {
                cs1 = ((change - _prevCs1) * mult1) + _prevCs1;
                as1 = ((absChange - _prevAs1) * mult1) + _prevAs1;
                _prevCs1 = cs1;
                _prevAs1 = as1;
            }
            else
            {
                cs1 = double.NaN;
                as1 = double.NaN;
            }

            // Store in history
            _cs1History.Add(cs1);
            _as1History.Add(as1);

            // Calculate second smoothing (EMA of first EMA)
            double cs2;
            double as2;

            // re/initialize second smoothing
            if (double.IsNaN(_prevCs2) && i >= SmoothPeriods && !double.IsNaN(cs1))
            {
                // Initialize second smoothing with SMA from history
                double sumCs = 0;
                double sumAs = 0;

                for (int p = i - SmoothPeriods + 1; p <= i; p++)
                {
                    sumCs += _cs1History[p];
                    sumAs += _as1History[p];
                }

                cs2 = sumCs / SmoothPeriods;
                as2 = sumAs / SmoothPeriods;
                _prevCs2 = cs2;
                _prevAs2 = as2;
            }
            // normal second smoothing
            else if (!double.IsNaN(_prevCs2) && !double.IsNaN(cs1))
            {
                cs2 = ((cs1 - _prevCs2) * mult2) + _prevCs2;
                as2 = ((as1 - _prevAs2) * mult2) + _prevAs2;
                _prevCs2 = cs2;
                _prevAs2 = as2;
            }
            else
            {
                cs2 = double.NaN;
                as2 = double.NaN;
            }

            // Calculate TSI (needed for signal calculation)
            double tsi = as2 != 0
                ? 100d * (cs2 / as2)
                : double.NaN;

            // Calculate signal line (need to rebuild for state restoration)
            if (SignalPeriods > 1)
            {
                if (double.IsNaN(_prevSignal) && i > SignalPeriods && !double.IsNaN(tsi))
                {
                    // We need to look back in Cache to get previous TSI values
                    double sum = tsi;
                    for (int p = i - SignalPeriods + 1; p < i; p++)
                    {
                        if (p >= 0 && p < Cache.Count)
                        {
                            sum += Cache[p].Tsi.Null2NaN();
                        }
                    }

                    _prevSignal = sum / SignalPeriods;
                }
                else if (!double.IsNaN(_prevSignal) && !double.IsNaN(tsi))
                {
                    _prevSignal = ((tsi - _prevSignal) * multS) + _prevSignal;
                }
            }
            else if (SignalPeriods == 1)
            {
                _prevSignal = tsi;
            }
        }
    }
}

public static partial class Tsi
{
    /// <summary>
    /// Creates a TSI streaming hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <param name="lookbackPeriods">Number of periods for the lookback calculation.</param>
    /// <param name="smoothPeriods">Number of periods for the smoothing calculation.</param>
    /// <param name="signalPeriods">Number of periods for the signal calculation.</param>
    /// <returns>A TSI hub.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the chain provider is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any of the parameters are invalid.</exception>
    public static TsiHub ToTsiHub(
        this IChainProvider<IReusable> chainProvider,
        int lookbackPeriods = 25,
        int smoothPeriods = 13,
        int signalPeriods = 7)
        => new(chainProvider, lookbackPeriods, smoothPeriods, signalPeriods);
}
