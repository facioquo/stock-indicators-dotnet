namespace Observables;

[TestClass]
public class StreamObservers : TestBase
{
    [TestMethod]
    public void RebuildCache()
    {
        const int qtyBars = 5000;

        // setup: many random bars (massive)
        IReadOnlyList<Bar> barsList
            = Data.GetRandom(qtyBars).ToList();

        int length = barsList.Count;

        length.Should().Be(qtyBars); // check rando

        BarHub barHub = new();

        BarPartHub observer = barHub
            .ToBarPartHub(CandlePart.Close);

        for (int i = 0; i < length; i++)
        {
            barHub.Add(barsList[i]);
        }

        // original results
        IReadOnlyList<TimeValue> original = observer.Results.ToList();

        // bars to replace
        Bar q1000original = barsList[1000] with { /* copy */ };
        TimeValue r1000original = observer.Cache[1000] with { /* copy */ };

        // modify results (keeping barHub intact)
        Bar q1000modified = barsList[1000] with { Close = 12345m };
        TimeValue r1000modified = q1000modified.ToBarPart(CandlePart.Close);

        observer.Cache.Insert(1000, r1000modified); // add directly to cache

        IReadOnlyList<TimeValue> modified = observer.Results.ToList();

        // precondition: prefilled, modified
        barHub.Cache.Should().HaveCount(length);
        observer.Cache.Should().HaveCount(length + 1);

        observer.Cache[1000].Value.Should().Be(12345);
        observer.Cache.Should().NotBeEquivalentTo(original);
        observer.Cache.IsExactly(modified);

        // act: Rebuild()
        observer.Rebuild();

        // assert: restored to original
        observer.Results.Should().HaveCount(length);
        observer.Results.IsExactly(original);

        observer.Cache[1000].Value.Should().NotBe(12345);
        observer.Cache[1000].Value.Should().Be((double)barsList[1000].Close);
    }
}
