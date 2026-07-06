namespace Observables;

[TestClass]
public class CacheUtilities : TestBase
{
    [TestMethod]
    public void ClearCacheByTimestamp()
    {
        // RemoveRange is a root-hub mutation; exercise it on the root BarHub.

        IReadOnlyList<Bar> barsList = Bars
            .Take(10)
            .ToList();

        int length = barsList.Count;

        BarHub barHub = new();

        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        Bar q3 = barsList[3];

        // act: clear cache from a timestamp (inclusive)
        barHub.RemoveRange(q3.Timestamp, notify: false);

        // assert: entries at/after q3 are gone
        barHub.Cache.Should().HaveCount(3);

        List<IBar> cacheOver
            = barHub.Results
                .Where(c => c.Timestamp >= q3.Timestamp).ToList();

        List<IBar> cacheUndr
            = barHub.Results
                .Where(c => c.Timestamp < q3.Timestamp).ToList();

        cacheOver.Should().BeEmpty();
        cacheUndr.Should().HaveCount(3);
    }

    [TestMethod]
    public void ClearCacheByIndex()
    {
        // RemoveRange is a root-hub mutation; exercise it on the root BarHub.

        List<Bar> barsList = Bars
            .ToSortedList()
            .Take(10)
            .ToList();

        int length = barsList.Count;

        BarHub barHub = new();

        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        Bar q3 = barsList[3];

        // act: clear cache from an index (inclusive)
        barHub.RemoveRange(3, notify: true);

        // assert: entries at/after index 3 are gone
        barHub.Cache.Should().HaveCount(3);

        List<IBar> cacheOver
            = barHub.Results
                .Where(c => c.Timestamp >= q3.Timestamp).ToList();

        List<IBar> cacheUndr
            = barHub.Results
                .Where(c => c.Timestamp < q3.Timestamp).ToList();

        cacheOver.Should().BeEmpty();
        cacheUndr.Should().HaveCount(3);
    }

    [TestMethod]
    public void GetIndex()
    {
        // setup bar provider hub

        IReadOnlyList<Bar> barsList = Bars
            .Take(10)
            .ToList();

        int length = barsList.Count;

        BarHub barHub = new();

        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // find position of bar
        Bar q = barsList[4];

        int itemIndexEx = barHub.Cache.IndexOf(q, true);
        int timeIndexEx = barHub.Cache.IndexOf(q.Timestamp, true);

        // assert: same index
        itemIndexEx.Should().Be(4);
        timeIndexEx.Should().Be(4);

        // out of range (exceptions)
        Bar o = Bars[10];

        Assert.ThrowsExactly<ArgumentException>(
            () => barHub.Cache.IndexOf(o, true));

        Assert.ThrowsExactly<ArgumentException>(
            () => barHub.Cache.IndexOf(o.Timestamp, true));

        // out of range (no exceptions)
        int itemIndexNo = barHub.Cache.IndexOf(o, false);
        int timeIndexNo = barHub.Cache.IndexOf(o.Timestamp, false);

        itemIndexNo.Should().Be(-1);
        timeIndexNo.Should().Be(-1);

        int timeInsertOut = barHub.Cache.IndexGte(o.Timestamp);
        int timeInsertIn = barHub.Cache.IndexGte(barsList[2].Timestamp);

        timeInsertOut.Should().Be(-1);
        timeInsertIn.Should().Be(2);
    }

    [TestMethod]
    public void TryFindIndex()
    {

        // setup bar provider hub

        List<Bar> barsList = Bars
            .ToSortedList()
            .Take(10)
            .ToList();

        int length = barsList.Count;

        BarHub barHub = new();

        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        Bar q = barsList[4];

        // act: find index of bar

        // assert: correct index
        if (barHub.Cache.TryFindIndex(q.Timestamp, out int goodIndex))
        {
            goodIndex.Should().Be(4);
        }
        else
        {
            Assert.Fail("index not found");
        }

        // assert: out of range
        if (barHub.Cache.TryFindIndex(DateTime.MaxValue, out int badIndex))
        {
            Assert.Fail("unexpected index found");
        }
        else
        {
            badIndex.Should().Be(-1);
        }
    }
}
