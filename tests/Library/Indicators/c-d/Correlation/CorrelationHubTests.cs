namespace StreamHubs;

[TestClass]
public class CorrelationHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int LookbackPeriods = 20;
    private const int SmaPeriods = 5;

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        CorrelationHub observer = barHub.ToSmaHub(SmaPeriods).ToCorrelationHub(barHub, LookbackPeriods);
        IReadOnlyList<CorrResult> sut = observer.Results;

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
        sut.IsExactly(Bars.ToSma(SmaPeriods).ToCorrelation(Bars, LookbackPeriods));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToCorrelation(RevisedBars, LookbackPeriods));
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
        IReadOnlyList<CorrResult> expected = bars.ToSma(SmaPeriods)
            .ToCorrelation(bars, LookbackPeriods)
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        CorrelationHub observer = barHub.ToSmaHub(SmaPeriods).ToCorrelationHub(barHub, LookbackPeriods);

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
        CorrelationHub observer = barHub.ToSmaHub(SmaPeriods).ToCorrelationHub(barHub.ToEmaHub(emaPeriods), LookbackPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<CorrResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToCorrelation(RevisedBars.ToEma(emaPeriods), LookbackPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = barHub.ToSmaHub(SmaPeriods).ToCorrelationHub(barHub, LookbackPeriods).ToEmaHub(emaPeriods);

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
        sut.IsExactly(RevisedBars.ToSma(SmaPeriods).ToCorrelation(RevisedBars, LookbackPeriods).ToEma(emaPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SeparateProviders_LateArrivalOnEachSide_MatchesSeriesExactly()
    {
        BarHub sourceA = new();
        BarHub sourceB = new();
        CorrelationHub observer = sourceA.ToCorrelationHub(sourceB, LookbackPeriods);

        for (int i = 0; i < barsCount; i++)
        {
            // each side skips a different bar (add later)
            if (i != 80) { sourceA.Add(OtherBars[i]); }

            if (i != 120) { sourceB.Add(Bars[i]); }
        }

        observer.Results.Should().HaveCount(barsCount - 2);

        sourceA.Add(OtherBars[80]);
        sourceB.Add(Bars[120]);

        observer.Results.IsExactly(OtherBars.ToCorrelation(Bars, LookbackPeriods));

        // a removal on series B drops that pair; the Series method needs both sides revised
        sourceB.RemoveAt(removeAtIndex);
        List<Bar> revisedA = [.. OtherBars];
        revisedA.RemoveAt(removeAtIndex);

        observer.Results.Should().HaveCount(barsCount - 1);
        observer.Results.IsExactly(revisedA.ToCorrelation(RevisedBars, LookbackPeriods));

        observer.Unsubscribe();
        sourceA.EndTransmission();
        sourceB.EndTransmission();
    }

    [TestMethod]
    public void FlatWindow_GivesNullCorrelation_MatchesSeriesExactly()
    {
        // a flat run on series A zeroes its variance, so the divisor is 0
        List<Bar> barsA = [.. OtherBars];
        for (int i = 200; i < 200 + LookbackPeriods; i++)
        {
            barsA[i] = barsA[i] with { Close = 100m };
        }

        BarHub sourceA = new();
        BarHub sourceB = new();
        CorrelationHub observer = sourceA.ToCorrelationHub(sourceB, LookbackPeriods);

        sourceA.Add(barsA);
        sourceB.Add(Bars);

        observer.Results.IsExactly(barsA.ToCorrelation(Bars, LookbackPeriods));
        observer.Results[200 + LookbackPeriods - 1].Correlation.Should().BeNull();

        sourceA.EndTransmission();
        sourceB.EndTransmission();
    }

    [TestMethod]
    public void Unsubscribe_ReleasesBothProviders()
    {
        BarHub sourceA = new();
        BarHub sourceB = new();
        CorrelationHub observer = sourceA.ToCorrelationHub(sourceB, LookbackPeriods);

        observer.Unsubscribe();

        observer.IsSubscribed.Should().BeFalse();
        sourceA.HasObservers.Should().BeFalse();
        sourceB.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_InvalidArguments_Throw()
    {
        BarHub barHub = new();

        FluentActions.Invoking(() => ((IChainProvider<IReusable>)null!).ToCorrelationHub(barHub, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceA");
        FluentActions.Invoking(() => barHub.ToCorrelationHub(null!, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceB");

        // a rejected lookback leaves both providers unsubscribed
        FluentActions.Invoking(() => barHub.ToCorrelationHub(barHub, 0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        barHub.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        barHub.ToCorrelationHub(barHub, LookbackPeriods).ToString().Should().Be("CORR(20)");
    }
}
