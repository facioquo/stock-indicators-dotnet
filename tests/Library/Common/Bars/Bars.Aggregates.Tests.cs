namespace Utilities;

// bar aggregates

public partial class Bars : TestBase
{
    [TestMethod]
    public void Aggregate()
    {
        IReadOnlyList<Bar> bars = Data.GetIntraday();

        // aggregate
        IReadOnlyList<Bar> sut = bars
            .Aggregate(BarInterval.FifteenMinutes);

        // proper quantities
        sut.Should().HaveCount(108);

        // sample values
        Bar r0 = sut[0];
        Assert.AreEqual(DateTime.Parse("2020-12-15 09:30", invariantCulture), r0.Timestamp);
        r0.Open.Should().Be(367.40m);
        r0.High.Should().Be(367.775m);
        r0.Low.Should().Be(367.02m);
        r0.Close.Should().Be(367.24m);
        r0.Volume.Should().Be(2401786m);

        Bar r1 = sut[1];
        Assert.AreEqual(DateTime.Parse("2020-12-15 09:45", invariantCulture), r1.Timestamp);
        r1.Open.Should().Be(367.25m);
        r1.High.Should().Be(367.44m);
        r1.Low.Should().Be(366.69m);
        r1.Close.Should().Be(366.86m);
        r1.Volume.Should().Be(1669983m);

        Bar r2 = sut[2];
        Assert.AreEqual(DateTime.Parse("2020-12-15 10:00", invariantCulture), r2.Timestamp);
        r2.Open.Should().Be(366.85m);
        r2.High.Should().Be(367.17m);
        r2.Low.Should().Be(366.57m);
        r2.Close.Should().Be(366.97m);
        r2.Volume.Should().Be(1396993m);

        // no history scenario
        IReadOnlyList<Bar> noBars = [];
        IReadOnlyList<Bar> noResults = noBars.Aggregate(BarInterval.Day);
        Assert.IsEmpty(noResults);
    }

    [TestMethod]
    public void AggregateTimeSpan()
    {
        IReadOnlyList<Bar> bars = Data.GetIntraday();

        // aggregate
        IReadOnlyList<Bar> sut = bars
            .Aggregate(TimeSpan.FromMinutes(15));

        // proper quantities
        sut.Should().HaveCount(108);

        // sample values
        Bar r0 = sut[0];
        Assert.AreEqual(DateTime.Parse("2020-12-15 09:30", invariantCulture), r0.Timestamp);
        r0.Open.Should().Be(367.40m);
        r0.High.Should().Be(367.775m);
        r0.Low.Should().Be(367.02m);
        r0.Close.Should().Be(367.24m);
        r0.Volume.Should().Be(2401786m);

        Bar r1 = sut[1];
        Assert.AreEqual(DateTime.Parse("2020-12-15 09:45", invariantCulture), r1.Timestamp);
        r1.Open.Should().Be(367.25m);
        r1.High.Should().Be(367.44m);
        r1.Low.Should().Be(366.69m);
        r1.Close.Should().Be(366.86m);
        r1.Volume.Should().Be(1669983m);

        Bar r2 = sut[2];
        Assert.AreEqual(DateTime.Parse("2020-12-15 10:00", invariantCulture), r2.Timestamp);
        r2.Open.Should().Be(366.85m);
        r2.High.Should().Be(367.17m);
        r2.Low.Should().Be(366.57m);
        r2.Close.Should().Be(366.97m);
        r2.Volume.Should().Be(1396993m);

        // no history scenario
        IReadOnlyList<Bar> noBars = [];
        IReadOnlyList<Bar> noResults = noBars.Aggregate(TimeSpan.FromDays(1));
        Assert.IsEmpty(noResults);
    }

