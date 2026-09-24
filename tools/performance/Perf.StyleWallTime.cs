namespace Performance;

// STYLE WALL TIME BY DATASET SIZE

/// <summary>
/// Processes a whole historical dataset in each style, at each dataset size, from an empty start:
/// one Series call, one buffer list built from the bars, a hub fed one bar at a time, and a hub
/// attached to a provider already holding the bars.
/// </summary>
[ShortRunJob]
public class StyleWallTime
{
    private static readonly IReadOnlyList<Bar> history = Data.GetLongish();

    private IReadOnlyList<Bar> bars = [];

    [Params(10, 50, 100, 500, 1000, 5000)]
    public int Bars { get; set; }

    [GlobalSetup]
    public void Setup() => bars = [.. history.Take(Bars)];

    [Benchmark] public int EmaSeries() => bars.ToEma(20).Count;
    [Benchmark] public int EmaBuffer() => bars.ToEmaList(20).Count;
    [Benchmark] public int EmaStreamFed() => Fed(static p => p.ToEmaHub(20));
    [Benchmark] public int EmaStreamAttached() => Attached(static p => p.ToEmaHub(20).Results.Count);

    [Benchmark] public int SmaSeries() => bars.ToSma(20).Count;
    [Benchmark] public int SmaBuffer() => bars.ToSmaList(20).Count;
    [Benchmark] public int SmaStreamFed() => Fed(static p => p.ToSmaHub(20));
    [Benchmark] public int SmaStreamAttached() => Attached(static p => p.ToSmaHub(20).Results.Count);

    [Benchmark] public int RsiSeries() => bars.ToRsi(14).Count;
    [Benchmark] public int RsiBuffer() => bars.ToRsiList(14).Count;
    [Benchmark] public int RsiStreamFed() => Fed(static p => p.ToRsiHub(14));
    [Benchmark] public int RsiStreamAttached() => Attached(static p => p.ToRsiHub(14).Results.Count);

    [Benchmark] public int MacdSeries() => bars.ToMacd(12, 26, 9).Count;
    [Benchmark] public int MacdBuffer() => bars.ToMacdList(12, 26, 9).Count;
    [Benchmark] public int MacdStreamFed() => Fed(static p => p.ToMacdHub(12, 26, 9));
    [Benchmark] public int MacdStreamAttached() => Attached(static p => p.ToMacdHub(12, 26, 9).Results.Count);

    // bars arrive one at a time at a hub subscribed before the first one
    private int Fed(Action<BarHub> attach)
    {
        BarHub provider = new();
        attach(provider);

        for (int i = 0; i < bars.Count; i++)
        {
            provider.Add(bars[i]);
        }

        return provider.Results.Count;
    }

    // the hub subscribes after the provider holds every bar, and catches up on attach
    private int Attached(Func<BarHub, int> attach)
    {
        BarHub provider = new();
        provider.Add(bars);
        return attach(provider);
    }
}
