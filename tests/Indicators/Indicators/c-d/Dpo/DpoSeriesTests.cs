namespace StaticSeries;

[TestClass]
public class DpoSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public override void DefaultParameters_ReturnsExpectedResults()
    {
        // get expected data
        List<Bar> qot = [];
        List<DpoResult> exp = [];

        List<string> csvData = File.ReadAllLines("TestData/results/dpo.standard.csv")
            .Skip(1)
            .ToList();

        for (int i = 0; i < csvData.Count; i++)
        {
            string[] csv = csvData[i].Split(",");
            DateTime date = Convert.ToDateTime(csv[1], invariantCulture);

            qot.Add(new Bar(date, 0, 0, 0, Close: csv[5].ToDecimal(), 0));
            exp.Add(new(date, csv[7].ToDoubleNull(), csv[6].ToDoubleNull()));
        }

        // calculate actual data
        IReadOnlyList<DpoResult> act = qot.ToDpo(14);

        // assertions
        act.Should().HaveCount(exp.Count);

        // compare all values
        for (int i = 0; i < exp.Count; i++)
        {
            DpoResult e = exp[i];
            DpoResult a = act[i];

            a.Timestamp.Should().Be(e.Timestamp);
            a.Sma.Should().BeApproximately(e.Sma, Money5, $"at index {i}");
            a.Dpo.Should().BeApproximately(e.Dpo, Money5, $"at index {i}");
        }
    }

    [TestMethod]
    public void UseReusable_ClosePrice_ReturnsExpectedResult()
    {
        IReadOnlyList<DpoResult> sut = Bars
            .Use(CandlePart.Close)
            .ToDpo(14);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dpo != null).Should().HaveCount(489);
    }

    [TestMethod]
    public void Chainee_FromSma_ReturnsExpectedResult()
    {
        IReadOnlyList<DpoResult> sut = Bars
            .ToSma(2)
            .ToDpo(14);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Dpo != null).Should().HaveCount(488);
    }

    [TestMethod]
    public void ChainFromResults_ToSma_ReturnsExpectedResult()
    {
        IReadOnlyList<SmaResult> sut = Bars
            .ToDpo(14)
            .ToSma(10);

        sut.Should().HaveCount(502);
        sut.Where(static x => x.Sma is not null and not double.NaN).Should().HaveCount(480);
    }

    [TestMethod]
    public override void BadBars_DoesNotFail()
    {
        IReadOnlyList<DpoResult> r = BadBars
            .ToDpo(5);

        r.Should().HaveCount(502);
        r.Where(static x => x.Dpo is double v && double.IsNaN(v)).Should().BeEmpty();
    }

    [TestMethod]
    public override void NoBars_ReturnsEmpty()
    {
        IReadOnlyList<DpoResult> r0 = Nobars
            .ToDpo(5);

        r0.Should().BeEmpty();

        IReadOnlyList<DpoResult> r1 = Onebar
            .ToDpo(5);

        r1.Should().HaveCount(1);
    }

    /// <summary>
    /// bad SMA period
    /// </summary>
    [TestMethod]
    public void Exceptions_InvalidLookback_ThrowsArgumentOutOfRangeException()
        => FluentActions
            .Invoking(static () => Bars.ToDpo(0))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
