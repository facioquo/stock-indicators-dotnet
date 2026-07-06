namespace Catalogging;

/// <summary>
/// Test class for Pivots catalog functionality.
/// </summary>
[TestClass]
public class PivotsCatalogTests : TestBase
{
    [TestMethod]
    public void PivotsSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Pivots.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Pivots");
        listing.Uiid.Should().Be("PIVOTS");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToPivots");

        listing.Parameters?.Count.Should().Be(4);

        IndicatorParam leftSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "leftSpan");
        leftSpanParam.Should().NotBeNull();

        IndicatorParam rightSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "rightSpan");
        rightSpanParam.Should().NotBeNull();

        IndicatorParam maxTrendPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "maxTrendPeriods");
        maxTrendPeriodsParam.Should().NotBeNull();

        IndicatorParam endTypeParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "endType");
        endTypeParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(6);

        IndicatorResult highPointResult = listing.Results.SingleOrDefault(static r => r.DataName == "HighPoint");
        highPointResult.Should().NotBeNull();
        highPointResult?.DisplayName.Should().Be("High Point");
        highPointResult.IsReusable.Should().Be(false);

        IndicatorResult lowPointResult = listing.Results.SingleOrDefault(static r => r.DataName == "LowPoint");
        lowPointResult.Should().NotBeNull();
        lowPointResult?.DisplayName.Should().Be("Low Point");
        lowPointResult.IsReusable.Should().Be(false);

        IndicatorResult highLineResult = listing.Results.SingleOrDefault(static r => r.DataName == "HighLine");
        highLineResult.Should().NotBeNull();
        highLineResult?.DisplayName.Should().Be("High Line");
        highLineResult.IsReusable.Should().Be(false);

        IndicatorResult lowLineResult = listing.Results.SingleOrDefault(static r => r.DataName == "LowLine");
        lowLineResult.Should().NotBeNull();
        lowLineResult?.DisplayName.Should().Be("Low Line");
        lowLineResult.IsReusable.Should().Be(false);

        IndicatorResult highTrendResult = listing.Results.SingleOrDefault(static r => r.DataName == "HighTrend");
        highTrendResult.Should().NotBeNull();
        highTrendResult?.DisplayName.Should().Be("High Trend");
        highTrendResult.IsReusable.Should().Be(false);

        IndicatorResult lowTrendResult = listing.Results.SingleOrDefault(static r => r.DataName == "LowTrend");
        lowTrendResult.Should().NotBeNull();
        lowTrendResult?.DisplayName.Should().Be("Low Trend");
        lowTrendResult.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void PivotsBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Pivots.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Pivots");
        listing.Uiid.Should().Be("PIVOTS");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToPivotsList");

        listing.Parameters?.Count.Should().Be(4);
        listing.Results.Should().HaveCount(6);
    }

    [TestMethod]
    public void PivotsStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Pivots.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Pivots");
        listing.Uiid.Should().Be("PIVOTS");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToPivotsHub");

        listing.Parameters?.Count.Should().Be(4);
        listing.Results.Should().HaveCount(6);
    }
}
