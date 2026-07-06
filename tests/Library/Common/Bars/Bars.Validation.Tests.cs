namespace Utilities;

// bar validation

public partial class Bars : TestBase
{
    [TestMethod]
    public void Validate()
    {
        IReadOnlyList<Bar> bars = Data.GetDefault();

        IReadOnlyList<Bar> h = bars.Validate();

        // proper quantities
        h.Should().HaveCount(502);

        // sample values
        DateTime lastDate = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
        h[501].Timestamp.Should().Be(lastDate);

        DateTime spotDate = DateTime.ParseExact("02/01/2017", "MM/dd/yyyy", invariantCulture);
        h[20].Timestamp.Should().Be(spotDate);
    }

    [TestMethod]
    public void ValidateLong()
    {
        IReadOnlyList<Bar> h = LongishBars.Validate();

        // proper quantities
        h.Should().HaveCount(5285);

        // sample values
        DateTime lastDate = DateTime.ParseExact("09/04/2020", "MM/dd/yyyy", invariantCulture);
        h[5284].Timestamp.Should().Be(lastDate);
    }

    [TestMethod]
    public void ValidateCut()
    {
        // if bars post-cleaning, is cut down in size it should not corrupt the results

        IReadOnlyList<Bar> bars = Data.GetDefault(200);

        IReadOnlyList<Bar> h = bars.Validate();

        // should be 200 periods, initially
        h.Should().HaveCount(200);

        // should be 20 results and no index corruption
        IReadOnlyList<SmaResult> r1 = h.TakeLast(20).ToList().ToSma(14).ToList();
        r1.Should().HaveCount(20);

        for (int i = 1; i < r1.Count; i++)
        {
            Assert.IsGreaterThanOrEqualTo(r1[i - 1].Timestamp, r1[i].Timestamp);
        }

        // should be 50 results and no index corruption
        IReadOnlyList<SmaResult> r2 = h.TakeLast(50).ToList().ToSma(14).ToList();
        r2.Should().HaveCount(50);

        for (int i = 1; i < r2.Count; i++)
        {
            Assert.IsGreaterThanOrEqualTo(r2[i - 1].Timestamp, r2[i].Timestamp);
        }

        // should be original 200 periods and no index corruption, after temp mods
        h.Should().HaveCount(200);

        for (int i = 1; i < h.Count; i++)
        {
            Assert.IsGreaterThanOrEqualTo(h[i - 1].Timestamp, h[i].Timestamp);
        }
    }

    [TestMethod]
    public void ValidateDuplicates()
    {
        IReadOnlyList<Bar> dupBars = new List<Bar>
        {
            new(Timestamp: DateTime.ParseExact("2017-01-03", "yyyy-MM-dd", invariantCulture), Open: 214.86m, High: 220.33m, Low: 210.96m, Close: 216.99m, Volume: 5923254),
            new(Timestamp: DateTime.ParseExact("2017-01-04", "yyyy-MM-dd", invariantCulture), Open: 214.75m, High: 228.00m, Low: 214.31m, Close: 226.99m, Volume: 11213471),
            new(Timestamp: DateTime.ParseExact("2017-01-05", "yyyy-MM-dd", invariantCulture), Open: 226.42m, High: 227.48m, Low: 221.95m, Close: 226.75m, Volume: 5911695),
            new(Timestamp: DateTime.ParseExact("2017-01-06", "yyyy-MM-dd", invariantCulture), Open: 226.93m, High: 230.31m, Low: 225.45m, Close: 229.01m, Volume: 5527893),
            new(Timestamp: DateTime.ParseExact("2017-01-06", "yyyy-MM-dd", invariantCulture), Open: 228.97m, High: 231.92m, Low: 228.00m, Close: 231.28m, Volume: 3979484)
        };

        InvalidBarsException dx
            = Assert.ThrowsExactly<InvalidBarsException>(
                () => dupBars.Validate());

        dx.Message.Should().Contain("Duplicate date found on 2017-01-06T00:00:00.0000000.");
    }

    [TestMethod]
    public void ValidateOutOfSequence()
    {
        IReadOnlyList<Bar> unorderedBars = new List<Bar>
        {
            new(Timestamp: DateTime.ParseExact("2017-01-03", "yyyy-MM-dd", invariantCulture), Open: 214.86m, High: 220.33m, Low: 210.96m, Close: 216.99m, Volume: 5923254),
            new(Timestamp: DateTime.ParseExact("2017-01-04", "yyyy-MM-dd", invariantCulture), Open: 214.75m, High: 228.00m, Low: 214.31m, Close: 226.99m, Volume: 11213471),
            new(Timestamp: DateTime.ParseExact("2017-01-06", "yyyy-MM-dd", invariantCulture), Open: 228.97m, High: 231.92m, Low: 228.00m, Close: 231.28m, Volume: 3979484),
            new(Timestamp: DateTime.ParseExact("2017-01-05", "yyyy-MM-dd", invariantCulture), Open: 226.42m, High: 227.48m, Low: 221.95m, Close: 226.75m, Volume: 5911695),
            new(Timestamp: DateTime.ParseExact("2017-01-06", "yyyy-MM-dd", invariantCulture), Open: 226.93m, High: 230.31m, Low: 225.45m, Close: 229.01m, Volume: 5527893)
        };

        InvalidBarsException dx
            = Assert.ThrowsExactly<InvalidBarsException>(
                () => unorderedBars.Validate());

        dx.Message.Should()
            .Contain("Bars are out of sequence on 2017-01-05T00:00:00.0000000.");
    }
}
