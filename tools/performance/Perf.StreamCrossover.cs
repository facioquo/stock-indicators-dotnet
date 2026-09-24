namespace Performance;

// SERIES VS STREAM CROSSOVER

/// <summary>
/// Prices one newly arrived bar two ways at each history length: re-running the Series
/// method over the whole history, or adding the bar to a hub already holding that history.
/// The Series cost grows with the history and the hub cost does not, so the history length
/// where they cross is where streaming starts to win for incrementally arriving bars.
/// </summary>
[ShortRunJob]
public class StreamCrossover
{
    private static readonly IReadOnlyList<Bar> history = Data.GetLongish();

    private IReadOnlyList<Bar> prefix = [];
    private Feed ema = null!;
    private Feed sma = null!;
    private Feed rsi = null!;
    private Feed macd = null!;

    [Params(10, 20, 50, 100, 200, 500, 1000)]
    public int Bars { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        prefix = [.. history.Take(Bars)];
        ema = new Feed(prefix, static p => p.ToEmaHub(20));
        sma = new Feed(prefix, static p => p.ToSmaHub(20));
        rsi = new Feed(prefix, static p => p.ToRsiHub(14));
        macd = new Feed(prefix, static p => p.ToMacdHub(12, 26, 9));
    }

    [Benchmark] public double? EmaSeries() => prefix.ToEma(20)[^1].Ema;
    [Benchmark] public double? SmaSeries() => prefix.ToSma(20)[^1].Sma;
    [Benchmark] public double? RsiSeries() => prefix.ToRsi(14)[^1].Rsi;
    [Benchmark] public double? MacdSeries() => prefix.ToMacd(12, 26, 9)[^1].Macd;

    [Benchmark] public int EmaStream() => ema.Next();
    [Benchmark] public int SmaStream() => sma.Next();
    [Benchmark] public int RsiStream() => rsi.Next();
    [Benchmark] public int MacdStream() => macd.Next();

    /// <summary>
    /// A provider holding the history, with one hub attached, that takes one new bar per call.
    /// </summary>
    private sealed class Feed
    {
        private readonly BarHub provider;
        private DateTime timestamp;
        private decimal close;
        private int count;

        internal Feed(IReadOnlyList<Bar> prefix, Action<BarHub> attach)
        {
            // at steady state the cache is full, so each arrival also prunes one bar
            provider = new BarHub(Math.Max(prefix.Count, 100));
            attach(provider);
            provider.Add(prefix);
            timestamp = prefix[^1].Timestamp;
            close = prefix[^1].Close;
        }

        internal int Next()
        {
            // seconds, not days: the engine calls this millions of times
            timestamp = timestamp.AddSeconds(1);
            close += close * 0.001m * ((++count % 3) - 1);
            provider.Add(new Bar(timestamp, close, close + 1, close - 1, close, 1000));
            return provider.Results.Count;
        }
    }
}
