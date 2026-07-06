namespace Catalogging;

/// <summary>
/// Test class for StdDevChannels catalog functionality.
/// </summary>
[TestClass]
public class StdDevChannelsCatalogTests : TestBase
{
    [TestMethod]
    public void StdDevChannelsSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StdDevChannels.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Standard Deviation Channels");
        listing.Uiid.Should().Be("STDEV-CHANNELS");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToStdDevChannels");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam stdDeviationsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "stdDeviations");
        stdDeviationsParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult upperchannelResult = listing.Results.SingleOrDefault(static r => r.DataName == "UpperChannel");
        upperchannelResult.Should().NotBeNull();
        upperchannelResult?.DisplayName.Should().Be("Upper Channel");
        upperchannelResult.IsReusable.Should().Be(false);
        IndicatorResult centerlineResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Centerline");
        centerlineResult1.Should().NotBeNull();
        centerlineResult1?.DisplayName.Should().Be("Centerline");
        centerlineResult1.IsReusable.Should().Be(true);
        IndicatorResult lowerchannelResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerChannel");
        lowerchannelResult2.Should().NotBeNull();
        lowerchannelResult2?.DisplayName.Should().Be("Lower Channel");
        lowerchannelResult2.IsReusable.Should().Be(false);
    }
}
