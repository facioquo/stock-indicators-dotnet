namespace Catalogging;

/// <summary>
/// Test class for Mama catalog functionality.
/// </summary>
[TestClass]
public class MamaCatalogTests : TestBase
{
    [TestMethod]
    public void MamaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Mama.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("MESA Adaptive Moving Average");
        listing.Uiid.Should().Be("MAMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToMama");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam fastLimitParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastLimit");
        fastLimitParam.Should().NotBeNull();
        IndicatorParam slowLimitParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowLimit");
        slowLimitParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult mamaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Mama");
        mamaResult.Should().NotBeNull();
        mamaResult?.DisplayName.Should().Be("MAMA");
        mamaResult.IsReusable.Should().Be(true);
        IndicatorResult famaResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Fama");
        famaResult1.Should().NotBeNull();
        famaResult1?.DisplayName.Should().Be("FAMA");
        famaResult1.IsReusable.Should().Be(false);
    }
}
