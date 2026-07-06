namespace Utilities;

[TestClass]
public class Reusable : TestBaseWithPrecision
{
    [TestMethod]
    public void Condense()
    {
        List<AdxResult> original = Bars
            .ToAdx()
            .ToList();

        // make a few more in the middle null and NaN
        original[249] = original[249] with { Adx = null };
        original[345] = original[345] with { Adx = double.NaN };

        IReadOnlyList<AdxResult> sut = original.Condense();

        // proper quantities
        sut.Should().HaveCount(473);

        // sample values
        AdxResult last = sut[^1];
        last.Pdi.Should().BeApproximately(17.7565, Money4);
        last.Mdi.Should().BeApproximately(31.1510, Money4);
        last.Adx.Should().BeApproximately(34.2987, Money4);
    }

    [TestMethod]
    public void ToReusableList()
    {
        IReadOnlyList<IReusable> reusableList = Bars
            .ToReusable(CandlePart.Close);

        reusableList.Should().NotBeNull();
        reusableList.Should().HaveCount(502);
        reusableList[^1].Value.Should().Be(245.28d);
    }

    [TestMethod]
    public void BarToReusable()
    {
        DateTime t = DateTime.Parse("5/5/2055", invariantCulture);

        const decimal l = 111111111111111m;
        const decimal o = 222222222222222m;
        const decimal c = 333333333333333m;
        const decimal h = 444444444444444m;
        const decimal v = 555555555555555m;
        const decimal hl2 = (h + l) / 2m;
        const decimal hlc3 = (h + l + c) / 3m;
        const decimal oc2 = (o + c) / 2m;
        const decimal ohl3 = (o + h + l) / 3m;
        const decimal ohlc4 = (o + h + l + c) / 4m;

        Bar q = new(t, o, h, l, c, v);

        q.ToReusable(CandlePart.Open).Value.Should()
            .BeApproximately((double)o, Money10);
        q.ToReusable(CandlePart.High).Value.Should()
            .BeApproximately((double)h, Money10);
        q.ToReusable(CandlePart.Low).Value.Should()
            .BeApproximately((double)l, Money10);
        q.ToReusable(CandlePart.Close).Value.Should()
            .BeApproximately((double)c, Money10);
        q.ToReusable(CandlePart.Volume).Value.Should()
            .BeApproximately((double)v, Money10);
        q.ToReusable(CandlePart.HL2).Value.Should()
            .BeApproximately((double)hl2, Money10);
        q.ToReusable(CandlePart.HLC3).Value.Should()
            .BeApproximately((double)hlc3, Money10);
        q.ToReusable(CandlePart.OC2).Value.Should()
            .BeApproximately((double)oc2, Money10);
        q.ToReusable(CandlePart.OHL3).Value.Should()
            .BeApproximately((double)ohl3, Money10);
        q.ToReusable(CandlePart.OHLC4).Value.Should()
            .BeApproximately((double)ohlc4, Money10);

        // bad argument
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(()
            => q.ToReusable((CandlePart)999));

        // bad argument
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(()
            => q.ToReusable((CandlePart)999));
    }
}
