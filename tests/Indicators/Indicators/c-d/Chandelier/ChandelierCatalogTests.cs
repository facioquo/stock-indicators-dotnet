namespace Catalogging;

/// <summary>
/// Test class for Chandelier catalog functionality.
/// </summary>
[TestClass]
public class ChandelierCatalogTests : TestBase
{
    [TestMethod]
    public void ChandelierSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Chandelier.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chandelier Exit");
        listing.Uiid.Should().Be("CHEXIT");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToChandelier");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult chandelierexitResult = listing.Results.SingleOrDefault(static r => r.DataName == "ChandelierExit");
        chandelierexitResult.Should().NotBeNull();
        chandelierexitResult?.DisplayName.Should().Be("Chandelier Exit");
        chandelierexitResult.IsReusable.Should().Be(true);
    }
}
