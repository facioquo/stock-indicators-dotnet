namespace Catalogging;

/// <summary>
/// Test class for Pvo catalog functionality.
/// </summary>
[TestClass]
public class PvoCatalogTests : TestBase
{
    [TestMethod]
    public void PvoSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Pvo.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Price Volume Oscillator");
        listing.Uiid.Should().Be("PVO");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToPvo");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam1.Should().NotBeNull();
        IndicatorParam signalPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult pvoResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pvo");
        pvoResult.Should().NotBeNull();
        pvoResult?.DisplayName.Should().Be("PVO");
        pvoResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("Signal");
        signalResult1.IsReusable.Should().Be(false);
        IndicatorResult histogramResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Histogram");
        histogramResult2.Should().NotBeNull();
        histogramResult2?.DisplayName.Should().Be("Histogram");
        histogramResult2.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void PvoBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Pvo.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Price Volume Oscillator");
        listing.Uiid.Should().Be("PVO");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToPvoList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam1.Should().NotBeNull();
        IndicatorParam signalPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult pvoResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pvo");
        pvoResult.Should().NotBeNull();
        pvoResult?.DisplayName.Should().Be("PVO");
        pvoResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("Signal");
        signalResult1.IsReusable.Should().Be(false);
        IndicatorResult histogramResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Histogram");
        histogramResult2.Should().NotBeNull();
        histogramResult2?.DisplayName.Should().Be("Histogram");
        histogramResult2.IsReusable.Should().Be(false);
    }
}
