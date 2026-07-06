using System.Globalization;

namespace FacioQuo.Stock.Indicators;

#pragma warning disable IDE0072 // Missing cases in switch statement

/// <summary>
/// Moving Average Envelopes from incremental reusable values.
/// </summary>
public class MaEnvelopesList : BufferList<MaEnvelopeResult>, IIncrementFromChain
{
    private readonly object _maList;
    private readonly double _offsetRatio;
    private readonly MaType _movingAverageType;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaEnvelopesList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the moving average.</param>
    /// <param name="percentOffset">Percentage offset for the envelopes.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="movingAverageType"/> is invalid.</exception>
    public MaEnvelopesList(
        int lookbackPeriods,
        double percentOffset = 2.5,
        MaType movingAverageType = MaType.SMA)
    {
        MaEnvelopes.Validate(percentOffset);
        LookbackPeriods = lookbackPeriods;
        PercentOffset = percentOffset;
        MovingAverageType = movingAverageType;
        _movingAverageType = movingAverageType;

        _offsetRatio = percentOffset / 100d;

        // Create the appropriate MA BufferList based on type
        _maList = movingAverageType switch {
            MaType.ALMA => new AlmaList(lookbackPeriods, offset: 0.85, sigma: 6),
            MaType.DEMA => new DemaList(lookbackPeriods),
            MaType.EMA => new EmaList(lookbackPeriods),
            MaType.EPMA => new EpmaList(lookbackPeriods),
            MaType.HMA => new HmaList(lookbackPeriods),
            MaType.SMA => new SmaList(lookbackPeriods),
            MaType.SMMA => new SmmaList(lookbackPeriods),
            MaType.TEMA => new TemaList(lookbackPeriods),
            MaType.WMA => new WmaList(lookbackPeriods),

            _ => throw new ArgumentOutOfRangeException(
                    nameof(movingAverageType), movingAverageType,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Moving Average Envelopes does not support {0}.",
                        Enum.GetName(movingAverageType)))
        };

        Name = $"MAENVELOPES({lookbackPeriods}, {percentOffset}, {movingAverageType})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaEnvelopesList"/> class with initial values.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods for the moving average.</param>
    /// <param name="percentOffset">Percentage offset for the envelopes.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public MaEnvelopesList(
        int lookbackPeriods,
        double percentOffset,
        MaType movingAverageType,
        IReadOnlyList<IReusable> values)
        : this(lookbackPeriods, percentOffset, movingAverageType) => Add(values);

    /// <inheritdoc />
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public double PercentOffset { get; init; }

    /// <inheritdoc />
    public MaType MovingAverageType { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of results to retain in the list.
    /// When the list exceeds this value, the oldest items are pruned.
    /// Also propagates to the inner MA buffer list.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is out of the valid range</exception>
    public new int MaxListSize
    {
        get => base.MaxListSize;
        set {
            base.MaxListSize = value;

            // Propagate MaxListSize to the inner MA buffer list
            _ = _movingAverageType switch {
                MaType.ALMA => ((AlmaList)_maList).MaxListSize = value,
                MaType.DEMA => ((DemaList)_maList).MaxListSize = value,
                MaType.EMA => ((EmaList)_maList).MaxListSize = value,
                MaType.EPMA => ((EpmaList)_maList).MaxListSize = value,
                MaType.HMA => ((HmaList)_maList).MaxListSize = value,
                MaType.SMA => ((SmaList)_maList).MaxListSize = value,
                MaType.SMMA => ((SmmaList)_maList).MaxListSize = value,
                MaType.TEMA => ((TemaList)_maList).MaxListSize = value,
                MaType.WMA => ((WmaList)_maList).MaxListSize = value,
                _ => throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
    }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Add value to the underlying MA BufferList using switch expression
        double? ma = _movingAverageType switch {
            MaType.ALMA => AddToList<AlmaList, AlmaResult>((AlmaList)_maList, timestamp, value, static r => r.Alma),
            MaType.DEMA => AddToList<DemaList, DemaResult>((DemaList)_maList, timestamp, value, static r => r.Dema),
            MaType.EMA => AddToList<EmaList, EmaResult>((EmaList)_maList, timestamp, value, static r => r.Ema),
            MaType.EPMA => AddToList<EpmaList, EpmaResult>((EpmaList)_maList, timestamp, value, static r => r.Epma),
            MaType.HMA => AddToList<HmaList, HmaResult>((HmaList)_maList, timestamp, value, static r => r.Hma),
            MaType.SMA => AddToList<SmaList, SmaResult>((SmaList)_maList, timestamp, value, static r => r.Sma),
            MaType.SMMA => AddToList<SmmaList, SmmaResult>((SmmaList)_maList, timestamp, value, static r => r.Smma),
            MaType.TEMA => AddToList<TemaList, TemaResult>((TemaList)_maList, timestamp, value, static r => r.Tema),
            MaType.WMA => AddToList<WmaList, WmaResult>((WmaList)_maList, timestamp, value, static r => r.Wma),
            _ => throw new ArgumentOutOfRangeException(nameof(timestamp))
        };

        // Calculate envelopes
        double? upperEnvelope = ma.HasValue ? ma + (ma * _offsetRatio) : null;
        double? lowerEnvelope = ma.HasValue ? ma - (ma * _offsetRatio) : null;

        AddInternal(new MaEnvelopeResult(
            Timestamp: timestamp,
            Centerline: ma,
            UpperEnvelope: upperEnvelope,
            LowerEnvelope: lowerEnvelope));
    }

    private static double? AddToList<TList, TResult>(
        TList list,
        DateTime timestamp,
        double value,
        Func<TResult, double?> selector)
        where TList : BufferList<TResult>, IIncrementFromChain
        where TResult : IReusable
    {
        list.Add(timestamp, value);
        return selector(list[^1]);
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

        // Clear the underlying MA list using switch expression
        Action clearAction = _movingAverageType switch {
            MaType.ALMA => () => ((AlmaList)_maList).Clear(),
            MaType.DEMA => () => ((DemaList)_maList).Clear(),
            MaType.EMA => () => ((EmaList)_maList).Clear(),
            MaType.EPMA => () => ((EpmaList)_maList).Clear(),
            MaType.HMA => () => ((HmaList)_maList).Clear(),
            MaType.SMA => () => ((SmaList)_maList).Clear(),
            MaType.SMMA => () => ((SmmaList)_maList).Clear(),
            MaType.TEMA => () => ((TemaList)_maList).Clear(),
            MaType.WMA => () => ((WmaList)_maList).Clear(),
            _ => throw new ArgumentOutOfRangeException(nameof(_movingAverageType))
        };

        clearAction();
    }
}

public static partial class MaEnvelopes
{
    /// <summary>
    /// Creates a buffer list for Moving Average Envelopes calculations.
    /// </summary>
    /// <param name="source">Source list of reusable values.</param>
    /// <param name="lookbackPeriods">Number of periods for the moving average.</param>
    /// <param name="percentOffset">Percentage offset for the envelopes.</param>
    /// <param name="movingAverageType">Type of moving average to use.</param>
    /// <returns>A new <see cref="MaEnvelopesList"/> instance.</returns>
    public static MaEnvelopesList ToMaEnvelopesList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods,
        double percentOffset = 2.5,
        MaType movingAverageType = MaType.SMA)
        => new(lookbackPeriods, percentOffset, movingAverageType) { source };
}
