namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Average Directional Index (ADX) from incremental reusable values.
/// </summary>
public class AdxList : BufferList<AdxResult>, IIncrementFromBar, IAdx
{
    private readonly Queue<AdxBuffer> _buffer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdxList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public AdxList(int lookbackPeriods)
    {
        Adx.Validate(lookbackPeriods);
        LookbackPeriods = lookbackPeriods;

        _buffer = new Queue<AdxBuffer>(lookbackPeriods);

        Name = $"ADX({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AdxList"/> class with initial bars.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public AdxList(int lookbackPeriods, IReadOnlyList<IBar> bars)
        : this(lookbackPeriods) => Add(bars);

    /// <inheritdoc />
    public int LookbackPeriods { get; private init; }

    /// <inheritdoc />
    public void Add(IBar bar)
    {
        ArgumentNullException.ThrowIfNull(bar);

        DateTime timestamp = bar.Timestamp;

        AdxBuffer curr = new(
            (double)bar.High,
            (double)bar.Low,
            (double)bar.Close);

        // skip first period
        if (Count == 0)
        {
            _buffer.Update(LookbackPeriods, curr);
            AddInternal(new AdxResult(timestamp));
            return;
        }

        // get last, then peek at the oldest (for ADXR), then add current object
        AdxBuffer last = _buffer.Last();
        AdxBuffer? priorForAdxr = _buffer.Count == LookbackPeriods ? _buffer.Peek() : null;
        _buffer.Update(LookbackPeriods, curr);

        // calculate TR, PDM, and MDM
        double hmpc = Math.Abs(curr.High - last.Close);
        double lmpc = Math.Abs(curr.Low - last.Close);
        double hmph = curr.High - last.High;
        double plml = last.Low - curr.Low;

        curr.Tr = Math.Max(curr.High - curr.Low, Math.Max(hmpc, lmpc));

        curr.Pdm1 = hmph > plml ? Math.Max(hmph, 0) : 0;
        curr.Mdm1 = plml > hmph ? Math.Max(plml, 0) : 0;

        // skip incalculable
        if (Count < LookbackPeriods)
        {
            AddInternal(new AdxResult(timestamp));
            return;
        }

        // re/initialize smooth TR and DM
        if (Count >= LookbackPeriods && last.Trs == 0)
        {
            foreach (AdxBuffer buffer in _buffer)
            {
                curr.Trs += buffer.Tr;
                curr.Pdm += buffer.Pdm1;
                curr.Mdm += buffer.Mdm1;
            }
        }

        // normal movement calculations
        else
        {
            curr.Trs = last.Trs - (last.Trs / LookbackPeriods) + curr.Tr;
            curr.Pdm = last.Pdm - (last.Pdm / LookbackPeriods) + curr.Pdm1;
            curr.Mdm = last.Mdm - (last.Mdm / LookbackPeriods) + curr.Mdm1;
        }

        // skip incalculable periods
        if (curr.Trs == 0)
        {
            AddInternal(new AdxResult(timestamp));
            return;
        }

        // directional increments
        double pdi = 100 * curr.Pdm / curr.Trs;
        double mdi = 100 * curr.Mdm / curr.Trs;

        // calculate directional index (DX)
        curr.Dx = pdi == mdi
            ? 0
            : pdi + mdi != 0
            ? 100 * Math.Abs(pdi - mdi) / (pdi + mdi)
            : double.NaN;

        // skip incalculable ADX periods
        if (Count < (2 * LookbackPeriods) - 1)
        {
            AddInternal(new AdxResult(timestamp,
                Pdi: pdi.NaN2Null(),
                Mdi: mdi.NaN2Null(),
                Dx: curr.Dx.NaN2Null()));

            return;
        }

        double adxr = double.NaN;

        // re/initialize ADX
        if (Count >= (2 * LookbackPeriods) - 1 && double.IsNaN(last.Adx))
        {
            double sumDx = 0;

            foreach (AdxBuffer buffer in _buffer)
            {
                sumDx += buffer.Dx;
            }

            curr.Adx = sumDx / LookbackPeriods;
        }

        // normal ADX calculation
        else
        {
            curr.Adx
                = ((last.Adx * (LookbackPeriods - 1)) + curr.Dx)
                / LookbackPeriods;

            // Calculate ADXR using the prior ADX value (lookbackPeriods ago)
            // priorForAdxr was captured before buffer update, so it's from the correct period
            if (priorForAdxr != null && !double.IsNaN(priorForAdxr.Adx))
            {
                adxr = (curr.Adx + priorForAdxr.Adx) / 2;
            }
        }

        AdxResult r = new(
            Timestamp: timestamp,
            Pdi: pdi,
            Mdi: mdi,
            Dx: curr.Dx.NaN2Null(),
            Adx: curr.Adx.NaN2Null(),
            Adxr: adxr.NaN2Null());

        AddInternal(r);
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
        _buffer.Clear();
    }
    internal class AdxBuffer(
        double high,
        double low,
        double close)
    {
        internal double High { get; init; } = high;
        internal double Low { get; init; } = low;
        internal double Close { get; init; } = close;

        internal double Tr { get; set; } = double.NaN;
        internal double Pdm1 { get; set; } = double.NaN;
        internal double Mdm1 { get; set; } = double.NaN;

        internal double Trs { get; set; }
        internal double Pdm { get; set; }
        internal double Mdm { get; set; }

        internal double Dx { get; set; } = double.NaN;
        internal double Adx { get; set; } = double.NaN;
    }
}

public static partial class Adx
{
    /// <summary>
    /// Creates a buffer list for Average Directional Index (ADX) calculations.
    /// </summary>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <returns>An initialized <see cref="AdxList" />.</returns>
    public static AdxList ToAdxList(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
        => new(lookbackPeriods) { bars };
}
