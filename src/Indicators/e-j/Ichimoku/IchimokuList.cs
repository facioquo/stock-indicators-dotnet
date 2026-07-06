namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Calculates Ichimoku Cloud indicator from incremental bar values with buffered state management.
/// </summary>
public class IchimokuList : BufferList<IchimokuResult>, IIncrementFromBar, IIchimoku
{
    // Historical results buffer for lookback (needed for offset calculations)
    private readonly List<(DateTime Timestamp, double? TenkanSen, double? KijunSen)> _historicalResults;

    // Historical high/low buffer for Senkou Span B lookback (need to look back by offset)
    private readonly List<(double High, double Low)> _historicalHighLow;

    // Rolling window buffers for max/min calculations
    private readonly Queue<(double High, double Low)> _tenkanBuffer;
    private readonly Queue<(double High, double Low)> _kijunBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="IchimokuList"/> class.
    /// </summary>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="senkouOffset">Number of periods for the Senkou offset.</param>
    /// <param name="chikouOffset">Number of periods for the Chikou offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="chikouOffset"/> is invalid.</exception>
    public IchimokuList(
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52,
        int senkouOffset = 26,
        int chikouOffset = 26)
    {
        Ichimoku.Validate(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

        TenkanPeriods = tenkanPeriods;
        KijunPeriods = kijunPeriods;
        SenkouBPeriods = senkouBPeriods;
        SenkouOffset = senkouOffset;
        ChikouOffset = chikouOffset;

        // Initialize buffers
        _historicalResults = new List<(DateTime, double?, double?)>();
        _historicalHighLow = new List<(double, double)>();
        _tenkanBuffer = new Queue<(double, double)>(tenkanPeriods);
        _kijunBuffer = new Queue<(double, double)>(kijunPeriods);

        Name = $"ICHIMOKU({tenkanPeriods}, {kijunPeriods}, {senkouBPeriods}, {senkouOffset}, {chikouOffset})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IchimokuList"/> class with initial bars.
    /// </summary>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="senkouOffset">Number of periods for the Senkou offset.</param>
    /// <param name="chikouOffset">Number of periods for the Chikou offset.</param>
    /// <param name="bars">Initial bars to populate the list.</param>
    public IchimokuList(
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int senkouOffset,
        int chikouOffset,
        IReadOnlyList<IBar> bars)
        : this(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset) => Add(bars);

    /// <inheritdoc/>
    public int TenkanPeriods { get; init; }

    /// <inheritdoc/>
    public int KijunPeriods { get; init; }

    /// <inheritdoc/>
    public int SenkouBPeriods { get; init; }

    /// <inheritdoc/>
    public int SenkouOffset { get; init; }

    /// <inheritdoc/>
    public int ChikouOffset { get; init; }

    /// <summary>
    /// Adds a new bar to the Ichimoku list.
    /// </summary>
    /// <param name="bar">Bar to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bar is null.</exception>
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        // Update rolling buffers using BufferUtilities
        _tenkanBuffer.Update(TenkanPeriods, ((double)bar.High, (double)bar.Low));
        _kijunBuffer.Update(KijunPeriods, ((double)bar.High, (double)bar.Low));

        // Store historical high/low for Senkou B lookback
        _historicalHighLow.Add(((double)bar.High, (double)bar.Low));

        // Calculate Tenkan-sen (conversion line)
        double? tenkanSen = null;
        if (_tenkanBuffer.Count == TenkanPeriods)
        {
            tenkanSen = CalculateMidpoint(_tenkanBuffer);
        }

        // Calculate Kijun-sen (base line)
        double? kijunSen = null;
        if (_kijunBuffer.Count == KijunPeriods)
        {
            kijunSen = CalculateMidpoint(_kijunBuffer);
        }

        // Store historical Tenkan and Kijun for offset lookback
        _historicalResults.Add((bar.Timestamp, tenkanSen, kijunSen));

        // Calculate Senkou Span A (leading span A)
        double? senkouSpanA = null;
        int senkouStartPeriod = Math.Max(
            2 * SenkouOffset,
            Math.Max(TenkanPeriods, KijunPeriods)) - 1;

        if (Count >= senkouStartPeriod)
        {
            if (SenkouOffset == 0)
            {
                senkouSpanA = (tenkanSen + kijunSen) / 2d;
            }
            else if (Count >= SenkouOffset)
            {
                // Look back to get historical Tenkan and Kijun values
                int lookbackIndex = Count - SenkouOffset;
                if (lookbackIndex >= 0 && lookbackIndex < _historicalResults.Count)
                {
                    (DateTime _, double? historicalTenkan, double? historicalKijun) = _historicalResults[lookbackIndex];
                    senkouSpanA = (historicalTenkan + historicalKijun) / 2d;
                }
            }
        }

        // Calculate Senkou Span B (leading span B)
        double? senkouSpanB = CalculateSenkouSpanB(Count);

        // Calculate Chikou Span (lagging span)
        // ChikouSpan for the current result will be null (can't see future in streaming)
        // Create and emit the result immediately
        IchimokuResult result = new(
            Timestamp: bar.Timestamp,
            TenkanSen: tenkanSen,
            KijunSen: kijunSen,
            SenkouSpanA: senkouSpanA,
            SenkouSpanB: senkouSpanB,
            ChikouSpan: null);

        AddInternal(result);

        // Update ChikouSpan for past result (retroactive update)
        // The result ChikouOffset periods back should have its ChikouSpan set to the current close
        if (Count > ChikouOffset)
        {
            int pastIndex = Count - ChikouOffset - 1;
            IchimokuResult pastResult = this[pastIndex];
            IchimokuResult updatedResult = pastResult with { ChikouSpan = (double)bar.Close };
            UpdateInternal(pastIndex, updatedResult);
        }
    }

    /// <summary>
    /// Adds a list of bars to the Ichimoku list.
    /// </summary>
    /// <param name="bars">List of bars to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when the bars list is null.</exception>
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        // Pre-populate the future close buffer with all closes for Chikou Span calculation
        // This allows us to look ahead for Chikou values during batch processing
        List<double> allCloses = new(bars.Count);
        for (int i = 0; i < bars.Count; i++)
        {
            allCloses.Add((double)bars[i].Close);
        }

        for (int i = 0; i < bars.Count; i++)
        {
            IBar bar = bars[i];

            // Update rolling buffers using BufferUtilities
            _tenkanBuffer.Update(TenkanPeriods, ((double)bar.High, (double)bar.Low));
            _kijunBuffer.Update(KijunPeriods, ((double)bar.High, (double)bar.Low));

            // Store historical high/low for Senkou B lookback
            _historicalHighLow.Add(((double)bar.High, (double)bar.Low));

            // Calculate Tenkan-sen (conversion line)
            double? tenkanSen = null;
            if (_tenkanBuffer.Count == TenkanPeriods)
            {
                tenkanSen = CalculateMidpoint(_tenkanBuffer);
            }

            // Calculate Kijun-sen (base line)
            double? kijunSen = null;
            if (_kijunBuffer.Count == KijunPeriods)
            {
                kijunSen = CalculateMidpoint(_kijunBuffer);
            }

            // Store historical Tenkan and Kijun for offset lookback
            _historicalResults.Add((bar.Timestamp, tenkanSen, kijunSen));

            // Calculate Senkou Span A (leading span A)
            double? senkouSpanA = null;
            int senkouStartPeriod = Math.Max(
                2 * SenkouOffset,
                Math.Max(TenkanPeriods, KijunPeriods)) - 1;

            if (Count >= senkouStartPeriod)
            {
                if (SenkouOffset == 0)
                {
                    senkouSpanA = (tenkanSen + kijunSen) / 2d;
                }
                else if (Count >= SenkouOffset)
                {
                    // Look back to get historical Tenkan and Kijun values
                    int lookbackIndex = Count - SenkouOffset;
                    if (lookbackIndex >= 0 && lookbackIndex < _historicalResults.Count)
                    {
                        (DateTime _, double? historicalTenkan, double? historicalKijun) = _historicalResults[lookbackIndex];
                        senkouSpanA = (historicalTenkan + historicalKijun) / 2d;
                    }
                }
            }

            // Calculate Senkou Span B (leading span B)
            double? senkouSpanB = CalculateSenkouSpanB(Count);

            // Calculate Chikou Span (lagging span) - look ahead in the pre-populated list
            double? chikouSpan = null;
            int futureIndex = i + ChikouOffset;
            if (futureIndex < allCloses.Count)
            {
                chikouSpan = allCloses[futureIndex];
            }

            // Create and add the result
            IchimokuResult result = new(
                Timestamp: bar.Timestamp,
                TenkanSen: tenkanSen,
                KijunSen: kijunSen,
                SenkouSpanA: senkouSpanA,
                SenkouSpanB: senkouSpanB,
                ChikouSpan: chikouSpan);

            AddInternal(result);
        }
    }

