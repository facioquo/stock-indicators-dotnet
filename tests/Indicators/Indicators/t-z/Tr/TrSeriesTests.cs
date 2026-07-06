namespace StaticSeries;

[TestClass]
public class TrSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        IReadOnlyList<TrResult> sut = Bars
            .ToTr();

        // proper quantities
        sut.Should().HaveCount(502);
        sut.Where(static x => x.Tr != null).Should().HaveCount(501);

        // sample values
        TrResult r0 = sut[0];
        r0.Tr.Should().BeNull();

        TrResult r1 = sut[1];
        r1.Tr.Should().BeApproximately(1.42, Money8);

        TrResult r12 = sut[12];
        r12.Tr.Should().BeApproximately(1.32, Money8);

        TrResult r13 = sut[13];
        r13.Tr.Should().BeApproximately(1.45, Money8);

        TrResult r24 = sut[24];
        r24.Tr.Should().BeApproximately(0.88, Money8);

        TrResult r249 = sut[249];
        r249.Tr.Should().BeApproximately(0.58, Money8);

        TrResult r501 = sut[501];
        r501.Tr.Should().BeApproximately(2.67, Money8);
    }

    [TestMethod]
    public void ChainingFromResults_WorksAsExpected()
    {
        // same as ATR
        IReadOnlyList<SmmaResult> sut = Bars
            .ToTr()
            .ToSmma(14);

        IReadOnlyList<AtrResult> atrResults = Bars
            .ToAtr();

        for (int i = 0; i < sut.Count; i++)
        {
            SmmaResult r = sut[i];
            AtrResult a = atrResults[i];

            r.Timestamp.Should().Be(a.Timestamp);
            r.Smma.Should().Be(a.Atr);
        }
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<TrResult> r = BadBars
            .ToTr();

        r.Should().HaveCount(502);
        r.Where(static x => x.Tr is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<TrResult> r0 = Nobars
            .ToTr();

        r0.Should().BeEmpty();

        IReadOnlyList<TrResult> r1 = Onebar
            .ToTr();

        r1.Should().HaveCount(1);
    }
}
