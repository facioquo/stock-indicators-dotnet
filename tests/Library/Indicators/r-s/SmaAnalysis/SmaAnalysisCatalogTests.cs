namespace Catalogging;

/// <summary>
/// Test class for SmaAnalysis catalog functionality.
/// </summary>
[TestClass]
public class SmaAnalysisCatalogTests : TestBase
{
    [TestMethod]
    public void SmaAnalysisSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = SmaAnalysis.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Simple Moving Average Analysis");
        listing.Uiid.Should().Be("SMA-ANALYSIS");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSmaAnalysis");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult smaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sma");
        smaResult.Should().NotBeNull();
        smaResult?.DisplayName.Should().Be("SMA");
        smaResult.IsReusable.Should().Be(true);
        IndicatorResult madResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mad");
        madResult1.Should().NotBeNull();
        madResult1?.DisplayName.Should().Be("Mean absolute deviation");
        madResult1.IsReusable.Should().Be(false);
        IndicatorResult mseResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Mse");
        mseResult2.Should().NotBeNull();
        mseResult2?.DisplayName.Should().Be("Mean square error");
        mseResult2.IsReusable.Should().Be(false);
        IndicatorResult mapeResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Mape");
        mapeResult3.Should().NotBeNull();
        mapeResult3?.DisplayName.Should().Be("Mean absolute percentage error");
        mapeResult3.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void SmaAnalysisStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = SmaAnalysis.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Simple Moving Average Analysis");
        listing.Uiid.Should().Be("SMA-ANALYSIS");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSmaAnalysisHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult smaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sma");
        smaResult.Should().NotBeNull();
        smaResult!.DisplayName.Should().Be("SMA");
        smaResult.IsReusable.Should().Be(true);
        IndicatorResult madResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mad");
        madResult1.Should().NotBeNull();
        madResult1!.DisplayName.Should().Be("Mean absolute deviation");
        madResult1.IsReusable.Should().Be(false);
        IndicatorResult mseResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Mse");
        mseResult2.Should().NotBeNull();
        mseResult2!.DisplayName.Should().Be("Mean square error");
        mseResult2.IsReusable.Should().Be(false);
        IndicatorResult mapeResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Mape");
        mapeResult3.Should().NotBeNull();
        mapeResult3!.DisplayName.Should().Be("Mean absolute percentage error");
        mapeResult3.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void SmaAnalysisBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = SmaAnalysis.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Simple Moving Average Analysis");
        listing.Uiid.Should().Be("SMA-ANALYSIS");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToSmaAnalysisList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult smaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sma");
        smaResult.Should().NotBeNull();
        smaResult!.DisplayName.Should().Be("SMA");
        smaResult.IsReusable.Should().Be(true);
        IndicatorResult madResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mad");
        madResult1.Should().NotBeNull();
        madResult1!.DisplayName.Should().Be("Mean absolute deviation");
        madResult1.IsReusable.Should().Be(false);
        IndicatorResult mseResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Mse");
        mseResult2.Should().NotBeNull();
        mseResult2!.DisplayName.Should().Be("Mean square error");
        mseResult2.IsReusable.Should().Be(false);
        IndicatorResult mapeResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Mape");
        mapeResult3.Should().NotBeNull();
        mapeResult3!.DisplayName.Should().Be("Mean absolute percentage error");
        mapeResult3.IsReusable.Should().Be(false);
    }
}
