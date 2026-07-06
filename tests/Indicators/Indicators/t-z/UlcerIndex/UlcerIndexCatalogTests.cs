namespace Catalogging;

/// <summary>
/// Test class for UlcerIndex catalog functionality.
/// </summary>
[TestClass]
public class UlcerIndexCatalogTests : TestBase
{
    [TestMethod]
    public void UlcerIndexSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = UlcerIndex.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Ulcer Index");
        listing.Uiid.Should().Be("ULCER");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToUlcerIndex");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult ulcerindexResult = listing.Results.SingleOrDefault(static r => r.DataName == "UlcerIndex");
        ulcerindexResult.Should().NotBeNull();
        ulcerindexResult?.DisplayName.Should().Be("Ulcer Index");
        ulcerindexResult.IsReusable.Should().Be(true);
    }
}
