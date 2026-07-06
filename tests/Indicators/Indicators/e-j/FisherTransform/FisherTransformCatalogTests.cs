namespace Catalogging;

/// <summary>
/// Test class for FisherTransform catalog functionality.
/// </summary>
[TestClass]
public class FisherTransformCatalogTests : TestBase
{
    [TestMethod]
    public void FisherTransformSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = FisherTransform.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Ehlers Fisher Transform");
        listing.Uiid.Should().Be("FISHER");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToFisherTransform");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult fisherResult = listing.Results.SingleOrDefault(static r => r.DataName == "Fisher");
        fisherResult.Should().NotBeNull();
        fisherResult?.DisplayName.Should().Be("Fisher");
        fisherResult.IsReusable.Should().Be(true);
        IndicatorResult triggerResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Trigger");
        triggerResult1.Should().NotBeNull();
        triggerResult1?.DisplayName.Should().Be("Trigger");
        triggerResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void FisherTransformStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = FisherTransform.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Ehlers Fisher Transform");
        listing.Uiid.Should().Be("FISHER");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToFisherTransformHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult fisherResult = listing.Results.SingleOrDefault(static r => r.DataName == "Fisher");
        fisherResult.Should().NotBeNull();
        fisherResult?.DisplayName.Should().Be("Fisher");
        fisherResult.IsReusable.Should().Be(true);
        IndicatorResult triggerResult = listing.Results.SingleOrDefault(static r => r.DataName == "Trigger");
        triggerResult.Should().NotBeNull();
        triggerResult?.DisplayName.Should().Be("Trigger");
        triggerResult.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void FisherTransformBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = FisherTransform.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Ehlers Fisher Transform");
        listing.Uiid.Should().Be("FISHER");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToFisherTransformList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult fisherResult = listing.Results.SingleOrDefault(static r => r.DataName == "Fisher");
        fisherResult.Should().NotBeNull();
        fisherResult?.DisplayName.Should().Be("Fisher");
        fisherResult.IsReusable.Should().Be(true);
        IndicatorResult triggerResult = listing.Results.SingleOrDefault(static r => r.DataName == "Trigger");
        triggerResult.Should().NotBeNull();
        triggerResult?.DisplayName.Should().Be("Trigger");
        triggerResult.IsReusable.Should().Be(false);
    }
}
