namespace Catalogging;

/// <summary>
/// Test class for HtTrendline catalog functionality.
/// </summary>
[TestClass]
public class HtTrendlineCatalogTests : TestBase
{
    [TestMethod]
    public void HtTrendlineSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = HtTrendline.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Hilbert Transform Instantaneous Trendline");
        listing.Uiid.Should().Be("HTL");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToHtTrendline");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult dcperiodsResult = listing.Results.SingleOrDefault(static r => r.DataName == "DcPeriods");
        dcperiodsResult.Should().NotBeNull();
        dcperiodsResult?.DisplayName.Should().Be("Dominant Cycle Periods");
        dcperiodsResult.IsReusable.Should().Be(false);
        IndicatorResult trendlineResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Trendline");
        trendlineResult1.Should().NotBeNull();
        trendlineResult1?.DisplayName.Should().Be("Trendline");
        trendlineResult1.IsReusable.Should().Be(true);
        IndicatorResult smoothpriceResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "SmoothPrice");
        smoothpriceResult2.Should().NotBeNull();
        smoothpriceResult2?.DisplayName.Should().Be("Smooth Price");
        smoothpriceResult2.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void HtTrendlineBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = HtTrendline.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Hilbert Transform Instantaneous Trendline");
        listing.Uiid.Should().Be("HTL");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToHtTrendlineList");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult dcperiodsResult = listing.Results.SingleOrDefault(static r => r.DataName == "DcPeriods");
        dcperiodsResult.Should().NotBeNull();
        dcperiodsResult?.DisplayName.Should().Be("Dominant Cycle Periods");
        dcperiodsResult.IsReusable.Should().Be(false);
        IndicatorResult trendlineResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Trendline");
        trendlineResult1.Should().NotBeNull();
        trendlineResult1?.DisplayName.Should().Be("Trendline");
        trendlineResult1.IsReusable.Should().Be(true);
        IndicatorResult smoothpriceResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "SmoothPrice");
        smoothpriceResult2.Should().NotBeNull();
        smoothpriceResult2?.DisplayName.Should().Be("Smooth Price");
        smoothpriceResult2.IsReusable.Should().Be(false);
    }
}
