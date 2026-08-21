namespace Catalogging;

/// <summary>
/// Test class for Prs catalog functionality.
/// </summary>
[TestClass]
public class PrsCatalogTests : TestBase
{
    [TestMethod]
    public void PrsSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Prs.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Price Relative Strength");
        listing.Uiid.Should().Be("PRS");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToPrs");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult prsResult = listing.Results.SingleOrDefault(static r => r.DataName == "Prs");
        prsResult.Should().NotBeNull();
        prsResult?.DisplayName.Should().Be("PRS");
        prsResult.IsReusable.Should().Be(true);
        IndicatorResult prspercentResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "PrsPercent");
        prspercentResult1.Should().NotBeNull();
        prspercentResult1?.DisplayName.Should().Be("PRS %");
        prspercentResult1.IsReusable.Should().Be(false);
    }
}
