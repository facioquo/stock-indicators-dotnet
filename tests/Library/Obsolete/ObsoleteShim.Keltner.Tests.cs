namespace ObsoleteShims;
#pragma warning disable CS0618

/// <summary>
/// Tests for obsolete v3 shims whose signatures were narrower than the v2 methods they
/// stand in for, so valid v2 code did not compile.
/// </summary>
/// <remarks>
/// <c>GetKeltner</c> declared <c>int multiplier</c> where v2 — and <c>ToKeltner</c> —
/// declare <c>double</c>. <c>GetStdDevChannels</c>'s tuple overload declared
/// <c>int lookbackPeriods</c> where v2 declared <c>int?</c>, whose <c>null</c> means
/// "the whole series"; its own sibling bar overload still accepts that and says so in
/// its obsolete message.
/// <para>
/// A signature narrowing is invisible to runtime assertions — only compiling v2-shaped
/// source reveals it. The fractional and null-lookback calls below are the guard:
/// reintroducing either narrowing fails to compile this file.
/// </para>
/// </remarks>
[TestClass]
public class ObsoleteKeltnerTests : TestBase
{
    [TestMethod]
    public void GetKeltnerAcceptsAFractionalMultiplier()
    {
        List<KeltnerResult> shim = Bars.GetKeltner(20, 2.5, 10).ToList();

        IReadOnlyList<KeltnerResult> expected = Bars.ToKeltner(20, 2.5, 10);

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.UpperBand).Should().Equal(expected.Select(static r => r.UpperBand));
        shim.Select(static r => r.Centerline).Should().Equal(expected.Select(static r => r.Centerline));
        shim.Select(static r => r.LowerBand).Should().Equal(expected.Select(static r => r.LowerBand));
    }

    [TestMethod]
    public void GetKeltnerDefaultsMatchToKeltner()
    {
        // the bands, not just the centerline: Centerline is the EMA and does not depend
        // on multiplier at all, so asserting it alone cannot catch a wrong default
        List<KeltnerResult> shim = Bars.GetKeltner().ToList();

        IReadOnlyList<KeltnerResult> expected = Bars.ToKeltner();

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.UpperBand).Should().Equal(expected.Select(static r => r.UpperBand));
        shim.Select(static r => r.Centerline).Should().Equal(expected.Select(static r => r.Centerline));
        shim.Select(static r => r.LowerBand).Should().Equal(expected.Select(static r => r.LowerBand));
    }
}
