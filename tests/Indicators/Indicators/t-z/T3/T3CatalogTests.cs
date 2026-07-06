namespace Catalogging;

/// <summary>
/// Test class for T3 catalog functionality.
/// </summary>
[TestClass]
public class T3CatalogTests : TestBase
{
    [TestMethod]
    public void T3Series_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = T3.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("T3 Moving Average");
        listing.Uiid.Should().Be("T3");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToT3");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam volumeFactorParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "volumeFactor");
        volumeFactorParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult t3Result = listing.Results.SingleOrDefault(static r => r.DataName == "T3");
        t3Result.Should().NotBeNull();
        t3Result?.DisplayName.Should().Be("T3");
        t3Result.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void T3Stream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = T3.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("T3 Moving Average");
        listing.Uiid.Should().Be("T3");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToT3Hub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void T3Buffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = T3.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("T3 Moving Average");
        listing.Uiid.Should().Be("T3");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToT3List");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
    }
}
