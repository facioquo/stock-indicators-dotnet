namespace Observables;

/// <summary>
/// Covers the <c>InsertWithoutRebuild</c> branches of a root
/// <see cref="BarHub"/>: a late arrival that lands mid-cache is inserted
/// in place (no rebuild of the root's own cache), duplicates of that late
/// arrival are suppressed, and downstream observers rebuild to match the
/// corrected timeline exactly.
/// </summary>
[TestClass]
public class InsertWithoutRebuild : TestBase
{
    [TestMethod]
    public void LateArrival_MidCache_InsertsInPlace()
    {
        List<Bar> bars = Bars.Take(20).ToList();

        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(5);

        // skip index 10, then deliver it late
        for (int i = 0; i < bars.Count; i++)
        {
            if (i == 10)
            {
                continue;
            }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[10]);

        // root cache is chronological and complete
        barHub.Results.Should().HaveCount(bars.Count);
        barHub.Results[10].Timestamp.Should().Be(bars[10].Timestamp);

        // observer matches the batch series exactly
        observer.Results.IsExactly(bars.ToSma(5));
    }

    [TestMethod]
    public void LateArrival_DuplicateResend_IsSuppressed()
    {
        List<Bar> bars = Bars.Take(20).ToList();

        BarHub barHub = new();

        for (int i = 0; i < bars.Count; i++)
        {
            if (i == 10)
            {
                continue;
            }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[10]);
        barHub.Add(bars[10]); // duplicate of the same late arrival

        barHub.Results.Should().HaveCount(bars.Count);
        barHub.IsFaulted.Should().BeFalse();
    }

    [TestMethod]
    public void LateArrival_FirstPosition_InsertsAtFront()
    {
        List<Bar> bars = Bars.Take(15).ToList();

        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(5);

        // withhold the earliest bar, then deliver it late;
        // note: a root BarHub rejects arrivals older than its first
        // cached item, so the late arrival here is the second bar
        for (int i = 0; i < bars.Count; i++)
        {
            if (i == 1)
            {
                continue;
            }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[1]);

        barHub.Results.Should().HaveCount(bars.Count);
        barHub.Results[1].Timestamp.Should().Be(bars[1].Timestamp);

        observer.Results.IsExactly(bars.ToSma(5));
    }

    [TestMethod]
    public void BatchAdd_UnsortedInput_IsSortedStably()
    {
        List<Bar> bars = Bars.Take(30).ToList();

        // shuffle deterministically
        List<Bar> shuffled = [.. bars];
        (shuffled[3], shuffled[22]) = (shuffled[22], shuffled[3]);
        (shuffled[8], shuffled[15]) = (shuffled[15], shuffled[8]);

        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(5);

        barHub.Add(shuffled);

        barHub.Results.Should().HaveCount(bars.Count);
        observer.Results.IsExactly(bars.ToSma(5));
    }

    [TestMethod]
    public void BatchAdd_SortedInput_FastPathMatchesSeries()
    {
        List<Bar> bars = Bars.Take(30).ToList();

        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(5);

        barHub.Add(bars);

        barHub.Results.Should().HaveCount(bars.Count);
        observer.Results.IsExactly(bars.ToSma(5));
    }
}
