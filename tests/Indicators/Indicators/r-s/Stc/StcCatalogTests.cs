namespace Catalogging;

/// <summary>
/// Test class for Stc catalog functionality.
/// </summary>
[TestClass]
public class StcCatalogTests : TestBase
{
    [TestMethod]
    public void StcSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stc.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Schaff Trend Cycle");
        listing.Uiid.Should().Be("STC");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStc");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam cyclePeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "cyclePeriods");
        cyclePeriodsParam.Should().NotBeNull();
        IndicatorParam fastPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam1.Should().NotBeNull();
        IndicatorParam slowPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult stcResult = listing.Results.SingleOrDefault(static r => r.DataName == "Stc");
        stcResult.Should().NotBeNull();
        stcResult?.DisplayName.Should().Be("STC");
        stcResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void StcBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stc.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Schaff Trend Cycle");
        listing.Uiid.Should().Be("STC");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStcList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam cyclePeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "cyclePeriods");
        cyclePeriodsParam.Should().NotBeNull();
        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult stcResult = listing.Results.SingleOrDefault(static r => r.DataName == "Stc");
        stcResult.Should().NotBeNull();
        stcResult?.DisplayName.Should().Be("STC");
        stcResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void StcStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stc.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Schaff Trend Cycle");
        listing.Uiid.Should().Be("STC");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStcHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam cyclePeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "cyclePeriods");
        cyclePeriodsParam.Should().NotBeNull();
        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult stcResult = listing.Results.SingleOrDefault(static r => r.DataName == "Stc");
        stcResult.Should().NotBeNull();
        stcResult?.DisplayName.Should().Be("STC");
        stcResult.IsReusable.Should().Be(true);
    }
}
