namespace Catalogging;

/// <summary>
/// Test class for MaEnvelopes catalog functionality.
/// </summary>
[TestClass]
public class MaEnvelopesCatalogTests : TestBase
{
    [TestMethod]
    public void MaEnvelopesSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = MaEnvelopes.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Moving Average Envelopes");
        listing.Uiid.Should().Be("MA-ENV");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToMaEnvelopes");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam percentOffsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "percentOffset");
        percentOffsetParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult centerlineResult = listing.Results.SingleOrDefault(static r => r.DataName == "Centerline");
        centerlineResult.Should().NotBeNull();
        centerlineResult?.DisplayName.Should().Be("Centerline");
        centerlineResult.IsReusable.Should().Be(true);
        IndicatorResult upperenvelopeResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "UpperEnvelope");
        upperenvelopeResult1.Should().NotBeNull();
        upperenvelopeResult1?.DisplayName.Should().Be("Upper Envelope");
        upperenvelopeResult1.IsReusable.Should().Be(false);
        IndicatorResult lowerenvelopeResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerEnvelope");
        lowerenvelopeResult2.Should().NotBeNull();
        lowerenvelopeResult2?.DisplayName.Should().Be("Lower Envelope");
        lowerenvelopeResult2.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void MaEnvelopesStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = MaEnvelopes.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Moving Average Envelopes");
        listing.Uiid.Should().Be("MA-ENV");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceChannel);
        listing.MethodName.Should().Be("ToMaEnvelopesHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);
    }
}
