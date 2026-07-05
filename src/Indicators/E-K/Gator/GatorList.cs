namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Gator Oscillator from incremental reusable values.
/// </summary>
public class GatorList : BufferList<GatorResult>, IIncrementFromChain
{
    private readonly AlligatorList _alligatorList = [];
    private GatorResult? _previousResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatorList"/> class.
    /// </summary>
    public GatorList() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GatorList"/> class with initial reusable values.
    /// </summary>
    /// <param name="values">Initial reusable values to populate the list.</param>
    public GatorList(IReadOnlyList<IReusable> values)
        : this() => Add(values);

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        _alligatorList.Add(timestamp, value);
        AlligatorResult a = _alligatorList[^1];

        if (Count == 0)
        {
            // First result is always null values
            AddInternal(new GatorResult(timestamp));
            _previousResult = this[^1];
            return;
        }

        GatorResult p = _previousResult!;

        double? upper = (a.Jaw - a.Teeth).Abs();
        double? lower = -(a.Teeth - a.Lips).Abs();

        GatorResult result = new(
            Timestamp: timestamp,
            Upper: upper,
            Lower: lower,
            UpperIsExpanding: p.Upper is not null ? upper > p.Upper : null,
            LowerIsExpanding: p.Lower is not null ? lower < p.Lower : null);

        AddInternal(result);
        _previousResult = result;
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Hl2OrValue());
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            IReusable v = values[i];
            Add(v.Timestamp, v.Hl2OrValue());
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _alligatorList.Clear();
        _previousResult = null;
    }
}

public static partial class Gator
{
    /// <summary>
    /// Creates a buffer list for Gator Oscillator calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    public static GatorList ToGatorList(
        this IReadOnlyList<IReusable> source)
        => new() { source };
}
