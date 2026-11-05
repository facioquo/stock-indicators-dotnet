namespace StreamHubs;

[TestClass]
public class PrsHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int LookbackPeriods = 20;
    private const int SmaPeriods = 5;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        PrsHub observer = barHub.ToSmaHub(SmaPeriods).ToPrsHub(barHub, LookbackPeriods);
        IReadOnlyList<PrsResult> sut = observer.Results;

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
        sut.IsExactly(Bars.ToSma(SmaPeriods).ToPrs(Bars, LookbackPeriods));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToPrs(RevisedBars, LookbackPeriods));
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
        IReadOnlyList<PrsResult> expected = bars.ToSma(SmaPeriods)
            .ToPrs(bars, LookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        PrsHub observer = barHub.ToSmaHub(SmaPeriods).ToPrsHub(barHub, LookbackPeriods);

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
        PrsHub observer = barHub.ToSmaHub(SmaPeriods).ToPrsHub(barHub.ToEmaHub(emaPeriods), LookbackPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<PrsResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToPrs(RevisedBars.ToEma(emaPeriods), LookbackPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = barHub.ToSmaHub(SmaPeriods).ToPrsHub(barHub, LookbackPeriods).ToEmaHub(emaPeriods);

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
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToPrs(RevisedBars, LookbackPeriods).ToEma(emaPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SeparateProviders_LateArrivalOnEachSide_MatchesSeriesExactly()
    {
        BarHub eval = new();
        BarHub baseHub = new();
        PrsHub observer = eval.ToPrsHub(baseHub, LookbackPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // each side skips a different bar (add later)
            if (i != 80) { eval.Add(OtherBars[i]); }

            if (i != 120) { baseHub.Add(Bars[i]); }
        }

        observer.Results.Should().HaveCount(barsCount - 2);

        eval.Add(OtherBars[80]);
        baseHub.Add(Bars[120]);

        observer.Results.IsExactly(OtherBars.ToPrs(Bars, LookbackPeriods));

        // a removal on the base side drops that pair; the Series method needs both sides revised
        baseHub.RemoveAt(removeAtIndex);
        List<Bar> evalRevised = [.. OtherBars];
        evalRevised.RemoveAt(removeAtIndex);

        observer.Results.Should().HaveCount(barsCount - 1);
        observer.Results.IsExactly(evalRevised.ToPrs(RevisedBars, LookbackPeriods));

        observer.Unsubscribe();
        eval.EndTransmission();
        baseHub.EndTransmission();
    }

    [TestMethod]
    public void WithoutLookback_MatchesSeriesExactly()
    {
        BarHub eval = new();
        BarHub baseHub = new();
        PrsHub observer = eval.ToPrsHub(baseHub);

        eval.Add(OtherBars);
        baseHub.Add(Bars);

        observer.LookbackPeriods.Should().Be(0);
        observer.Results.IsExactly(OtherBars.ToPrs(Bars));
        observer.Results.Should().OnlyContain(static x => x.PrsPercent == null);

        eval.EndTransmission();
        baseHub.EndTransmission();
    }

    [TestMethod]
    public void ZeroValues_MatchesSeriesExactly()
    {
        // a 0 base gives a null PRS; a 0 earlier value on either side gives a null PRS%
        List<Bar> evalBars = [.. OtherBars];
        List<Bar> baseBars = [.. Bars];
        evalBars[200] = evalBars[200] with { Open = 0, High = 0, Low = 0, Close = 0 };
        baseBars[300] = baseBars[300] with { Open = 0, High = 0, Low = 0, Close = 0 };

        BarHub eval = new();
        BarHub baseHub = new();
        PrsHub observer = eval.ToPrsHub(baseHub, LookbackPeriods);

        eval.Add(evalBars);
        baseHub.Add(baseBars);

        observer.Results.IsExactly(evalBars.ToPrs(baseBars, LookbackPeriods));
        observer.Results[300].Prs.Should().BeNull();
        observer.Results[200 + LookbackPeriods].PrsPercent.Should().BeNull();
        observer.Results[300 + LookbackPeriods].PrsPercent.Should().BeNull();

        eval.EndTransmission();
        baseHub.EndTransmission();
    }

    [TestMethod]
    public void Unsubscribe_ReleasesBothProviders()
    {
        BarHub eval = new();
        BarHub baseHub = new();
        PrsHub observer = eval.ToPrsHub(baseHub, LookbackPeriods);

        eval.HasObservers.Should().BeTrue();
        baseHub.HasObservers.Should().BeTrue();

        observer.Unsubscribe();

        observer.IsSubscribed.Should().BeFalse();
        eval.HasObservers.Should().BeFalse();
        baseHub.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidArguments_Throw()
    {
        BarHub barHub = new();

        FluentActions.Invoking(() => ((IChainProvider<IReusable>)null!).ToPrsHub(barHub, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");
        FluentActions.Invoking(() => barHub.ToPrsHub(null!, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceBase");
        FluentActions.Invoking(() => barHub.ToPrsHub(null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceBase");

        // a rejected lookback leaves both providers unsubscribed
        FluentActions.Invoking(() => barHub.ToPrsHub(barHub, 0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        barHub.HasObservers.Should().BeFalse();

        // so does a cache too small for the lookback window, which only the hub can check
        BarHub small = new(LookbackPeriods);
        BarHub other = new();
        FluentActions.Invoking(() => small.ToPrsHub(other, LookbackPeriods))
            .Should().ThrowExactly<ArgumentOutOfRangeException>();
        small.HasObservers.Should().BeFalse();
        other.HasObservers.Should().BeFalse();

        // the largest lookback fails the same check rather than overflowing
        FluentActions.Invoking(() => other.ToPrsHub(small, int.MaxValue))
            .Should().ThrowExactly<ArgumentOutOfRangeException>();
        other.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        barHub.ToPrsHub(barHub, LookbackPeriods).ToString().Should().Be("PRS(20)");
        barHub.ToPrsHub(barHub).ToString().Should().Be("PRS");
    }
}
