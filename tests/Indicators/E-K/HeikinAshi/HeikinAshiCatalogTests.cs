namespace Catalogging;

/// <summary>
/// Test class for HeikinAshi catalog functionality.
/// </summary>
[TestClass]
public class HeikinAshiCatalogTests : TestBase
{
    [TestMethod]
    public void HeikinAshiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = HeikinAshi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("HeikinAshi");
        listing.Uiid.Should().Be("HEIKINASHI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToHeikinAshi");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

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
    }

    [TestMethod]
    public void HeikinAshiStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = HeikinAshi.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("HeikinAshi");
        listing.Uiid.Should().Be("HEIKINASHI");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToHeikinAshiHub");

        listing.Parameters?.Count.Should().Be(0);
        // No parameters for this indicator

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

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
    }
}
