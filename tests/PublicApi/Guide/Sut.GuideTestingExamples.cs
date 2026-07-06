namespace Sut.GuideTesting;

// SUBJECT UNDER TEST (SUT)
//
// These illustrative consumer types back the code samples in the
// user-facing "Testing your analysis code" guide (docs/guide/testing.md).
// Their purpose is to prove those documented samples actually compile and
// pass against the public API, so we don't ship guidance that doesn't work.
// Keep them in lock-step with the guide; names here match the guide exactly.

internal enum Signal { None, Buy, Sell }

internal static class Strategy
{
    // buy on an up-cross, sell on a down-cross
    public static Signal Crossover(
        SmaResult fastPrev, SmaResult fastNow,
        SmaResult slowPrev, SmaResult slowNow)
    {
        if (fastPrev.Sma is null || slowPrev.Sma is null
         || fastNow.Sma is null || slowNow.Sma is null)
        {
            return Signal.None;   // still warming up
        }

        bool wasBelow = fastPrev.Sma <= slowPrev.Sma;
        bool isAbove = fastNow.Sma > slowNow.Sma;

        return (wasBelow, isAbove) switch {
            (true, true) => Signal.Buy,     // crossed up
            (false, false) => Signal.Sell,  // crossed down
            _ => Signal.None
        };
    }

    // enter long only when momentum is oversold AND price is above its long-term trend
    public static bool ShouldEnterLong(IBar bar, RsiResult rsi, SmaResult trend)
        => rsi.Rsi <= 30
        && trend.Sma is not null
        && (double)bar.Close > trend.Sma;
}

// a small observer that raises an alert whenever RSI is at or below a threshold;
// see the Custom observers guide for the observer pattern this follows
internal sealed class RsiAlertObserver : IStreamObserver<RsiResult>, IDisposable
{
    private readonly double _threshold;
    private readonly Action<RsiResult> _onAlert;
    private IDisposable _subscription;

    public RsiAlertObserver(
        IStreamObservable<RsiResult> source,
        double threshold,
        Action<RsiResult> onAlert)
    {
        _threshold = threshold;
        _onAlert = onAlert;
        _subscription = source.Subscribe(this);
    }

    public bool IsSubscribed => _subscription is not null;

    public void OnAdd(RsiResult item, bool notify, int? indexHint)
    {
        if (item.Rsi <= _threshold)
        {
            _onAlert(item);
        }
    }

    public void OnRebuild(DateTime fromTimestamp) { /* clear derived state */ }
    public void OnPrune(DateTime toTimestamp) { /* drop retained rows */ }
    public void OnError(Exception exception) { /* surface to operator */ }
    public void OnCompleted() { /* finalize */ }

    public void Unsubscribe()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    public void Dispose() => Unsubscribe();
}
