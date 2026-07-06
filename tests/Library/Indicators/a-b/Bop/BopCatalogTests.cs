namespace Catalogging;

/// <summary>
/// Test class for Bop catalog functionality.
/// </summary>
[TestClass]
public class BopCatalogTests : TestBase
{
    [TestMethod]
    public void BopSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Bop.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Balance of Power (BOP)");
        listing.Uiid.Should().Be("BOP");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToBop");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam smoothPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult bopResult = listing.Results.SingleOrDefault(static r => r.DataName == "Bop");
        bopResult.Should().NotBeNull();
        bopResult?.DisplayName.Should().Be("BOP");
        bopResult.IsReusable.Should().Be(true);
    }
}
