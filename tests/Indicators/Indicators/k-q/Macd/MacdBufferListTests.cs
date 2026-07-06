namespace BufferLists;

[TestClass]
public class MacdBufferListTests : BufferListTestBase, ITestChainBufferList
{
    private const int fastPeriods = 12;
    private const int slowPeriods = 26;
    private const int signalPeriods = 9;

    private static readonly IReadOnlyList<IReusable> reusables
       = Bars
        .Cast<IReusable>()
        .ToList();

    private static readonly IReadOnlyList<MacdResult> series
       = Bars.ToMacd(fastPeriods, slowPeriods, signalPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (Bar bar in Bars)
        {
            sut.Add(bar);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        MacdList sut = Bars.ToMacdList(fastPeriods, slowPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItem_IncrementsResults()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddReusableItemBatch_IncrementsResults()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods) { reusables };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddDateAndValue_IncrementsResults()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (IReusable item in reusables)
        {
            sut.Add(item.Timestamp, item.Value);
        }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<MacdResult> expected = subset.ToMacd(fastPeriods, slowPeriods, signalPeriods);

        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void Extension_FromBars_ReturnsExpectedResult()
    {
        MacdList sut = Bars.ToMacdList(fastPeriods, slowPeriods, signalPeriods);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void StreamingAccuracy_PartialBars_MatchesSeriesExactly()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (Bar bar in Bars.Take(100))
        {
            sut.Add(bar);
        }

        MacdResult streamResult = sut[50];
        MacdResult seriesResult = series[50];

        streamResult.Should().Be(seriesResult);
        sut.Should().HaveCount(100);
        sut.IsExactly(series.Take(100));
    }

    [TestMethod]
    public void RealTimeSimulation_WithIncrementalBars_MatchesSeriesExactly()
    {
        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods);

        foreach (Bar bar in Bars.Take(100))
        {
            sut.Add(bar);
        }

        foreach (Bar bar in Bars.Skip(100).Take(10))
        {
            MacdResult previous = sut[^1];

            sut.Add(bar);

            MacdResult current = sut[^1];

            if (current.Macd.HasValue)
            {
                current.Macd.Should().NotBeNull();
                current.FastEma.Should().NotBeNull();
                current.SlowEma.Should().NotBeNull();
            }

            current.Timestamp.Should().BeAfter(previous.Timestamp);
        }
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        MacdList sut = new(fastPeriods, slowPeriods, signalPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<MacdResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
