namespace StreamHubs;

[TestClass]
public class BetaHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int LookbackPeriods = 20;
    private const BetaType Type = BetaType.All;
    private const int SmaPeriods = 5;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        BetaHub observer = barHub.ToSmaHub(SmaPeriods).ToBetaHub(barHub, LookbackPeriods, Type);
        IReadOnlyList<BetaResult> sut = observer.Results;

        for (int i = 20; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        // late arrival triggers observer rebuild, should equal series
        barHub.Add(Bars[80]);
        sut.IsExactly(Bars.ToSma(SmaPeriods).ToBeta(Bars, LookbackPeriods, Type));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToBeta(RevisedBars, LookbackPeriods, Type));
        sut.Should().HaveCount(barsCount - 1);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_MatchesSeriesExactly()
    {
        const int maxCacheSize = 50;
        const int totalBars = 100;

        IReadOnlyList<Bar> bars = Bars.Take(totalBars).ToList();
        IReadOnlyList<BetaResult> expected = bars.ToSma(SmaPeriods)
            .ToBeta(bars, LookbackPeriods, Type)
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        BetaHub observer = barHub.ToSmaHub(SmaPeriods).ToBetaHub(barHub, LookbackPeriods, Type);

        barHub.Add(bars);

        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 10;

        // SMA and EMA warmup feed NaN values into the hub under test
        BarHub barHub = new();
        BetaHub observer = barHub.ToSmaHub(SmaPeriods).ToBetaHub(barHub.ToEmaHub(emaPeriods), LookbackPeriods, Type);

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<BetaResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToBeta(RevisedBars.ToEma(emaPeriods), LookbackPeriods, Type));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = barHub.ToSmaHub(SmaPeriods).ToBetaHub(barHub, LookbackPeriods, Type).ToEmaHub(emaPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            Bar q = Bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105) { barHub.Add(q); }
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<EmaResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToBeta(RevisedBars, LookbackPeriods, Type).ToEma(emaPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SeparateProviders_LateArrivalOnEachSide_MatchesSeriesExactly()
    {
        BarHub eval = new();
        BarHub mrkt = new();
        BetaHub observer = eval.ToBetaHub(mrkt, LookbackPeriods, Type);

        for (int i = 0; i < barsCount; i++)
        {
            // each side skips a different bar (add later)
            if (i != 80) { eval.Add(OtherBars[i]); }

            if (i != 120) { mrkt.Add(Bars[i]); }
        }

        observer.Results.Should().HaveCount(barsCount - 2);

        eval.Add(OtherBars[80]);
        mrkt.Add(Bars[120]);

        observer.Results.IsExactly(OtherBars.ToBeta(Bars, LookbackPeriods, Type));

        // a removal on the market side drops that pair; the Series method needs both sides revised
        mrkt.RemoveAt(removeAtIndex);
        List<Bar> revisedEval = [.. OtherBars];
        revisedEval.RemoveAt(removeAtIndex);

        observer.Results.Should().HaveCount(barsCount - 1);
        observer.Results.IsExactly(revisedEval.ToBeta(RevisedBars, LookbackPeriods, Type));

        observer.Unsubscribe();
        eval.EndTransmission();
        mrkt.EndTransmission();
    }

    [TestMethod]
    public void EveryType_MatchesSeriesExactly()
    {
        BarHub eval = new();
        BarHub mrkt = new();
        BetaType[] types = [BetaType.Standard, BetaType.Up, BetaType.Down, BetaType.All];
        BetaHub[] observers = [.. types.Select(t => eval.ToBetaHub(mrkt, LookbackPeriods, t))];

        eval.Add(OtherBars);
        mrkt.Add(Bars);

        for (int t = 0; t < types.Length; t++)
        {
            observers[t].Type.Should().Be(types[t]);
            observers[t].Results.IsExactly(OtherBars.ToBeta(Bars, LookbackPeriods, types[t]), $"{types[t]} must match Series");
        }

        eval.EndTransmission();
        mrkt.EndTransmission();
    }

    [TestMethod]
    public void ZeroValues_MatchesSeriesExactly()
    {
        // a 0 value makes the next return 0, and a flat market window gives a null Beta
        List<Bar> evalBars = [.. OtherBars];
        List<Bar> mrktBars = [.. Bars];
        evalBars[200] = evalBars[200] with { Close = 0 };
        for (int i = 300; i <= 300 + LookbackPeriods; i++)
        {
            mrktBars[i] = mrktBars[i] with { Close = 100m };
        }

        BarHub eval = new();
        BarHub mrkt = new();
        BetaHub observer = eval.ToBetaHub(mrkt, LookbackPeriods, Type);

        eval.Add(evalBars);
        mrkt.Add(mrktBars);

        observer.Results.IsExactly(evalBars.ToBeta(mrktBars, LookbackPeriods, Type));
        observer.Results[201].ReturnsEval.Should().Be(0);
        observer.Results[300 + LookbackPeriods].Beta.Should().BeNull();

        eval.EndTransmission();
        mrkt.EndTransmission();
    }

    [TestMethod]
    public void Unsubscribe_ReleasesBothProviders()
    {
        BarHub eval = new();
        BarHub mrkt = new();
        BetaHub observer = eval.ToBetaHub(mrkt, LookbackPeriods, Type);

        eval.HasObservers.Should().BeTrue();
        mrkt.HasObservers.Should().BeTrue();

        observer.Unsubscribe();

        observer.IsSubscribed.Should().BeFalse();
        eval.HasObservers.Should().BeFalse();
        mrkt.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidArguments_Throw()
    {
        BarHub barHub = new();

        FluentActions.Invoking(() => ((IChainProvider<IReusable>)null!).ToBetaHub(barHub, LookbackPeriods, Type))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");
        FluentActions.Invoking(() => barHub.ToBetaHub(null!, LookbackPeriods, Type))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceMrkt");

        // a rejected lookback leaves both providers unsubscribed
        FluentActions.Invoking(() => barHub.ToBetaHub(barHub, 0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        barHub.HasObservers.Should().BeFalse();

        // so does a cache too small for the lookback window, which only the hub can check
        BarHub small = new(LookbackPeriods);
        BarHub other = new();
        FluentActions.Invoking(() => small.ToBetaHub(other, LookbackPeriods))
            .Should().ThrowExactly<ArgumentOutOfRangeException>();
        small.HasObservers.Should().BeFalse();
        other.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        barHub.ToBetaHub(barHub, LookbackPeriods, Type).ToString().Should().Be("BETA(20,ALL)");
    }
}
