namespace Catalogging;

/// <summary>
/// Test class for Kama catalog functionality.
/// </summary>
[TestClass]
public class KamaCatalogTests : TestBase
{
    [TestMethod]
    public void KamaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Kama.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Kaufman's Adaptive Moving Average");
        listing.Uiid.Should().Be("KAMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToKama");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam erPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "erPeriods");
        erPeriodsParam.Should().NotBeNull();
        IndicatorParam fastPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam1.Should().NotBeNull();
        IndicatorParam slowPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult erResult = listing.Results.SingleOrDefault(static r => r.DataName == "Er");
        erResult.Should().NotBeNull();
        erResult?.DisplayName.Should().Be("ER");
        erResult.IsReusable.Should().Be(false);
        IndicatorResult kamaResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Kama");
        kamaResult1.Should().NotBeNull();
        kamaResult1?.DisplayName.Should().Be("KAMA");
        kamaResult1.IsReusable.Should().Be(true);
    }
}
