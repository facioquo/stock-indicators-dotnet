namespace Catalogging;

/// <summary>
/// Test class for Fractal catalog functionality.
/// </summary>
[TestClass]
public class FractalCatalogTests : TestBase
{
    [TestMethod]
    public void FractalSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fractal.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Fractal (high/low)");
        listing.Uiid.Should().Be("FRACTAL");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PricePattern);
        listing.MethodName.Should().Be("ToFractal");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam windowSpanParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "windowSpan");
        windowSpanParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult fractalbearResult = listing.Results.SingleOrDefault(static r => r.DataName == "FractalBear");
        fractalbearResult.Should().NotBeNull();
        fractalbearResult?.DisplayName.Should().Be("Bear Fractal");
        fractalbearResult.IsReusable.Should().Be(false);
        IndicatorResult fractalbullResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "FractalBull");
        fractalbullResult1.Should().NotBeNull();
        fractalbullResult1?.DisplayName.Should().Be("Bull Fractal");
        fractalbullResult1.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void FractalBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fractal.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Fractal (high/low)");
        listing.Uiid.Should().Be("FRACTAL");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PricePattern);
        listing.MethodName.Should().Be("ToFractalList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void FractalStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Fractal.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Fractal (high/low)");
        listing.Uiid.Should().Be("FRACTAL");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PricePattern);
        listing.MethodName.Should().Be("ToFractalHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }
}
