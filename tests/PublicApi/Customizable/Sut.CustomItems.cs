namespace Sut;

// SUBJECT UNDER TEST (SUT)

public sealed record CustomBar : IBar
{
    // override, redirect required properties
    DateTime ISeries.Timestamp
        => CloseDate;

    decimal IBar.Close
        => CloseValue;

    // custom properties
    public int MyOtherProperty { get; set; }
    public DateTime CloseDate { get; init; }
    public decimal CloseValue { get; init; }

    // required base properties
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Volume { get; init; }

    double IReusable.Value
        => (double)CloseValue;
}

[Serializable]
public record CustomBarInherited
(
    DateTime CloseDate,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    int MyOtherProperty
) : Bar(CloseDate, Open, High, Low, Close, Volume);

public sealed record CustomReusable : IReusable
{
    public DateTime Timestamp { get; init; }
    public double? Sma { get; init; }

    double IReusable.Value
        => Sma.Null2NaN();
}

public sealed record CustomReusableInherited(
    DateTime Timestamp,
    double? Sma
    ) : IReusable
{
    public double Value
        => Sma.Null2NaN();
}

public sealed class CustomSeries : ISeries
{
    public DateTime Timestamp { get; init; }
    public int Id { get; init; }
    public bool MyProperty { get; init; }
    public double? Ema { get; set; }
}

public record class CustomInheritEma : EmaResult
{
    // classic constructor
    public CustomInheritEma(DateTime Timestamp)
        : base(Timestamp) { }

    // custom properties (has defaults)
    public int Id { get; set; }
    public bool MyProperty { get; set; }
}

public static class CustomIndicator
{
    // SERIES, from CHAIN
    public static IReadOnlyList<CustomReusable> GetIndicator<T>(
        this IReadOnlyList<T> source,
        int lookbackPeriods)
        where T : IReusable
        => source
            .ToSortedList()
            .CalcIndicator(lookbackPeriods);

    private static List<CustomReusable> CalcIndicator<T>(
        this IReadOnlyList<T> source,
        int lookbackPeriods)
        where T : IReusable
    {
        // check parameter arguments
        if (lookbackPeriods <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackPeriods), lookbackPeriods,
                "Lookback periods must be greater than 0 for SMA.");
        }

        // initialize
        int length = source.Count;
        List<CustomReusable> results = new(length);

        // roll through source values
        for (int i = 0; i < length; i++)
        {
            T s = source[i];

            double? sma;

            if (i >= lookbackPeriods - 1)
            {
                double sum = 0;
                for (int p = i - lookbackPeriods + 1; p <= i; p++)
                {
                    T ps = source[p];
                    sum += ps.Value;
                }

                sma = (sum / lookbackPeriods).NaN2Null();
            }
            else
            {
                sma = null;
            }

            results.Add(new() {
                Timestamp = s.Timestamp,
                Sma = sma
            });
        }

        return results;
    }
}
