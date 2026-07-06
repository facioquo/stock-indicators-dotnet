// bar list converters

namespace Utilities;

[TestClass]
public partial class Bars : TestBase
{
    [TestMethod]
    public void ToSortedList()
    {
        IReadOnlyList<Bar> bars = Data.GetMismatch();

        IReadOnlyList<Bar> h = bars.ToSortedList();

        // proper quantities
        h.Should().HaveCount(502);

        // check first date
        DateTime firstDate = DateTime.ParseExact("01/18/2016", "MM/dd/yyyy", invariantCulture);
        h[0].Timestamp.Should().Be(firstDate);

        // check last date
        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
        h[^1].Timestamp.Should().Be(lastDate);

        // spot check an out of sequence date
        DateTime spotDate = DateTime.ParseExact("03/16/2017", "MM/dd/yyyy", invariantCulture);
        h[50].Timestamp.Should().Be(spotDate);
    }

    [TestMethod]
    public void ToBarList()
    {
        // setup
        IReadOnlyList<Bar> bars
            = Bars.Take(5).ToList();

        IReadOnlyList<MyBar> myBars = bars
            .Select(static x => new MyBar {
                Timestamp = x.Timestamp,
                Open = x.Open,
                High = x.High,
                Low = x.Low,
                Close = x.Close,
                Volume = x.Volume
            }).ToList();

        // sut
        IReadOnlyList<Bar> sut
            = myBars.ToBarList();

        // assert is same as original
        sut.IsExactly(bars);
    }

    [TestMethod]
    public void ToBar()
    {
        // setup
        Bar q = Bars[0];

        MyBar myBar = new() {
            Timestamp = q.Timestamp,
            Open = q.Open,
            High = q.High,
            Low = q.Low,
            Close = q.Close,
            Volume = q.Volume
        };

        // sut
        Bar sut = myBar.ToBar();

        // assert value based equality
        sut.Should().Be(q);
        sut.Value.Should().Be(q.Value);
    }

    private class MyBar : IBar
    {
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public double Value => (double)Close;
    }
}
