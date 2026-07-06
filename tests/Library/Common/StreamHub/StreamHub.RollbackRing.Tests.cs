namespace Observables;

/// <summary>
/// Verifies the O(1) near-tail rollback fast path (RollbackRing snapshots)
/// and its deep-rollback fallback produce Series-exact results for stateful
/// hubs. RSI is the reference implementation; shallow corrections restore
/// state from ring snapshots while corrections deeper than the ring's
/// capacity fall back to full state replay.
/// </summary>
[TestClass]
public class RollbackRingState : TestBase
{
    [TestMethod]
    public void ShallowCorrection_RestoresFromSnapshot_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(120).ToList();

        BarHub barHub = new();
        RsiHub observer = barHub.ToRsiHub(14);

        barHub.Add(bars);

        // correct a recent bar (well within ring capacity)
        Bar corrected = bars[117] with { Close = bars[117].Close + 2m };
        bars[117] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToRsi(14));
    }

    [TestMethod]
    public void DeepCorrection_FallsBackToReplay_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(200).ToList();

        BarHub barHub = new();
        RsiHub observer = barHub.ToRsiHub(14);

        barHub.Add(bars);

        // correct a bar far beyond the ring capacity (32 snapshots)
        Bar corrected = bars[60] with { Close = bars[60].Close + 2m };
        bars[60] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToRsi(14));
    }

    [TestMethod]
    public void FormingBarChurn_RepeatedTailCorrections_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(80).ToList();

        BarHub barHub = new();
        RsiHub observer = barHub.ToRsiHub(14);

        barHub.Add(bars);

        // emulate an aggregator re-sending the forming bar on every tick;
        // more updates than the ring capacity, all on one timestamp
        Bar formingBar = bars[^1];
        for (int update = 1; update <= 40; update++)
        {
            formingBar = formingBar with { Close = formingBar.Close + 0.25m };
            barHub.Add(formingBar);
        }

        bars[^1] = formingBar;
        observer.Results.IsExactly(bars.ToRsi(14));
    }

    [TestMethod]
    public void SequentialCorrections_AcrossRecentBars_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(100).ToList();

        BarHub barHub = new();
        RsiHub observer = barHub.ToRsiHub(14);

        barHub.Add(bars);

        // several distinct recent bars corrected in sequence: each rollback
        // restores from a snapshot recorded by the previous replay
        for (int i = 95; i < 100; i++)
        {
            Bar corrected = bars[i] with { Close = bars[i].Close + 1m };
            bars[i] = corrected;
            barHub.Add(corrected);
        }

        observer.Results.IsExactly(bars.ToRsi(14));
    }

    [TestMethod]
    public void ShallowCorrection_AdxHub_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(150).ToList();

        BarHub barHub = new();
        AdxHub observer = barHub.ToAdxHub(14);

        barHub.Add(bars);

        Bar corrected = bars[147] with { Close = bars[147].Close + 2m };
        bars[147] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToAdx(14));
    }

    [TestMethod]
    public void ShallowCorrection_SuperTrendHub_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(150).ToList();

        BarHub barHub = new();
        SuperTrendHub observer = barHub.ToSuperTrendHub(10, 3);

        barHub.Add(bars);

        Bar corrected = bars[147] with { Close = bars[147].Close + 2m };
        bars[147] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToSuperTrend(10, 3));
    }

    [TestMethod]
    public void ShallowCorrection_KvoHub_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(150).ToList();

        BarHub barHub = new();
        KvoHub observer = barHub.ToKvoHub(34, 55, 13);

        barHub.Add(bars);

        Bar corrected = bars[147] with { Close = bars[147].Close + 2m };
        bars[147] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToKvo(34, 55, 13));
    }

    [TestMethod]
    public void ShallowCorrection_ParabolicSarHub_MatchesSeries()
    {
        List<Bar> bars = Bars.Take(150).ToList();

        BarHub barHub = new();
        ParabolicSarHub observer = barHub.ToParabolicSarHub(0.02, 0.2);

        barHub.Add(bars);

        Bar corrected = bars[147] with { Close = bars[147].Close + 2m };
        bars[147] = corrected;
        barHub.Add(corrected);

        observer.Results.IsExactly(bars.ToParabolicSar(0.02, 0.2));
    }
}
