namespace Performance;

// STREAMING OBSERVER PATH

/// <summary>
/// Streams bars one at a time into a fresh provider and ends each chain in an observer that
/// handles only <c>OnAdd</c>, the narrowest subscriber a consumer can write. The gap between
/// <see cref="BarHubOnly"/> and <see cref="MinimalObserver"/> is the cost of delivery; the gap
/// between <see cref="MinimalObserver"/> and a hub benchmark is what the indicator hub adds.
/// </summary>
[ShortRunJob]
public class StreamObserver
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();
    private const int n = 14;

    [Benchmark(Baseline = true)]
    public int BarHubOnly()
    {
        BarHub provider = new();
        return Stream(provider);
    }

    [Benchmark]
    public double MinimalObserver()
    {
        BarHub provider = new();
        AddOnlyObserver<IBar> observer = new(provider);
        Stream(provider);
        return observer.Last;
    }

    [Benchmark]
    public double EmaHub() => StreamThrough(static provider => provider.ToEmaHub(n));

    [Benchmark]
    public double SmaHub() => StreamThrough(static provider => provider.ToSmaHub(n));

    [Benchmark]
    public double RsiHub() => StreamThrough(static provider => provider.ToRsiHub(n));

    [Benchmark]
    public double MacdHub() => StreamThrough(static provider => provider.ToMacdHub(12, 26, 9));

    private static double StreamThrough<T>(Func<BarHub, IStreamObservable<T>> chain)
        where T : IReusable
    {
        BarHub provider = new();
        AddOnlyObserver<T> observer = new(chain(provider));
        Stream(provider);
        return observer.Last;
    }

    private static int Stream(BarHub provider)
    {
        foreach (Bar bar in bars)
        {
            provider.Add(bar);
        }

        return provider.Results.Count;
    }

    private sealed class AddOnlyObserver<T> : IStreamObserver<T>
        where T : IReusable
    {
        private readonly IDisposable subscription;
        private Exception fault;

        internal AddOnlyObserver(IStreamObservable<T> provider)
            => subscription = provider.Subscribe(this);

        // a faulted run must not be timed as a clean one
        internal double Last
        {
            get => fault is null
                ? field
                : throw new InvalidOperationException("The observed hub faulted.", fault);
            private set;
        }

        public bool IsSubscribed { get; private set; } = true;

        public void Unsubscribe()
        {
            subscription.Dispose();
            IsSubscribed = false;
        }

        public void OnAdd(T item, bool notify, int? indexHint) => Last = item.Value;

        public void OnRebuild(DateTime fromTimestamp) { }

        public void OnPrune(DateTime toTimestamp) { }

        public void OnError(Exception exception) => fault = exception;

        public void OnCompleted() { }
    }
}
