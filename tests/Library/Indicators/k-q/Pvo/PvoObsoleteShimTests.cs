namespace StaticSeries;

/// <summary>
/// Tests for the obsolete v3 <c>GetPvo</c> shim, which must return what the pre-3.0.0
/// API returned.
/// </summary>
/// <remarks>
/// The shim declared <c>9, 12, 9</c> where both v2 and <c>ToPvo</c> declare
/// <c>12, 26, 9</c> — the correct values shifted one position. Calling the documented
/// default therefore computed a different indicator, silently: the result is a
/// well-formed PVO, just not the one asked for. Nothing here called the shim, so
/// nothing caught it.
/// </remarks>
[TestClass]
public class PvoObsoleteShimTests : TestBaseWithPrecision
{
    [TestMethod]
    public void GetPvoDefaultsMatchToPvo()
    {
#pragma warning disable CS0618 // exercising the obsolete shim is the point
        List<PvoResult> shim = Bars.GetPvo().ToList();
#pragma warning restore CS0618

        IReadOnlyList<PvoResult> expected = Bars.ToPvo();

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.Pvo).Should().Equal(expected.Select(static r => r.Pvo));
        shim.Select(static r => r.Signal).Should().Equal(expected.Select(static r => r.Signal));
        shim.Select(static r => r.Histogram).Should().Equal(expected.Select(static r => r.Histogram));
    }

    [TestMethod]
    public void GetPvoDefaultsAreNotTheShiftedPeriods()
    {
        // guards the specific 3.0.0 regression: the shim declared 9, 12, 9
#pragma warning disable CS0618
        List<PvoResult> shim = Bars.GetPvo().ToList();
#pragma warning restore CS0618

        IReadOnlyList<PvoResult> shifted = Bars.ToPvo(9, 12, 9);

        shim[^1].Pvo.Should().NotBeApproximately(shifted[^1].Pvo!.Value, Money6);
    }

    [TestMethod]
    public void GetPvoMatchesKnownValues()
    {
        // anchors the shim to absolute values, so a co-regression in ToPvo cannot
        // move both sides of the comparison above and pass unnoticed
#pragma warning disable CS0618
        List<PvoResult> shim = Bars.GetPvo().ToList();
#pragma warning restore CS0618

        shim.Should().HaveCount(502);
        shim[501].Pvo.Should().BeApproximately(10.439509, Money6);
    }

    [TestMethod]
    public void GetPvoWithExplicitPeriodsMatchesToPvo()
    {
#pragma warning disable CS0618
        List<PvoResult> shim = Bars.GetPvo(10, 20, 7).ToList();
#pragma warning restore CS0618

        IReadOnlyList<PvoResult> expected = Bars.ToPvo(10, 20, 7);

        shim.Select(static r => r.Pvo).Should().Equal(expected.Select(static r => r.Pvo));
    }
}
