namespace Utilities;

[TestClass]
public class BarPartBufferListTests : TestBaseWithPrecision
{
    [TestMethod]
    public void ConvertBar()
    {
        // compose basic data
        TimeValue o = Bars[501].ToBarPart(CandlePart.Open);
        TimeValue h = Bars[501].ToBarPart(CandlePart.High);
        TimeValue l = Bars[501].ToBarPart(CandlePart.Low);
        TimeValue c = Bars[501].ToBarPart(CandlePart.Close);
        TimeValue v = Bars[501].ToBarPart(CandlePart.Volume);
        TimeValue hl = Bars[501].ToBarPart(CandlePart.HL2);
        TimeValue hlc = Bars[501].ToBarPart(CandlePart.HLC3);
        TimeValue oc = Bars[501].ToBarPart(CandlePart.OC2);
        TimeValue ohl = Bars[501].ToBarPart(CandlePart.OHL3);
        TimeValue ohlc = Bars[501].ToBarPart(CandlePart.OHLC4);

        // proper last date
        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
        c.Timestamp.Should().Be(lastDate);

        // last values should be correct
        c.Value.Should().Be(245.28);
        o.Value.Should().Be(244.92);
        h.Value.Should().Be(245.54);
        l.Value.Should().Be(242.87);
        c.Value.Should().Be(245.28);
        v.Value.Should().Be(147031456);
        hl.Value.Should().Be(244.205);
        hlc.Value.Should().BeApproximately(244.5633, Money4);
        oc.Value.Should().Be(245.1);
        ohl.Value.Should().BeApproximately(244.4433, Money4);
        ohlc.Value.Should().Be(244.6525);
    }

    [TestMethod]
    public void ConvertList()
    {
        // compose data
        IReadOnlyList<TimeValue> o = Bars.ToBarPart(CandlePart.Open);
        IReadOnlyList<TimeValue> h = Bars.ToBarPart(CandlePart.High);
        IReadOnlyList<TimeValue> l = Bars.ToBarPart(CandlePart.Low);
        IReadOnlyList<TimeValue> c = Bars.ToBarPart(CandlePart.Close);
        IReadOnlyList<TimeValue> v = Bars.ToBarPart(CandlePart.Volume);
        IReadOnlyList<TimeValue> hl = Bars.ToBarPart(CandlePart.HL2);
        IReadOnlyList<TimeValue> hlc = Bars.ToBarPart(CandlePart.HLC3);
        IReadOnlyList<TimeValue> oc = Bars.ToBarPart(CandlePart.OC2);
        IReadOnlyList<TimeValue> ohl = Bars.ToBarPart(CandlePart.OHL3);
        IReadOnlyList<TimeValue> ohlc = Bars.ToBarPart(CandlePart.OHLC4);

        // proper quantities
        c.Should().HaveCount(502);

        // samples
        TimeValue ro = o[501];
        TimeValue rh = h[501];
        TimeValue rl = l[501];
        TimeValue rc = c[501];
        TimeValue rv = v[501];
        TimeValue rhl = hl[501];
        TimeValue rhlc = hlc[501];
        TimeValue roc = oc[501];
        TimeValue rohl = ohl[501];
        TimeValue rohlc = ohlc[501];

        // proper last date
        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
        rc.Timestamp.Should().Be(lastDate);

        // last values should be correct
        rc.Value.Should().Be(245.28);
        ro.Value.Should().Be(244.92);
        rh.Value.Should().Be(245.54);
        rl.Value.Should().Be(242.87);
        rc.Value.Should().Be(245.28);
        rv.Value.Should().Be(147031456);
        rhl.Value.Should().Be(244.205);
        rhlc.Value.Should().BeApproximately(244.5633, Money4);
        roc.Value.Should().Be(245.1);
        rohl.Value.Should().BeApproximately(244.4433, Money4);
        rohlc.Value.Should().Be(244.6525);
    }
}
