namespace Catalogging;

/// <summary>
/// Test class for RenkoAtr catalog functionality.
/// </summary>
[TestClass]
public class RenkoAtrCatalogTests : TestBase
{
    [TestMethod]
    public void RenkoAtrSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = RenkoAtr.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Renko (ATR)");
        listing.Uiid.Should().Be("RENKO-ATR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToRenkoAtr");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam atrPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "atrPeriods");
        atrPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(6);

        IndicatorResult openResult = listing.Results.SingleOrDefault(static r => r.DataName == "Open");
        openResult.Should().NotBeNull();
        openResult?.DisplayName.Should().Be("Open");
        openResult.IsReusable.Should().Be(false);
        IndicatorResult highResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "High");
        highResult1.Should().NotBeNull();
        highResult1?.DisplayName.Should().Be("High");
        highResult1.IsReusable.Should().Be(false);
        IndicatorResult lowResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Low");
        lowResult2.Should().NotBeNull();
        lowResult2?.DisplayName.Should().Be("Low");
        lowResult2.IsReusable.Should().Be(false);
        IndicatorResult closeResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Close");
        closeResult3.Should().NotBeNull();
        closeResult3?.DisplayName.Should().Be("Close");
        closeResult3.IsReusable.Should().Be(true);
        IndicatorResult volumeResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Volume");
        volumeResult4.Should().NotBeNull();
        volumeResult4?.DisplayName.Should().Be("Volume");
        volumeResult4.IsReusable.Should().Be(false);
        IndicatorResult isupResult5 = listing.Results.SingleOrDefault(static r => r.DataName == "IsUp");
        isupResult5.Should().NotBeNull();
        isupResult5?.DisplayName.Should().Be("Is Up");
        isupResult5.IsReusable.Should().Be(false);
    }
}
