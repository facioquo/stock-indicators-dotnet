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

    // rows seen since construction or Clear, and the row index of the first retained history entry
    private int _rowsSeen;
    private int _historyBase;

    // Rolling window buffers for max/min calculations
    private readonly Queue<(double High, double Low)> _tenkanBuffer;
    private readonly Queue<(double High, double Low)> _kijunBuffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="IchimokuList"/> class, offsetting both
    /// spans by <paramref name="kijunPeriods"/> as the matching Series and hub do.
    /// </summary>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line), and for both offsets.</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is invalid.</exception>
    public IchimokuList(
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52)
        : this(tenkanPeriods, kijunPeriods, senkouBPeriods, kijunPeriods, kijunPeriods) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IchimokuList"/> class with one offset for both spans.
    /// </summary>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="offsetPeriods">Number of periods for both the Senkou and Chikou offsets.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is invalid.</exception>
    public IchimokuList(
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int offsetPeriods)
        : this(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods, offsetPeriods) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="IchimokuList"/> class.
    /// </summary>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="senkouOffset">Number of periods for the Senkou offset.</param>
    /// <param name="chikouOffset">Number of periods for the Chikou offset.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is invalid.</exception>
    public IchimokuList(
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int senkouOffset,
        int chikouOffset)
    {
        Ichimoku.Validate(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset);

        TenkanPeriods = tenkanPeriods;
        KijunPeriods = kijunPeriods;
        SenkouBPeriods = senkouBPeriods;
        SenkouOffset = senkouOffset;
        ChikouOffset = chikouOffset;

        // Initialize buffers
        _historicalResults = [];
        _historicalHighLow = [];
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

        AddInternal(Calculate(bar, chikouSpan: null));

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
        List<double> allCloses = [with(bars.Count)];
        for (int i = 0; i < bars.Count; i++)
        {
            allCloses.Add((double)bars[i].Close);
        }

        for (int i = 0; i < bars.Count; i++)
        {
            // Calculate Chikou Span (lagging span) - look ahead in the pre-populated list
            int futureIndex = i + ChikouOffset;
            double? chikouSpan = futureIndex < allCloses.Count ? allCloses[futureIndex] : null;

            AddInternal(Calculate(bars[i], chikouSpan));
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
        _rowsSeen = 0;
        _historyBase = 0;
    }
    /// <summary>
    /// Calculates the result for one bar from the rolling buffers and the bounded history,
    /// indexed by rows seen so a pruned result list does not shift the lookbacks.
    /// </summary>
    /// <param name="bar">Bar to calculate from.</param>
    /// <param name="chikouSpan">Lagging span for this row when it is known up front.</param>
    /// <returns>The result for the bar.</returns>
    private IchimokuResult Calculate(IBar bar, double? chikouSpan)
    {
        int index = _rowsSeen++;

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

        if (index >= senkouStartPeriod)
        {
            if (SenkouOffset == 0)
            {
                senkouSpanA = (tenkanSen + kijunSen) / 2d;
            }
            else if (index >= SenkouOffset)
            {
                // Look back to get historical Tenkan and Kijun values
                int lookbackIndex = index - SenkouOffset - _historyBase;
                if (lookbackIndex >= 0 && lookbackIndex < _historicalResults.Count)
                {
                    (DateTime _, double? historicalTenkan, double? historicalKijun) = _historicalResults[lookbackIndex];
                    senkouSpanA = (historicalTenkan + historicalKijun) / 2d;
                }
            }
        }

        // Calculate Senkou Span B (leading span B)
        double? senkouSpanB = CalculateSenkouSpanB(index);

        TrimHistory();

        return new IchimokuResult(
            Timestamp: bar.Timestamp,
            TenkanSen: tenkanSen,
            KijunSen: kijunSen,
            SenkouSpanA: senkouSpanA,
            SenkouSpanB: senkouSpanB,
            ChikouSpan: chikouSpan);
    }

    /// <summary>
    /// Drops history older than the Senkou lookbacks need, in blocks so each add stays O(1) amortized.
    /// </summary>
    private void TrimHistory()
    {
        // the lookbacks reach back SenkouOffset + SenkouBPeriods - 1 rows; one more is margin
        int keep = SenkouOffset + SenkouBPeriods + 1;

        if (_historicalHighLow.Count < 2 * keep)
        {
            return;
        }

        int remove = _historicalHighLow.Count - keep;
        _historicalHighLow.RemoveRange(0, remove);
        _historicalResults.RemoveRange(0, remove);
        _historyBase += remove;
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

        int endIndex = currentIndex - SenkouOffset - _historyBase;
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
}

/// <summary>
/// EXTENSION METHODS
/// </summary>
public static partial class Ichimoku
{
    /// <summary>
    /// Creates a buffer list for Ichimoku Cloud calculations, offsetting both spans by
    /// <paramref name="kijunPeriods"/> as <see cref="ToIchimoku(IReadOnlyList{IBar}, int, int, int)"/> does.
    /// </summary>
    /// <param name="bars">Historical price bars.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line). Default is 9.</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line), and for both offsets. Default is 26.</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B). Default is 52.</param>
    /// <returns>An IchimokuList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static IchimokuList ToIchimokuList(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52)
        => new(tenkanPeriods, kijunPeriods, senkouBPeriods) { bars };

    /// <summary>
    /// Creates a buffer list for Ichimoku Cloud calculations with one offset for both spans.
    /// </summary>
    /// <param name="bars">Historical price bars.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="offsetPeriods">Number of periods for both the Senkou and Chikou offsets.</param>
    /// <returns>An IchimokuList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static IchimokuList ToIchimokuList(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int offsetPeriods)
        => new(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods) { bars };

    /// <summary>
    /// Creates a buffer list for Ichimoku Cloud calculations.
    /// </summary>
    /// <param name="bars">Historical price bars.</param>
    /// <param name="tenkanPeriods">Number of periods for the Tenkan-sen (conversion line).</param>
    /// <param name="kijunPeriods">Number of periods for the Kijun-sen (base line).</param>
    /// <param name="senkouBPeriods">Number of periods for the Senkou Span B (leading span B).</param>
    /// <param name="senkouOffset">Number of periods for the Senkou offset.</param>
    /// <param name="chikouOffset">Number of periods for the Chikou offset.</param>
    /// <returns>An IchimokuList instance pre-populated with historical data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when bars is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are invalid.</exception>
    public static IchimokuList ToIchimokuList(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int senkouOffset,
        int chikouOffset)
        => new(tenkanPeriods, kijunPeriods, senkouBPeriods, senkouOffset, chikouOffset) { bars };
}
