using System.Reflection;

namespace StaticSeries;

/// <summary>
/// Tests for the obsolete v3 <c>GetPrs</c> shims, which must return what the pre-3.0.0
/// API returned.
/// </summary>
/// <remarks>
/// The shim exists only to keep v2 code working, so "matches <c>ToPrs</c>" is its whole
/// contract. Two defects shipped in 3.0.0 and survived because nothing here called it:
/// the evaluated and base series were passed in the wrong order, so every result was the
/// reciprocal of the ratio; and an unspecified lookback mapped to <c>0</c>, which
/// validation rejects, so the documented default threw instead of computing.
/// </remarks>
[TestClass]
public class PrsObsoleteShimTests : TestBaseWithPrecision
{
    [TestMethod]
    public void GetPrsMatchesToPrsRatioDirection()
    {
        const int lookbackPeriods = 20;

#pragma warning disable CS0618 // exercising the obsolete shim is the point
        IReadOnlyList<PrsResult> shim
            = Bars.GetPrs(OtherBars, lookbackPeriods).ToList();
#pragma warning restore CS0618

        IReadOnlyList<PrsResult> expected
            = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars, lookbackPeriods);

        shim.Select(static r => r.Prs).Should().Equal(expected.Select(static r => r.Prs),
            "GetPrs(quotesEval, quotesBase) evaluates the first series against the second, as it did before 3.0.0");

        shim.Select(static r => r.PrsPercent).Should().Equal(expected.Select(static r => r.PrsPercent));
    }

    [TestMethod]
    public void GetPrsIsNotTheInvertedRatio()
    {
        // guards the specific 3.0.0 regression: the arguments were swapped, so every
        // value came back as the reciprocal
        const int lookbackPeriods = 20;

#pragma warning disable CS0618
        List<PrsResult> shimResults = Bars.GetPrs(OtherBars, lookbackPeriods).ToList();
        double shim = shimResults[^1].Prs!.Value;
#pragma warning restore CS0618

        IReadOnlyList<PrsResult> invertedResults
            = ((IReadOnlyList<IReusable>)OtherBars).ToPrs(Bars, lookbackPeriods);
        double inverted = invertedResults[^1].Prs!.Value;

        shim.Should().BeApproximately(1 / inverted, Money6);
    }

    [TestMethod]
    public void GetPrsWithUnspecifiedLookbackComputesWithoutPercent()
    {
        // the shim's own default: v2 treated it as "no PrsPercent", not as an error
#pragma warning disable CS0618
        IReadOnlyList<PrsResult> shim = Bars.GetPrs(OtherBars).ToList();
#pragma warning restore CS0618

        IReadOnlyList<PrsResult> expected
            = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars);

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.Prs).Should().Equal(expected.Select(static r => r.Prs));
        shim.Should().OnlyContain(static r => r.PrsPercent == null);
    }

    [TestMethod]
    public void GetPrsFromTuplesMatchesToPrs()
    {
        IEnumerable<(DateTime d, double v)> evalTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        IEnumerable<(DateTime d, double v)> baseTuples
            = OtherBars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        IReadOnlyList<PrsResult> shim = evalTuples.GetPrs(baseTuples, 20).ToList();
#pragma warning restore CS0618

        IReadOnlyList<PrsResult> expected
            = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars, 20);

        shim.Select(static r => r.Prs).Should().Equal(expected.Select(static r => r.Prs));
    }

    [TestMethod]
    public void GetPrsFromTuplesWithUnspecifiedLookbackComputesWithoutPercent()
    {
        IEnumerable<(DateTime d, double v)> evalTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        IEnumerable<(DateTime d, double v)> baseTuples
            = OtherBars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        IReadOnlyList<PrsResult> shim = evalTuples.GetPrs(baseTuples).ToList();
#pragma warning restore CS0618

        IReadOnlyList<PrsResult> expected
            = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars);

        shim.Should().HaveCount(Bars.Count);
        shim.Select(static r => r.Prs).Should().Equal(expected.Select(static r => r.Prs));
        shim.Should().OnlyContain(static r => r.PrsPercent == null);
    }

    [TestMethod]
    public void GetPrsMatchesKnownValues()
    {
        // every other test here compares the shim against ToPrs, so a co-regression in
        // ToPrs would move both sides together; these anchor the shim to absolute values
        const int lookbackPeriods = 30;

#pragma warning disable CS0618
        List<PrsResult> shim = Bars.GetPrs(OtherBars, lookbackPeriods).ToList();
#pragma warning restore CS0618

        shim.Should().HaveCount(502);
        shim[8].Prs.Should().BeApproximately(0.902250, Money6);
        shim[249].Prs.Should().BeApproximately(0.818081, Money6);
        shim[249].PrsPercent.Should().BeApproximately(0.023089, Money6);
        shim[501].Prs.Should().BeApproximately(0.737019, Money6);
        shim[501].PrsPercent.Should().BeApproximately(-0.037082, Money6);
    }

    [TestMethod]
    public void GetPrsRejectsAnExplicitZeroLookback()
    {
        // v2 rejected <= 0; null was the "no PrsPercent" marker, not 0. This overload
        // still distinguishes the two, so an explicit 0 stays an error.
#pragma warning disable CS0618
        Action act = () => _ = Bars.GetPrs(OtherBars, 0).ToList();
#pragma warning restore CS0618

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void GetPrsRejectsANegativeLookback()
    {
#pragma warning disable CS0618
        Action act = () => _ = Bars.GetPrs(OtherBars, -5).ToList();
#pragma warning restore CS0618

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void GetPrsFromTuplesTreatsZeroAsUnspecified()
    {
        // pins the one place this shim knowingly diverges from v2, which rejected an
        // explicit 0: the 3.0.0 signature narrowed int? to int = 0, leaving 0 as the
        // only marker for "unspecified", and restoring the default has to honor it
        IEnumerable<(DateTime d, double v)> evalTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        IEnumerable<(DateTime d, double v)> baseTuples
            = OtherBars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        List<PrsResult> shim = evalTuples.GetPrs(baseTuples, 0).ToList();
#pragma warning restore CS0618

        shim.Should().HaveCount(Bars.Count);
        shim.Should().OnlyContain(static r => r.PrsPercent == null);
    }

    [TestMethod]
    public void ErrorLevelGetPrsOverloadAppliesTheSameLookbackMapping()
    {
        // the four-argument overload is Obsolete(.., true), so C# cannot call it at all
        // and CS0619 is not suppressible -- but reflection reaches it, and it carried
        // the same defective mapping, so it is verified the only way it can be
        MethodInfo overload = typeof(Prs).Assembly
            .GetType("FacioQuo.Stock.Indicators.Indicator")
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "GetPrs"
                      && m.GetParameters().Length == 4
                      && m.GetParameters()[0].ParameterType == typeof(IEnumerable<IBar>));

        object raw = overload.Invoke(null, [Bars, OtherBars, null, null]);
        List<PrsResult> shim = ((IEnumerable<PrsResult>)raw).ToList();

        shim.Should().HaveCount(Bars.Count);
        shim.Should().OnlyContain(static r => r.PrsPercent == null,
            "an unspecified lookback must compute no percent rather than throw");
        shim[^1].Prs.Should().BeApproximately(0.737019, Money6);
    }
}
