namespace Catalogging;

/// <summary>
/// Test class for Marubozu catalog functionality.
/// </summary>
[TestClass]
public class MarubozuCatalogTests : TestBase
{
    [TestMethod]
    public void MarubozuSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Marubozu.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Marubozu");
        listing.Uiid.Should().Be("MARUBOZU");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.CandlestickPattern);
        listing.MethodName.Should().Be("ToMarubozu");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam minBodyPercentParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "minBodyPercent");
        minBodyPercentParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult matchResult = listing.Results.SingleOrDefault(static r => r.DataName == "Match");
        matchResult.Should().NotBeNull();
        matchResult?.DisplayName.Should().Be("Match");
        matchResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void MarubozuStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Marubozu.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Marubozu");
        listing.Uiid.Should().Be("MARUBOZU");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.CandlestickPattern);
        listing.MethodName.Should().Be("ToMarubozuHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam minBodyPercentParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "minBodyPercent");
        minBodyPercentParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult matchResult = listing.Results.SingleOrDefault(static r => r.DataName == "Match");
        matchResult.Should().NotBeNull();
        matchResult?.DisplayName.Should().Be("Match");
        matchResult.IsReusable.Should().Be(true);
    }
}
