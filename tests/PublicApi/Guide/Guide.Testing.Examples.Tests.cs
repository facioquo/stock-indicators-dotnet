using Sut.GuideTesting;

namespace GuideExamples;

// Validates the code samples in the user-facing testing guide
// (docs/guide/testing.md). Each test mirrors a documented snippet so we
// can prove the guidance works against the published public API.
//
// The guide uses a user-supplied `TestData.DailyBars()` fixture helper;
// here we substitute the shared default fixture (the same ~502 daily
// S&P 500 bars the guide points readers to copy).

[TestClass]
public class TestingGuide
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();

    [TestMethod]
    public void Crossover_FastRisesAboveSlow_IsBuy()
    {
        DateTime t = default;  // timestamps don't affect the rule

        Signal signal = Strategy.Crossover(
            fastPrev: new(t, 99), fastNow: new(t, 101),
            slowPrev: new(t, 100), slowNow: new(t, 100));

        signal.Should().Be(Signal.Buy);
    }

    [TestMethod]
    public void ShouldEnterLong_OversoldInUptrend_IsTrue()
    {
        DateTime t = default;
        IBar bar = new Bar(t, Open: 104, High: 106, Low: 103, Close: 105, Volume: 0);

        bool enter = Strategy.ShouldEnterLong(
            bar,
            rsi: new(t, Rsi: 25),       // oversold
            trend: new(t, Sma: 100));   // price (105) is above trend

        enter.Should().BeTrue();
    }

    [TestMethod]
    public void ShouldEnterLong_OversoldBelowTrend_IsFalse()
    {
        // near miss: oversold, but price is below the trend line
        DateTime t = default;
        IBar bar = new Bar(t, Open: 96, High: 97, Low: 94, Close: 95, Volume: 0);

        bool enter = Strategy.ShouldEnterLong(
            bar,
            rsi: new(t, Rsi: 25),
            trend: new(t, Sma: 100));

        enter.Should().BeFalse();
    }

    [TestMethod]
    public void GoldenCrossBacktest_SignalsAlternate()
    {
        IReadOnlyList<SmaResult> fast = bars.ToSma(50);
        IReadOnlyList<SmaResult> slow = bars.ToSma(200);

        List<Signal> signals = [];
        for (int i = 1; i < bars.Count; i++)
        {
            Signal s = Strategy.Crossover(fast[i - 1], fast[i], slow[i - 1], slow[i]);
            if (s != Signal.None) { signals.Add(s); }
        }

        // a real invariant of crossovers: you can't buy twice without selling between
        signals.Should().NotBeEmpty();
        for (int i = 1; i < signals.Count; i++)
        {
            signals[i].Should().NotBe(signals[i - 1]);
        }
    }

    [TestMethod]
    public void Alerting_OnStreamedBars_FiresOnlyWhenOversold()
    {
        BarHub barHub = new();
        RsiHub rsiHub = barHub.ToRsiHub(14);

        List<RsiResult> alerts = [];
        using RsiAlertObserver observer = new(rsiHub, threshold: 30, onAlert: alerts.Add);

        foreach (Bar bar in bars) { barHub.Add(bar); }   // replay the feed

        alerts.Should().OnlyContain(r => r.Rsi <= 30);

        barHub.EndTransmission();
    }
}