    /// <summary>
    /// Clears the list and resets internal buffers so the instance can be reused.
    /// </summary>
    public override void Clear()
    {
        base.Clear();

        _historicalResults.Clear();
        _historicalHighLow.Clear();
        _tenkanBuffer.Clear();
        _kijunBuffer.Clear();
    }
    /// <summary>
    /// Calculates the midpoint (average of high and low) for a rolling window.
    /// </summary>
    /// <param name="buffer">Buffer containing high/low tuples.</param>
    /// <returns>Midpoint value.</returns>
    private static double CalculateMidpoint(Queue<(double High, double Low)> buffer)
    {
        double max = double.MinValue;
        double min = double.MaxValue;

        foreach ((double high, double low) in buffer)
        {
            if (high > max)
            {
                max = high;
            }

            if (low < min)
            {
                min = low;
            }
        }

        return (max + min) / 2d;
    }

    /// <summary>
    /// Calculates the Senkou Span B value for the current position.
    /// Uses historical data from range [currentIndex - senkouOffset - senkouBPeriods + 1, currentIndex - senkouOffset].
    /// </summary>
    /// <param name="currentIndex">Current position in the results list.</param>
    /// <returns>Senkou Span B value, or null if insufficient data.</returns>
    private double? CalculateSenkouSpanB(int currentIndex)
    {
        if (currentIndex < SenkouOffset + SenkouBPeriods - 1)
        {
            return null;
        }

        int endIndex = currentIndex - SenkouOffset;
        int startIndex = endIndex - SenkouBPeriods + 1;

        if (startIndex < 0 || endIndex >= _historicalHighLow.Count)
        {
            return null;
        }

        double max = double.MinValue;
        double min = double.MaxValue;

        for (int p = startIndex; p <= endIndex; p++)
        {
            (double high, double low) = _historicalHighLow[p];
            if (high > max)
            {
                max = high;
            }

            if (low < min)
            {
                min = low;
            }
        }

        return (max + min) / 2d;
    }

