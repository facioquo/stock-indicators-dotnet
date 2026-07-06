namespace Catalogging;

/// <summary>
/// Test class for ConnorsRsi catalog functionality.
/// </summary>
[TestClass]
public class ConnorsRsiCatalogTests : TestBase
{
    [TestMethod]
    public void ConnorsRsiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ConnorsRsi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ConnorsRSI (CRSI)");
        listing.Uiid.Should().Be("CRSI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToConnorsRsi");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam rsiPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "rsiPeriods");
        rsiPeriodsParam.Should().NotBeNull();
        IndicatorParam streakPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "streakPeriods");
        streakPeriodsParam1.Should().NotBeNull();
        IndicatorParam rankPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "rankPeriods");
        rankPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult streakResult = listing.Results.SingleOrDefault(static r => r.DataName == "Streak");
        streakResult.Should().NotBeNull();
        streakResult?.DisplayName.Should().Be("Streak");
        streakResult.IsReusable.Should().Be(false);
        IndicatorResult rsiResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Rsi");
        rsiResult1.Should().NotBeNull();
        rsiResult1?.DisplayName.Should().Be("RSI");
        rsiResult1.IsReusable.Should().Be(false);
        IndicatorResult rsistreakResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "RsiStreak");
        rsistreakResult2.Should().NotBeNull();
        rsistreakResult2?.DisplayName.Should().Be("RSI of Streak");
        rsistreakResult2.IsReusable.Should().Be(false);
        IndicatorResult percentrankResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "PercentRank");
        percentrankResult3.Should().NotBeNull();
        percentrankResult3?.DisplayName.Should().Be("Percent Rank");
        percentrankResult3.IsReusable.Should().Be(false);
        IndicatorResult connorsrsiResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "ConnorsRsi");
        connorsrsiResult4.Should().NotBeNull();
        connorsrsiResult4?.DisplayName.Should().Be("ConnorsRSI");
        connorsrsiResult4.IsReusable.Should().Be(true);
    }
}
