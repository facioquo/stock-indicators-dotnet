namespace Catalogging;

/// <summary>
/// Test class for Hurst catalog functionality.
/// </summary>
[TestClass]
public class HurstCatalogTests : TestBase
{
    [TestMethod]
    public void HurstSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Hurst.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Hurst Exponent");
        listing.Uiid.Should().Be("HURST");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToHurst");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        lookbackPeriodsParam?.Minimum.Should().Be(20);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult hurstexponentResult = listing.Results.SingleOrDefault(static r => r.DataName == "HurstExponent");
        hurstexponentResult.Should().NotBeNull();
        hurstexponentResult?.DisplayName.Should().Be("Hurst Exponent");
        hurstexponentResult.IsReusable.Should().Be(true);

        IndicatorResult hurstExponentAlResult = listing.Results.SingleOrDefault(static r => r.DataName == "HurstExponentAL");
        hurstExponentAlResult.Should().NotBeNull();
        hurstExponentAlResult?.DisplayName.Should().Be("Hurst Exponent AL");
        hurstExponentAlResult?.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void HurstBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Hurst.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Hurst Exponent");
        listing.Uiid.Should().Be("HURST");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToHurstList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        lookbackPeriodsParam?.Minimum.Should().Be(20);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult hurstexponentResult = listing.Results.SingleOrDefault(static r => r.DataName == "HurstExponent");
        hurstexponentResult.Should().NotBeNull();
        hurstexponentResult?.DisplayName.Should().Be("Hurst Exponent");
        hurstexponentResult.IsReusable.Should().Be(true);

        IndicatorResult hurstExponentAlResult = listing.Results.SingleOrDefault(static r => r.DataName == "HurstExponentAL");
        hurstExponentAlResult.Should().NotBeNull();
        hurstExponentAlResult?.DisplayName.Should().Be("Hurst Exponent AL");
        hurstExponentAlResult?.IsReusable.Should().Be(false);
    }
}
