namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Volume Weighted Moving Average (VWMA) from incremental bar values.
/// </summary>
public class VwmaList : BufferList<VwmaResult>, IIncrementFromBar, IVwma
{
    private readonly Queue<(double price, double volume)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="VwmaList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public VwmaList(
        int lookbackPeriods
    ) : this(lookbackPeriods, CandlePart.Close) { }

    /// <summary>
    /// Initializes a VWMA buffer using the selected component of each bar.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Bar component to weight by volume.</param>
    public VwmaList(int lookbackPeriods, CandlePart candlePart)
    {
        Vwma.Validate(lookbackPeriods);
        BarParts.Validate(candlePart);
        LookbackPeriods = lookbackPeriods;
        CandlePart = candlePart;

        _buffer = new Queue<(double, double)>(lookbackPeriods);

        Name = $"VWMA({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VwmaList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public VwmaList(
        int lookbackPeriods,
        IReadOnlyList<IBar> bars
    )
        : this(lookbackPeriods) => Add(bars);

    /// <summary>
    /// Initializes a VWMA buffer with initial bars and a selected component.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Bar component to weight by volume.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    public VwmaList(int lookbackPeriods, CandlePart candlePart, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods, candlePart) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public CandlePart CandlePart { get; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);
        Add(bar.Timestamp, bar.ToBarPartValue(CandlePart), (double)bar.Volume);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IBar> bars)
    {
        ArgumentNullException.ThrowIfNull(bars);

        for (int i = 0; i < bars.Count; i++)
        {
            Add(bars[i]);
        }
    }

    /// <summary>
    /// Apply new price and volume values for calculating incremental VWMA values.
    /// </summary>
    /// <param name="timestamp">Date context.</param>
    /// <param name="price">Preselected price value. CandlePart applies only to bar inputs.</param>
    /// <param name="volume">Volume value.</param>
    public void Add(DateTime timestamp, double price, double volume)
    {
        // Use BufferListUtilities extension method for consistent buffer management
        _buffer.Update(LookbackPeriods, (price, volume));

        // Calculate VWMA when we have enough values by recalculating from buffer
        // This matches the precision of the static series implementation
        double? vwma = null;
        if (_buffer.Count == LookbackPeriods)
        {
            double priceVolumeSum = 0;
            double volumeSum = 0;
            foreach ((double p, double v) in _buffer)
            {
                priceVolumeSum += p * v;
                volumeSum += v;
            }

            vwma = volumeSum != 0 ? priceVolumeSum / volumeSum : double.NaN;
        }

        AddInternal(new VwmaResult(
            timestamp,
            vwma.NaN2Null()));
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
    }
}

public static partial class Vwma
{
    /// <summary>
    /// Creates a buffer list for Volume Weighted Moving Average (VWMA) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static VwmaList ToVwmaList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
        => new(lookbackPeriods) { bars };

    /// <summary>
    /// Creates a VWMA buffer using the selected component of each bar.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="candlePart">Bar component to weight by volume. Default is Close.</param>
    public static VwmaList ToVwmaList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        CandlePart candlePart = CandlePart.Close)
        => new(lookbackPeriods, candlePart) { bars };
}
