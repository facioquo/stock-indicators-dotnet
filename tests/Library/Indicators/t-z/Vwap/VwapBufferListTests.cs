namespace BufferLists;

[TestClass]
public class VwapBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private static readonly DateTime startDate = DateTime.Parse("2018-12-31", invariantCulture);

    private static readonly IReadOnlyList<VwapResult> series
       = Bars.ToVwap(startDate);

    private static readonly IReadOnlyList<VwapResult> seriesDefault
       = Bars.ToVwap();

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        VwapList sut = new(startDate);

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
        VwapList sut = Bars.ToVwapList(startDate);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        VwapList sut = new(startDate, Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void DefaultStartDate_OnExtensionInvocation_IncrementsResults()
    {
        VwapList sut = Bars.ToVwapList();

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(seriesDefault);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<VwapResult> expected = subset.ToVwap(startDate);

        VwapList sut = new(startDate, subset);

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

        VwapList sut = new(startDate) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<VwapResult> expected = series
            .Skip(series.Count - maxListSize)
            .ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }
}
