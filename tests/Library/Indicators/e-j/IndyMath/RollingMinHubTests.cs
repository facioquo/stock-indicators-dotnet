namespace StreamHubs;

[TestClass]
public class RollingMinHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const int lookbackPeriods = 20;

    private const string ExpectedName = "ROLLINGMIN(20)";

    private static RollingMinHub NewHub(IChainProvider<IReusable> provider)
        => provider.ToRollingMinHub(lookbackPeriods);

    private static IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source)
        => source.RollingMin(lookbackPeriods);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        RollingMinHub observer = NewHub(barHub);
        IReadOnlyList<MathResult> sut = observer.Results;

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
        sut.IsExactly(Series(Bars));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(Series(RevisedBars));
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
        IReadOnlyList<MathResult> expected = Series(bars)
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        RollingMinHub observer = NewHub(barHub);

        barHub.Add(bars);

        barHub.Bars.Should().HaveCount(maxCacheSize);
        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Results.IsExactly(expected);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void WithCachePruning_CorrectionWithinRetainedWindow_MatchesSeriesExactly()
    {
        // the closest correction to the pruned head whose replay window is still fully
        // retained; nearer corrections hit the documented rollback-after-prune limit
        const int maxCacheSize = 50;
        const int totalBars = 100;
        const int retainedOffset = 20;

        List<Bar> bars = [.. Bars.Take(totalBars)];

        BarHub barHub = new(maxCacheSize);
        RollingMinHub observer = NewHub(barHub);
        barHub.Add(bars);

        const int correctionIndex = totalBars - maxCacheSize + retainedOffset;
        Bar original = bars[correctionIndex];
        Bar corrected = original with {
            Open = original.Open * 1.05m,
            High = original.High * 1.05m,
            Low = original.Low * 1.05m,
            Close = original.Close * 1.05m
        };

        barHub.Add(corrected);
        bars[correctionIndex] = corrected;

        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Results.IsExactly(Series(bars).TakeLast(maxCacheSize).ToList());

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        const int smaPeriods = 5;

        // SMA warmup feeds NaN values into the hub under test
        BarHub barHub = new();
        RollingMinHub observer = NewHub(barHub.ToSmaHub(smaPeriods));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<MathResult> sut = observer.Results;
        sut.Should().HaveCount(barsCount - 1);
        sut.IsExactly(Series(RevisedBars.ToSma(smaPeriods)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = NewHub(barHub).ToEmaHub(emaPeriods);

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
        sut.IsExactly(Series(RevisedBars).ToEma(emaPeriods));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void NaNInRollbackWindow_LateArrivalAndRemoval_MatchesSeriesExactly()
    {
        // zero bars make ROC(1) emit NaN mid-series: single, consecutive, and late gaps
        int[] zeroes = [30, 61, 62, 63, 200];
        List<Bar> bars = [.. Bars.Select((b, i) => zeroes.Contains(i)
            ? b with { Open = 0, High = 0, Low = 0, Close = 0 }
            : b)];

        BarHub barHub = new();
        RollingMinHub observer = NewHub(barHub.ToRocHub(1));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one just after the NaN run, so its rebuild window replays the NaNs
            if (i == 66) { continue; }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[66]);
        observer.Results.IsExactly(Series(bars.ToRoc(1)));

        // removal whose replay window also holds the NaN run
        barHub.RemoveAt(70);
        bars.RemoveAt(70);
        observer.Results.IsExactly(Series(bars.ToRoc(1)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampCorrection_BeforeWindowFills_MatchesSeriesExactly()
    {
        // correcting an early bar rolls back to a restore index below the lookback
        const int correctionIndex = 5;

        List<Bar> bars = [.. Bars];
        Bar original = bars[correctionIndex];
        Bar corrected = original with {
            Open = original.Open * 1.05m,
            High = original.High * 1.05m,
            Low = original.Low * 1.05m,
            Close = original.Close * 1.05m
        };

        BarHub barHub = new();
        RollingMinHub observer = NewHub(barHub);

        barHub.Add(bars);
        barHub.Add(corrected);

        bars[correctionIndex] = corrected;
        observer.Results.Should().HaveCount(barsCount);
        observer.Results.IsExactly(Series(bars));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Exceptions_NullProvider_ThrowsArgumentNullException()
        => FluentActions.Invoking(() => NewHub(null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("chainProvider");

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        RollingMinHub hub = NewHub(new BarHub());
        hub.ToString().Should().Be(ExpectedName);
    }

    [TestMethod]
    public void Properties_AfterConstruction_ReflectArguments()
        => new BarHub().ToRollingMinHub(14).LookbackPeriods.Should().Be(14);

    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
    {
        FluentActions.Invoking(static () => new BarHub().ToRollingMinHub(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(static () => new BarHub(50).ToRollingMinHub(51))
            .Should().ThrowExactly<ArgumentOutOfRangeException>();
        new BarHub(50).ToRollingMinHub(50).LookbackPeriods.Should().Be(50);
    }
}
