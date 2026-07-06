namespace Catalogging;

/// <summary>
/// Test class for Fcb catalog functionality.
/// </summary>
[TestClass]
public class FcbCatalogTests : TestBase
{
    [TestMethod]
    public void FcbSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fcb.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Fractal Chaos Bands");
        listing.Uiid.Should().Be("FCB");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToFcb");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam windowSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "windowSpan");
        windowSpanParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult upperbandResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult.Should().NotBeNull();
        upperbandResult?.DisplayName.Should().Be("Upper Band");
        upperbandResult.IsReusable.Should().Be(true);
        IndicatorResult lowerbandResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult1.Should().NotBeNull();
        lowerbandResult1?.DisplayName.Should().Be("Lower Band");
        lowerbandResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void FcbStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fcb.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Fractal Chaos Bands");
        listing.Uiid.Should().Be("FCB");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToFcbHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam windowSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "windowSpan");
        windowSpanParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult upperbandResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult.Should().NotBeNull();
        upperbandResult?.DisplayName.Should().Be("Upper Band");
        upperbandResult.IsReusable.Should().Be(true);
        IndicatorResult lowerbandResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult1.Should().NotBeNull();
        lowerbandResult1?.DisplayName.Should().Be("Lower Band");
        lowerbandResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void FcbBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fcb.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Fractal Chaos Bands");
        listing.Uiid.Should().Be("FCB");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToFcbList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam windowSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "windowSpan");
        windowSpanParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult upperbandResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult.Should().NotBeNull();
        upperbandResult?.DisplayName.Should().Be("Upper Band");
        upperbandResult.IsReusable.Should().Be(true);
        IndicatorResult lowerbandResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult1.Should().NotBeNull();
        lowerbandResult1?.DisplayName.Should().Be("Lower Band");
        lowerbandResult1.IsReusable.Should().Be(false);
    }
}
