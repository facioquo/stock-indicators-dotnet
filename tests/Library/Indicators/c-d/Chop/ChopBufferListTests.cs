namespace BufferLists;

[TestClass]
public class ChopBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const int lookbackPeriods = 14;

    private static readonly IReadOnlyList<ChopResult> series
       = Bars.ToChop(lookbackPeriods);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        ChopList sut = [with(lookbackPeriods)];

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
        ChopList sut = new(lookbackPeriods) { Bars };

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ChopList sut = new(lookbackPeriods, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Results_AreAlwaysBounded()
    {
        ChopList sut = new(14, Bars);
        sut.IsBetween(static x => x.Chop, 0, 100);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ChopResult> expected = subset.ToChop(lookbackPeriods);

        ChopList sut = new(lookbackPeriods, subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        ChopList sut = new(lookbackPeriods) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ChopResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public void AddBars_UnevenPrices_MatchesSeriesExactly()
    {
        // the window sum must round as the Series does, newest value first (#2446);
        // prices with many decimal places make the summation order visible
        Random random = new(7);
        decimal price = 100m;
        List<Bar> bars = [];

        for (int i = 0; i < 1500; i++)
        {
            decimal open = price;
            decimal close = Math.Max(1m, open + ((decimal)(random.NextDouble() - 0.5) * 3m));
            decimal high = Math.Max(open, close) + (decimal)random.NextDouble();
            decimal low = Math.Min(open, close) - (decimal)random.NextDouble();
            bars.Add(new Bar(new DateTime(2020, 1, 1).AddDays(i), open, high, low, close, 1000 + random.Next(500)));
            price = close;
        }

        ChopList sut = new(lookbackPeriods, bars);

        sut.IsExactly(bars.ToChop(lookbackPeriods));
    }
}
