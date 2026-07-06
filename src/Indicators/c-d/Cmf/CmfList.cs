namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Chaikin Money Flow (CMF) from incremental bars.
/// </summary>
public class CmfList : BufferList<CmfResult>, IIncrementFromBar, ICmf
{
    private readonly AdlList _adlList;
    private readonly Queue<(double Volume, double? Mfv)> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="CmfList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CmfList(int lookbackPeriods = 20)
    {
        Cmf.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _adlList = [];
        _buffer = new Queue<(double, double?)>(lookbackPeriods);

        Name = $"CMF({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CmfList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public CmfList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;
        double volume = (double)bar.Volume;

        // Calculate ADL
        _adlList.Add(bar);
        AdlResult adlResult = _adlList[^1];

        // Update buffer with consolidated tuple
        _buffer.Update(LookbackPeriods, (volume, adlResult.MoneyFlowVolume));

        double? cmf = null;

        // Calculate CMF when we have enough data
        if (_buffer.Count == LookbackPeriods)
        {
            double? sumMfv = 0;
            double sumVol = 0;

            foreach ((double Volume, double? Mfv) in _buffer)
            {
                sumMfv += Mfv;
                sumVol += Volume;
            }

            double? avgMfv = sumMfv / LookbackPeriods;
            double avgVol = sumVol / LookbackPeriods;

            if (avgVol != 0)
            {
                cmf = avgMfv / avgVol;
            }
        }

        AddInternal(new CmfResult(
            timestamp,
            adlResult.MoneyFlowMultiplier,
            adlResult.MoneyFlowVolume,
            cmf));
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

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _adlList.Clear();
        _buffer.Clear();
    }
}

public static partial class Cmf
{
    /// <summary>
    /// Creates a buffer list for Chaikin Money Flow (CMF) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static CmfList ToCmfList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
        => new(lookbackPeriods) { bars };
}
