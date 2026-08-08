namespace Utilities;

[TestClass]
[TestCategory("Utilities")]
public class Numericals : TestBase
{
    private readonly double[] _closePrice = LongishBars
        .Select(static x => (double)x.Close)
        .ToArray();

    private readonly double[] _x = [1, 2, 3, 4, 5];
    private readonly double[] _y = [0, 0, 0, 0];

    [TestMethod]
    public void StdDev()
    {
        double sd = _closePrice.StdDev();

        Assert.AreEqual(633.932098287, Math.Round(sd, 9));
    }

    [TestMethod]
    public void StdDevNull()
        => FluentActions
            .Invoking(static () => Numerical.StdDev(null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

    [TestMethod]
    public void Slope()
    {
        double s = Numerical.Slope(_x, _x);

        s.Should().Be(1d);
    }

    [TestMethod]
    public void SlopeXnull()
        => FluentActions
            .Invoking(() => Numerical.Slope(null, _x))
            .Should()
            .ThrowExactly<ArgumentNullException>();

    [TestMethod]
    public void SlopeYnull()
        => FluentActions
            .Invoking(() => Numerical.Slope(_x, null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

    [TestMethod]
    public void SlopeMismatch()
        => FluentActions
            .Invoking(() => Numerical.Slope(_x, _y))
            .Should()
            .ThrowExactly<ArgumentException>();

    [TestMethod]
    public void Slope_AllEqualXValues_ReturnsNaN()
    {
        // pins IEEE 754 0/0 = NaN for zero-variance x
        double[] flatX = [5, 5, 5, 5];
        double[] anyY = [1, 2, 3, 4];

        double s = Numerical.Slope(flatX, anyY);

        s.Should().Be(double.NaN);
    }

    [TestMethod]
    public void Slope_SquaredDeviationUnderflow_ReturnsNaN()
    {
        // devX * devX underflows to exactly 0 while devX * devY
        // does not; without the zero-denominator guard this
        // returned +Infinity instead of NaN
        double[] tinyX = [1e-200, -1e-200];
        double[] hugeY = [1e200, -1e200];

        double s = Numerical.Slope(tinyX, hugeY);

        s.Should().Be(double.NaN);
    }

    [TestMethod]
    public void GetDecimalPlaces_LargeIntegerPart_DoesNotOverflow()
    {
        // values beyond int.MaxValue (e.g. large volumes)
        // previously threw OverflowException from an int cast
        const decimal largeVolume = 3_000_000_000.25m;

        int places = largeVolume.GetDecimalPlaces();

        places.Should().Be(2);
    }

    [TestMethod]
    public void RoundDownDate()
    {
        TimeSpan interval = BarInterval.OneHour.ToTimeSpan();
        DateTime evDate = DateTime.Parse("2020-12-15 09:35:45", invariantCulture);

        DateTime rnDate = evDate.RoundDown(interval);
        DateTime exDate = DateTime.Parse("2020-12-15 09:00:00", invariantCulture);

        rnDate.Should().Be(exDate);
    }

    [TestMethod]
    public void ToTimeSpan()
    {
        Assert.AreEqual(BarInterval.OneMinute.ToTimeSpan(), TimeSpan.FromMinutes(1));
        Assert.AreEqual(BarInterval.TwoMinutes.ToTimeSpan(), TimeSpan.FromMinutes(2));
        Assert.AreEqual(BarInterval.ThreeMinutes.ToTimeSpan(), TimeSpan.FromMinutes(3));
        Assert.AreEqual(BarInterval.FiveMinutes.ToTimeSpan(), TimeSpan.FromMinutes(5));
        Assert.AreEqual(BarInterval.FifteenMinutes.ToTimeSpan(), TimeSpan.FromHours(0.25));
        Assert.AreEqual(BarInterval.ThirtyMinutes.ToTimeSpan(), TimeSpan.FromHours(0.5));
        Assert.AreEqual(BarInterval.OneHour.ToTimeSpan(), TimeSpan.FromMinutes(60));
        Assert.AreEqual(BarInterval.TwoHours.ToTimeSpan(), TimeSpan.FromHours(2));
        Assert.AreEqual(BarInterval.FourHours.ToTimeSpan(), TimeSpan.FromHours(4));
        Assert.AreEqual(BarInterval.Day.ToTimeSpan(), TimeSpan.FromHours(24));
        Assert.AreEqual(BarInterval.Week.ToTimeSpan(), TimeSpan.FromDays(7));

        TimeSpan.Zero.Should().Be(BarInterval.Month.ToTimeSpan());
    }
}
