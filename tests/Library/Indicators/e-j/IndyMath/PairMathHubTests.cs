namespace StreamHubs;

[TestClass]
public class PairMathHubTests : StreamHubTestBase, ITestChainObserver, ITestChainProvider
{
    private const string ExpectedName = "MINUS-SERIES";
    private const int SmaPeriods = 5;

    private static PairMathHub NewHub(IChainProvider<IReusable> source, IChainProvider<IReusable> other)
        => source.Minus(other);

    private static IReadOnlyList<MathResult> Series(IReadOnlyList<IReusable> source, IReadOnlyList<IReusable> other)
        => source.Minus(other);

    [TestMethod]
    public void BarObserver_WithWarmupLateArrivalAndRemoval_MatchesSeriesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(20));

        PairMathHub observer = NewHub(barHub, barHub.ToSmaHub(SmaPeriods));
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
        sut.IsExactly(Series(Bars, Bars.ToSma(SmaPeriods)));

        // delete, should equal series (revised)
        barHub.RemoveAt(removeAtIndex);
        sut.IsExactly(Series(RevisedBars, RevisedBars.ToSma(SmaPeriods)));
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
        IReadOnlyList<MathResult> expected = Series(bars, bars.ToSma(SmaPeriods))
            .TakeLast(maxCacheSize)
            .ToList();

        BarHub barHub = new(maxCacheSize);
        PairMathHub observer = NewHub(barHub, barHub.ToSmaHub(SmaPeriods));

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
        // the replay window of this correction is still fully retained
        const int maxCacheSize = 50;
        const int totalBars = 100;
        const int retainedOffset = 20;

        List<Bar> bars = [.. Bars.Take(totalBars)];

        BarHub barHub = new(maxCacheSize);
        PairMathHub observer = NewHub(barHub, barHub.ToSmaHub(SmaPeriods));
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

        IReadOnlyList<Bar> originals = Bars.Take(totalBars).ToList();

        // the correction must change the retained output, or the replay goes unchecked
        Series(bars, bars.ToSma(SmaPeriods)).TakeLast(maxCacheSize)
            .Should().NotEqual(Series(originals, originals.ToSma(SmaPeriods)).TakeLast(maxCacheSize));

        observer.Results.Should().HaveCount(maxCacheSize);
        observer.Results.IsExactly(Series(bars, bars.ToSma(SmaPeriods)).TakeLast(maxCacheSize).ToList());

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainObserver_ChainedProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 10;

        // SMA and EMA warmup feed NaN values into the hub under test
        BarHub barHub = new();
        PairMathHub observer = NewHub(barHub.ToSmaHub(SmaPeriods), barHub.ToEmaHub(emaPeriods));

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
        sut.IsExactly(Series(RevisedBars.ToSma(SmaPeriods), RevisedBars.ToEma(emaPeriods)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void ChainProvider_MatchesSeriesExactly()
    {
        const int emaPeriods = 12;

        BarHub barHub = new();
        EmaHub observer = NewHub(barHub, barHub.ToSmaHub(SmaPeriods)).ToEmaHub(emaPeriods);

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
        sut.IsExactly(Series(RevisedBars, RevisedBars.ToSma(SmaPeriods)).ToEma(emaPeriods));

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
        PairMathHub observer = NewHub(barHub.ToRocHub(1), barHub.ToSmaHub(SmaPeriods));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one just after the NaN run, so its rebuild window replays the NaNs
            if (i == 66) { continue; }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[66]);
        observer.Results.IsExactly(Series(bars.ToRoc(1), bars.ToSma(SmaPeriods)));

        // removal whose replay window also holds the NaN run
        barHub.RemoveAt(70);
        bars.RemoveAt(70);
        observer.Results.IsExactly(Series(bars.ToRoc(1), bars.ToSma(SmaPeriods)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SameTimestampCorrection_BeforeWindowFills_MatchesSeriesExactly()
    {
        // correcting an early bar lands inside the SMA warmup on the other side
        const int correctionIndex = 2;

        List<Bar> bars = [.. Bars];
        Bar original = bars[correctionIndex];
        Bar corrected = original with {
            Open = original.Open * 1.05m,
            High = original.High * 1.05m,
            Low = original.Low * 1.05m,
            Close = original.Close * 1.05m
        };

        BarHub barHub = new();
        PairMathHub observer = NewHub(barHub, barHub.ToSmaHub(SmaPeriods));

        barHub.Add(bars);
        barHub.Add(corrected);

        bars[correctionIndex] = corrected;

        // the correction must change the output, or the replay goes unchecked
        Series(bars, bars.ToSma(SmaPeriods)).Should().NotEqual(Series(Bars, Bars.ToSma(SmaPeriods)));

        observer.Results.Should().HaveCount(barsCount);
        observer.Results.IsExactly(Series(bars, bars.ToSma(SmaPeriods)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Composite_FromDiscussion977_MatchesSeriesExactly()
    {
        // MYC = MACD − EMA(MACD, 9) + SMA(9), streamed from one bar hub
        BarHub barHub = new();
        MacdHub macd = barHub.ToMacdHub(12, 26, 9);
        PairMathHub myc = macd
            .Minus(macd.ToEmaHub(9))
            .Plus(barHub.ToSmaHub(9));

        for (int i = 0; i < barsCount; i++)
        {
            // skip one (add later)
            if (i == 80) { continue; }

            barHub.Add(Bars[i]);
        }

        barHub.Add(Bars[80]);
        barHub.RemoveAt(removeAtIndex);

        IReadOnlyList<MacdResult> expectedMacd = RevisedBars.ToMacd(12, 26, 9);
        IReadOnlyList<MathResult> expected = expectedMacd
            .Minus(expectedMacd.ToEma(9))
            .Plus(RevisedBars.ToSma(9));

        myc.Results.Should().HaveCount(barsCount - 1);
        myc.Results.IsExactly(expected);
        myc.Results.Should().Contain(static x => x.Result != null);

        myc.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void EveryOperation_StreamedBars_MatchesSeriesExactly()
    {
        // the other side holds a 0 that DividedBy must turn into null
        List<Bar> others = [.. OtherBars];
        others[300] = others[300] with { Open = 0, High = 0, Low = 0, Close = 0 };

        BarHub source = new();
        BarHub other = new();
        PairMathHub plus = source.Plus(other);
        PairMathHub minus = source.Minus(other);
        PairMathHub times = source.Times(other);
        PairMathHub divided = source.DividedBy(other);
        PairMathHub max = source.MaxWith(other);
        PairMathHub min = source.MinWith(other);

        source.Add(Bars);
        other.Add(others);

        plus.Results.IsExactly(Bars.Plus(others));
        minus.Results.IsExactly(Bars.Minus(others));
        times.Results.IsExactly(Bars.Times(others));
        divided.Results.IsExactly(Bars.DividedBy(others));
        max.Results.IsExactly(Bars.MaxWith(others));
        min.Results.IsExactly(Bars.MinWith(others));

        divided.Results[300].Result.Should().BeNull();
        divided.Operation.Should().Be(PairOperation.DividedBy);

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void SameProvider_GivesFlatlineResults()
    {
        BarHub barHub = new();
        PairMathHub minus = barHub.Minus(barHub);
        PairMathHub divided = barHub.DividedBy(barHub);

        barHub.Add(Bars);

        minus.Results.Should().HaveCount(barsCount).And.OnlyContain(static x => x.Result == 0);
        divided.Results.Should().HaveCount(barsCount).And.OnlyContain(static x => x.Result == 1);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void UnmatchedTimestamp_NoResultUntilLateArrival()
    {
        const int skip = 50;
        List<Bar> withGap = [.. Bars];
        withGap.RemoveAt(skip);

        BarHub source = new();
        BarHub other = new();
        PairMathHub observer = source.Plus(other);

        source.Add(Bars);
        other.Add(withGap);

        observer.Results.Should().HaveCount(barsCount - 1);
        observer.Results.Should().NotContain(x => x.Timestamp == Bars[skip].Timestamp);
        observer.Results.IsExactly(withGap.Plus(withGap));

        // the late value is paired through the usual rebuild
        other.Add(Bars[skip]);
        observer.Results.IsExactly(Bars.Plus(Bars));

        source.EndTransmission();
        other.EndTransmission();
    }

    [TestMethod]
    public void Unsubscribe_ReleasesBothProviders()
    {
        BarHub source = new();
        BarHub other = new();
        PairMathHub observer = source.Times(other);

        observer.Unsubscribe();

        observer.IsSubscribed.Should().BeFalse();
        source.HasObservers.Should().BeFalse();
        other.HasObservers.Should().BeFalse();
    }

    [TestMethod]
    public void Exceptions_NullProvider_ThrowsArgumentNullException()
    {
        BarHub barHub = new();

        FluentActions.Invoking(() => NewHub(null!, barHub))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("source");
        FluentActions.Invoking(() => NewHub(barHub, null!))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("other");
    }

    [TestMethod]
    public override void ToStringOverride_ReturnsExpectedName()
    {
        BarHub barHub = new();
        PairMathHub hub = NewHub(barHub, barHub);
        hub.ToString().Should().Be(ExpectedName);
    }
}
