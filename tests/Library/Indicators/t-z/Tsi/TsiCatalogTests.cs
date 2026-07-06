namespace Catalogging;

/// <summary>
/// Test class for Tsi catalog functionality.
/// </summary>
[TestClass]
public class TsiCatalogTests : TestBase
{
    [TestMethod]
    public void TsiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tsi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("True Strength Index");
        listing.Uiid.Should().Be("TSI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToTsi");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam smoothPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam1.Should().NotBeNull();
        IndicatorParam signalPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult tsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tsi");
        tsiResult.Should().NotBeNull();
        tsiResult?.DisplayName.Should().Be("TSI");
        tsiResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("Signal");
        signalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void TsiStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tsi.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("True Strength Index");
        listing.Uiid.Should().Be("TSI");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToTsiHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam smoothPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam.Should().NotBeNull();
        IndicatorParam signalPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult tsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tsi");
        tsiResult.Should().NotBeNull();
        tsiResult?.DisplayName.Should().Be("TSI");
        tsiResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult.Should().NotBeNull();
        signalResult?.DisplayName.Should().Be("Signal");
        signalResult.IsReusable.Should().Be(false);
    }
}
