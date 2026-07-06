namespace Catalogging;

/// <summary>
/// Test class for RocWb catalog functionality.
/// </summary>
[TestClass]
public class RocWbCatalogTests : TestBase
{
    [TestMethod]
    public void RocWbSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = RocWb.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ROC with Bands");
        listing.Uiid.Should().Be("ROC-WB");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToRocWb");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam emaPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "emaPeriods");
        emaPeriodsParam1.Should().NotBeNull();
        IndicatorParam stdDevPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "stdDevPeriods");
        stdDevPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult?.IsReusable.Should().Be(true);
        IndicatorResult rocemaResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "RocEma");
        rocemaResult1.Should().NotBeNull();
        rocemaResult1?.DisplayName.Should().Be("ROC EMA");
        rocemaResult1?.IsReusable.Should().Be(false);
        IndicatorResult upperbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult2.Should().NotBeNull();
        upperbandResult2?.DisplayName.Should().Be("Upper Band");
        upperbandResult2?.IsReusable.Should().Be(false);
        IndicatorResult lowerbandResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult3.Should().NotBeNull();
        lowerbandResult3?.DisplayName.Should().Be("Lower Band");
        lowerbandResult3?.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void RocWbStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = RocWb.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ROC with Bands");
        listing.Uiid.Should().Be("ROC-WB");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToRocWbHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam emaPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "emaPeriods");
        emaPeriodsParam1.Should().NotBeNull();
        IndicatorParam stdDevPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "stdDevPeriods");
        stdDevPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult?.IsReusable.Should().Be(true);
        IndicatorResult rocemaResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "RocEma");
        rocemaResult1.Should().NotBeNull();
        rocemaResult1?.DisplayName.Should().Be("ROC EMA");
        rocemaResult1?.IsReusable.Should().Be(false);
        IndicatorResult upperbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult2.Should().NotBeNull();
        upperbandResult2?.DisplayName.Should().Be("Upper Band");
        upperbandResult2?.IsReusable.Should().Be(false);
        IndicatorResult lowerbandResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult3.Should().NotBeNull();
        lowerbandResult3?.DisplayName.Should().Be("Lower Band");
        lowerbandResult3?.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void RocWbBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = RocWb.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("ROC with Bands");
        listing.Uiid.Should().Be("ROC-WB");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToRocWbList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam emaPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "emaPeriods");
        emaPeriodsParam1.Should().NotBeNull();
        IndicatorParam stdDevPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "stdDevPeriods");
        stdDevPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult?.IsReusable.Should().Be(true);
        IndicatorResult rocemaResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "RocEma");
        rocemaResult1.Should().NotBeNull();
        rocemaResult1?.DisplayName.Should().Be("ROC EMA");
        rocemaResult1?.IsReusable.Should().Be(false);
        IndicatorResult upperbandResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "UpperBand");
        upperbandResult2.Should().NotBeNull();
        upperbandResult2?.DisplayName.Should().Be("Upper Band");
        upperbandResult2?.IsReusable.Should().Be(false);
        IndicatorResult lowerbandResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "LowerBand");
        lowerbandResult3.Should().NotBeNull();
        lowerbandResult3?.DisplayName.Should().Be("Lower Band");
        lowerbandResult3?.IsReusable.Should().Be(false);
    }
}

