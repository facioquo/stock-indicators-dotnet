namespace Utilities;

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
public class ObsoleteShimNarrowedSignatureTests : TestBase
{
    [TestMethod]
    public void GetKeltnerAcceptsAFractionalMultiplier()
    {
#pragma warning disable CS0618 // exercising the obsolete shim is the point
        List<KeltnerResult> shim = Bars.GetKeltner(20, 2.5, 10).ToList();
#pragma warning restore CS0618

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
#pragma warning disable CS0618
        List<KeltnerResult> shim = Bars.GetKeltner().ToList();
#pragma warning restore CS0618

        IReadOnlyList<KeltnerResult> expected = Bars.ToKeltner();

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.UpperBand).Should().Equal(expected.Select(static r => r.UpperBand));
        shim.Select(static r => r.Centerline).Should().Equal(expected.Select(static r => r.Centerline));
        shim.Select(static r => r.LowerBand).Should().Equal(expected.Select(static r => r.LowerBand));
    }

    [TestMethod]
    public void GetStdDevChannelsTupleAcceptsANullLookback()
    {
        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        List<StdDevChannelsResult> shim = priceTuples.GetStdDevChannels(null, 2).ToList();
#pragma warning restore CS0618

        // null means "the whole series", matching the sibling bar overload
        IReadOnlyList<StdDevChannelsResult> expected
            = Bars.Use(CandlePart.Close).ToStdDevChannels(Bars.Count, 2);

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.Centerline).Should().Equal(expected.Select(static r => r.Centerline));
        shim.Select(static r => r.UpperChannel).Should().Equal(expected.Select(static r => r.UpperChannel));
    }

    [TestMethod]
    public void GetStdDevChannelsTupleDefaultsMatchToStdDevChannels()
    {
        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        List<StdDevChannelsResult> shim = priceTuples.GetStdDevChannels().ToList();
#pragma warning restore CS0618

        IReadOnlyList<StdDevChannelsResult> expected
            = Bars.Use(CandlePart.Close).ToStdDevChannels(20, 2);

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.UpperChannel).Should().Equal(expected.Select(static r => r.UpperChannel));
    }

    [TestMethod]
    public void GetStdDevChannelsBarOverloadStillAcceptsANullLookback()
    {
        // the overload that was already correct, so the two forms stay in step
#pragma warning disable CS0618
        List<StdDevChannelsResult> barForm = Bars.GetStdDevChannels(null, 2).ToList();
#pragma warning restore CS0618

        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        List<StdDevChannelsResult> tupleForm = priceTuples.GetStdDevChannels(null, 2).ToList();
#pragma warning restore CS0618

        tupleForm.Select(static r => r.Centerline).Should().Equal(barForm.Select(static r => r.Centerline));
    }
}
