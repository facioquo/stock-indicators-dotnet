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
public class ObsoleteStdDevChannelsTests : TestBase
{
    [TestMethod]
    public void GetStdDevChannelsTupleAcceptsANullLookback()
    {
        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        List<StdDevChannelsResult> shim = priceTuples.GetStdDevChannels(null, 2).ToList();

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

        List<StdDevChannelsResult> shim = priceTuples.GetStdDevChannels().ToList();

        IReadOnlyList<StdDevChannelsResult> expected
            = Bars.Use(CandlePart.Close).ToStdDevChannels(20, 2);

        shim.Should().HaveCount(expected.Count);
        shim.Select(static r => r.UpperChannel).Should().Equal(expected.Select(static r => r.UpperChannel));
    }

    [TestMethod]
    public void GetStdDevChannelsBarOverloadStillAcceptsANullLookback()
    {
        // the overload that was already correct, so the two forms stay in step
        List<StdDevChannelsResult> barForm = Bars.GetStdDevChannels(null, 2).ToList();

        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        List<StdDevChannelsResult> tupleForm = priceTuples.GetStdDevChannels(null, 2).ToList();

        tupleForm.Select(static r => r.Centerline).Should().Equal(barForm.Select(static r => r.Centerline));
    }
}
