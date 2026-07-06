namespace Catalogging;

/// <summary>
/// Test class for AtrStop catalog functionality.
/// </summary>
[TestClass]
public class AtrStopCatalogTests : TestBase
{
    [TestMethod]
    public void AtrStopSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = AtrStop.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ATR Trailing Stop");
        listing.Uiid.Should().Be("ATR-STOP");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAtrStop");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult atrstopResult = listing.Results.SingleOrDefault(static r => r.DataName == "AtrStop");
        atrstopResult.Should().NotBeNull();
        atrstopResult?.DisplayName.Should().Be("ATR Trailing Stop");
        atrstopResult.IsReusable.Should().Be(true);
        IndicatorResult buystopResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BuyStop");
        buystopResult1.Should().NotBeNull();
        buystopResult1?.DisplayName.Should().Be("Buy Stop");
        buystopResult1.IsReusable.Should().Be(false);
        IndicatorResult sellstopResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "SellStop");
        sellstopResult2.Should().NotBeNull();
        sellstopResult2?.DisplayName.Should().Be("Sell Stop");
        sellstopResult2.IsReusable.Should().Be(false);
        IndicatorResult atrResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult3.Should().NotBeNull();
        atrResult3?.DisplayName.Should().Be("ATR");
        atrResult3.IsReusable.Should().Be(false);
    }
    [TestMethod]
    public void AtrStopStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = AtrStop.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ATR Trailing Stop");
        listing.Uiid.Should().Be("ATR-STOP");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAtrStopHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult atrstopResult = listing.Results.SingleOrDefault(static r => r.DataName == "AtrStop");
        atrstopResult.Should().NotBeNull();
        atrstopResult?.DisplayName.Should().Be("ATR Trailing Stop");
        atrstopResult.IsReusable.Should().Be(true);
        IndicatorResult buystopResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BuyStop");
        buystopResult1.Should().NotBeNull();
        buystopResult1?.DisplayName.Should().Be("Buy Stop");
        buystopResult1.IsReusable.Should().Be(false);
        IndicatorResult sellstopResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "SellStop");
        sellstopResult2.Should().NotBeNull();
        sellstopResult2?.DisplayName.Should().Be("Sell Stop");
        sellstopResult2.IsReusable.Should().Be(false);
        IndicatorResult atrResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult3.Should().NotBeNull();
        atrResult3?.DisplayName.Should().Be("ATR");
        atrResult3.IsReusable.Should().Be(false);
    }
    [TestMethod]
    public void AtrStopBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = AtrStop.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ATR Trailing Stop");
        listing.Uiid.Should().Be("ATR-STOP");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAtrStopList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam multiplierParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "multiplier");
        multiplierParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult atrstopResult = listing.Results.SingleOrDefault(static r => r.DataName == "AtrStop");
        atrstopResult.Should().NotBeNull();
        atrstopResult?.DisplayName.Should().Be("ATR Trailing Stop");
        atrstopResult.IsReusable.Should().Be(true);
        IndicatorResult buystopResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BuyStop");
        buystopResult1.Should().NotBeNull();
        buystopResult1?.DisplayName.Should().Be("Buy Stop");
        buystopResult1.IsReusable.Should().Be(false);
        IndicatorResult sellstopResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "SellStop");
        sellstopResult2.Should().NotBeNull();
        sellstopResult2?.DisplayName.Should().Be("Sell Stop");
        sellstopResult2.IsReusable.Should().Be(false);
        IndicatorResult atrResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Atr");
        atrResult3.Should().NotBeNull();
        atrResult3?.DisplayName.Should().Be("ATR");
        atrResult3.IsReusable.Should().Be(false);
    }
}