    /// <summary>
    /// Removes oldest results from both the outer list and the nested historical buffers
    /// when the list exceeds <see cref="BufferList{TResult}.MaxListSize"/>.
    /// This prevents unbounded growth of the auxiliary historical caches.
    /// </summary>
    protected override void PruneList()
    {
        // Keep the historical buffers in sync with the main list
        int itemsToRemove = Count - MaxListSize;
        if (itemsToRemove > 0)
        {
            if (_historicalResults.Count > itemsToRemove)
            {
                _historicalResults.RemoveRange(0, itemsToRemove);
            }

            if (_historicalHighLow.Count > itemsToRemove)
            {
                _historicalHighLow.RemoveRange(0, itemsToRemove);
            }
        }

        // Call base implementation to prune the outer result list
        base.PruneList();
    }
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class Ichimoku
{
    /// <summary>
    /// Creates a buffer list for Ichimoku Cloud calculations.
    /// </summary>
    /// <param name="bars">Historical price bars.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line). Default is 9.</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line). Default is 26.</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B). Default is 52.</param>
    /// <param name="senkouOffset">Number of periods for the Senkou offset. Default is 26.</param>
    /// <param name="chikouOffset">Number of periods for the Chikou offset. Default is 26.</param>
    /// <returns>An IchimokuList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static IchimokuList ToIchimokuList(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52,
        int senkouOffset = 26,
        int chikouOffset = 26)
        => new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset) { bars };
}
