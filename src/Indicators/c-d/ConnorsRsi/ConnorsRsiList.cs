namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Connors RSI from incremental reusable values.
/// </summary>
public class ConnorsRsiList : BufferList<ConnorsRsiResult>, IIncrementFromChain
{
    private readonly RsiList _rsiClose;
    private readonly RsiList _rsiStreak;
    private readonly Queue<double> _gainBuffer;
    private double _prevValue = double.NaN;
    private double _streak;
    private int _processedCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnorsRsiList"/> class.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation on close prices.</param>
    /// <param name="streakPeriods">Number of periods for the RSI calculation on streak.</param>
    /// <param name="rankPeriods">Number of periods for the percent rank calculation.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="rankPeriods"/> is invalid.</exception>
    public ConnorsRsiList(
        int rsiPeriods = 3,
        int streakPeriods = 2,
        int rankPeriods = 100)
    {
        ConnorsRsi.Validate(rsiPeriods, streakPeriods, rankPeriods);

        RsiPeriods = rsiPeriods;
        StreakPeriods = streakPeriods;
        RankPeriods = rankPeriods;

        _rsiClose = new RsiList(rsiPeriods);
        _rsiStreak = new RsiList(streakPeriods);
        _gainBuffer = new Queue<double>(rankPeriods + 1);
        _processedCount = 0;

        Name = $"CONNORSRSI({rsiPeriods}, {streakPeriods}, {rankPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnorsRsiList"/> class with initial values.
    /// </summary>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation on close prices.</param>
    /// <param name="streakPeriods">Number of periods for the RSI calculation on streak.</param>
    /// <param name="rankPeriods">Number of periods for the percent rank calculation.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public ConnorsRsiList(
        int rsiPeriods,
        int streakPeriods,
        int rankPeriods,
        IReadOnlyList<IReusable> values)
        : this(rsiPeriods, streakPeriods, rankPeriods) => Add(values);

    /// <inheritdoc />
    public int RsiPeriods { get; init; }

    /// <inheritdoc />
    public int StreakPeriods { get; init; }

    /// <inheritdoc />
    public int RankPeriods { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of results to retain in the list.
    /// When the list exceeds this value, the oldest items are pruned.
    /// Also propagates to the inner RSI buffer lists.
    /// </summary>
    public new int MaxListSize
    {
        get => base.MaxListSize;
        set {
            base.MaxListSize = value;

            // Propagate MaxListSize to inner RSI buffer lists
            _rsiClose.MaxListSize = value;
            _rsiStreak.MaxListSize = value;
        }
    }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Calculate streak
        UpdateStreak(value);

        // Calculate RSI components
        _rsiClose.Add(timestamp, value);
        double? rsi = _rsiClose[^1].Rsi;

        _rsiStreak.Add(timestamp, _streak);
        double? rsiStreak = GetRsiStreakValue();

        // Calculate gain and percent rank
        double gain = CalculateGain(value);
        _gainBuffer.Update(RankPeriods + 1, gain);
        double? percentRank = CalculatePercentRank(gain);

        // Calculate Connors RSI
        double? connorsRsi = CalculateConnorsRsi(rsi, rsiStreak, percentRank);

        AddInternal(new ConnorsRsiResult(
            Timestamp: timestamp,
            Streak: _streak,
            Rsi: rsi,
            RsiStreak: rsiStreak,
            PercentRank: percentRank,
            ConnorsRsi: connorsRsi));

        _prevValue = value;
        _processedCount++;
    }

    private void UpdateStreak(double value)
    {
        if (_processedCount == 0)
        {
            _streak = 0;
            _prevValue = value;
        }
        else
        {
            _streak = double.IsNaN(value) || double.IsNaN(_prevValue)
                ? double.NaN
                : value > _prevValue
                    ? _streak >= 0 ? _streak + 1 : 1
                    : value < _prevValue
                        ? _streak <= 0 ? _streak - 1 : -1
                        : 0;
        }
    }

    private double? GetRsiStreakValue()
        => _processedCount >= StreakPeriods + 2 ? _rsiStreak[^1].Rsi : null;

    private double CalculateGain(double value)
        => double.IsNaN(value) || double.IsNaN(_prevValue) || _prevValue <= 0
            ? double.NaN
            : (value - _prevValue) / _prevValue;

    private double? CalculatePercentRank(double gain)
    {
        if (_processedCount < RankPeriods || double.IsNaN(gain))
        {
            return null;
        }

        int qty = 0;
        foreach (double g in _gainBuffer)
        {
            if (double.IsNaN(g))
            {
                return null;
            }

            if (g < gain)
            {
                qty++;
            }
        }

        return 100.0 * qty / RankPeriods;
    }

    private double? CalculateConnorsRsi(double? rsi, double? rsiStreak, double? percentRank)
    {
        int startPeriod = Math.Max(RsiPeriods, Math.Max(StreakPeriods, RankPeriods)) + 2;

        return _processedCount >= startPeriod - 1 && rsi.HasValue && rsiStreak.HasValue && percentRank.HasValue
            ? (rsi.Value + rsiStreak.Value + percentRank.Value) / 3
            : null;
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _rsiClose.Clear();
        _rsiStreak.Clear();
        _gainBuffer.Clear();
        _prevValue = double.NaN;
        _streak = 0;
        _processedCount = 0;
    }
}

public static partial class ConnorsRsi
{
    /// <summary>
    /// Creates a buffer list for Connors RSI calculations.
    /// </summary>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="rsiPeriods">Number of periods for the RSI calculation on close prices.</param>
    /// <param name="streakPeriods">Number of periods for the RSI calculation on streak.</param>
    /// <param name="rankPeriods">Number of periods for the percent rank calculation.</param>
    /// <returns>A new <see cref="ConnorsRsiList"/> instance.</returns>
    public static ConnorsRsiList ToConnorsRsiList(
        this IReadOnlyList<IReusable> source,
        int rsiPeriods = 3,
        int streakPeriods = 2,
        int rankPeriods = 100)
        => new(rsiPeriods, streakPeriods, rankPeriods) { source };
}
