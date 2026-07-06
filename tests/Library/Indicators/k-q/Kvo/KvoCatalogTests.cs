namespace Catalogging;

/// <summary>
/// Test class for Kvo catalog functionality.
/// </summary>
[TestClass]
public class KvoCatalogTests : TestBase
{
    [TestMethod]
    public void KvoSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Kvo.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Klinger Volume Oscillator");
        listing.Uiid.Should().Be("KVO");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToKvo");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam1.Should().NotBeNull();
        IndicatorParam signalPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult oscillatorResult = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult.Should().NotBeNull();
        oscillatorResult?.DisplayName.Should().Be("Oscillator");
        oscillatorResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("Signal");
        signalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void KvoStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Kvo.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Klinger Volume Oscillator");
        listing.Uiid.Should().Be("KVO");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToKvoHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam.Should().NotBeNull();
        IndicatorParam signalPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult oscillatorResult = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult.Should().NotBeNull();
        oscillatorResult?.DisplayName.Should().Be("Oscillator");
        oscillatorResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult.Should().NotBeNull();
        signalResult?.DisplayName.Should().Be("Signal");
        signalResult.IsReusable.Should().Be(false);
    }
}