    [TestMethod]
    public void AggregateMonth()
    {
        // aggregate
        IReadOnlyList<Bar> sut = Bars
            .Aggregate(BarInterval.Month);

        // proper quantities
        sut.Should().HaveCount(24);

        // sample values
        Bar r0 = sut[0];
        Assert.AreEqual(DateTime.Parse("2017-01-01", invariantCulture), r0.Timestamp);
        r0.Open.Should().Be(212.61m);
        r0.High.Should().Be(217.02m);
        r0.Low.Should().Be(211.52m);
        r0.Close.Should().Be(214.96m);
        r0.Volume.Should().Be(1569087580m);

        Bar r1 = sut[1];
        Assert.AreEqual(DateTime.Parse("2017-02-01", invariantCulture), r1.Timestamp);
        r1.Open.Should().Be(215.65m);
        r1.High.Should().Be(224.20m);
        r1.Low.Should().Be(214.29m);
        r1.Close.Should().Be(223.41m);
        r1.Volume.Should().Be(1444958340m);

        Bar r23 = sut[23];
        Assert.AreEqual(DateTime.Parse("2018-12-01", invariantCulture), r23.Timestamp);
        r23.Open.Should().Be(273.47m);
        r23.High.Should().Be(273.59m);
        r23.Low.Should().Be(229.42m);
        r23.Close.Should().Be(245.28m);
        r23.Volume.Should().Be(3173255968m);
    }

    [TestMethod]
    public void AggregateWeek_StartsOnMonday()
    {
        // weeks are fixed Monday 00:00 buckets, not a rolling 7 days
        List<Bar> dailyBars =
        [
            new(DateTime.Parse("2023-11-05 23:59", invariantCulture), 100, 101, 99, 100, 10),  // Sun
            new(DateTime.Parse("2023-11-06 00:00", invariantCulture), 100, 102, 98, 101, 20),  // Mon
            new(DateTime.Parse("2023-11-10 16:00", invariantCulture), 101, 105, 97, 104, 30),  // Fri
            new(DateTime.Parse("2023-11-12 23:59", invariantCulture), 104, 106, 103, 105, 40), // Sun
            new(DateTime.Parse("2023-11-13 09:30", invariantCulture), 105, 107, 104, 106, 50), // Mon
        ];

        IReadOnlyList<Bar> sut = dailyBars.Aggregate(BarInterval.Week);

        sut.Select(static b => b.Timestamp).Should().Equal(
            DateTime.Parse("2023-10-30", invariantCulture),
            DateTime.Parse("2023-11-06", invariantCulture),
            DateTime.Parse("2023-11-13", invariantCulture));
        sut.Should().AllSatisfy(static b => {
            b.Timestamp.DayOfWeek.Should().Be(DayOfWeek.Monday);
            b.Timestamp.TimeOfDay.Should().Be(TimeSpan.Zero);
        });

        Bar week = sut[1];
        week.Open.Should().Be(100);
        week.High.Should().Be(106);
        week.Low.Should().Be(97);
        week.Close.Should().Be(105);
        week.Volume.Should().Be(90);
    }

    [TestMethod]
    public void AggregateTimeSpan_AlignsToCalendarOrigin()
    {
        // 1440 is not a multiple of 7, so a 7-minute bucket spans midnight
        List<Bar> minuteBars =
        [
            new(DateTime.Parse("2023-11-09 23:52", invariantCulture), 100, 101, 99, 100, 1),
            new(DateTime.Parse("2023-11-09 23:55", invariantCulture), 100, 102, 99, 101, 2),
            new(DateTime.Parse("2023-11-10 00:00", invariantCulture), 101, 103, 100, 102, 3),
            new(DateTime.Parse("2023-11-10 00:01", invariantCulture), 102, 104, 101, 103, 4),
        ];

        IReadOnlyList<Bar> sut = minuteBars.Aggregate(TimeSpan.FromMinutes(7));

        sut.Select(static b => b.Timestamp).Should().Equal(
            DateTime.Parse("2023-11-09 23:47", invariantCulture),
            DateTime.Parse("2023-11-09 23:54", invariantCulture),
            DateTime.Parse("2023-11-10 00:01", invariantCulture));
        sut[1].Volume.Should().Be(5);
    }

    [TestMethod]  // bad period size
    public void AggregateBadSize()
        => FluentActions
            .Invoking(static () => Bars.Aggregate(TimeSpan.Zero))
            .Should()
            .ThrowExactly<ArgumentOutOfRangeException>();
}
