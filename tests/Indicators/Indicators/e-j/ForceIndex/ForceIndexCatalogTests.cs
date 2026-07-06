namespace Catalogging;

/// <summary>
/// Test class for ForceIndex catalog functionality.
/// </summary>
[TestClass]
public class ForceIndexCatalogTests : TestBase
{
    [TestMethod]
    public void ForceIndexSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ForceIndex.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Force Index");
        listing.Uiid.Should().Be("FORCE");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToForceIndex");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult forceindexResult = listing.Results.SingleOrDefault(static r => r.DataName == "ForceIndex");
        forceindexResult.Should().NotBeNull();
        forceindexResult?.DisplayName.Should().Be("Force Index");
        forceindexResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void ForceIndexBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ForceIndex.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Force Index");
        listing.Uiid.Should().Be("FORCE");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToForceIndexList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult forceindexResult = listing.Results.SingleOrDefault(static r => r.DataName == "ForceIndex");
        forceindexResult.Should().NotBeNull();
        forceindexResult?.DisplayName.Should().Be("Force Index");
        forceindexResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void ForceIndexStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ForceIndex.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Force Index");
        listing.Uiid.Should().Be("FORCE");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToForceIndexHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult forceindexResult = listing.Results.SingleOrDefault(static r => r.DataName == "ForceIndex");
        forceindexResult.Should().NotBeNull();
        forceindexResult?.DisplayName.Should().Be("Force Index");
        forceindexResult.IsReusable.Should().Be(true);
    }
}
