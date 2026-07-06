namespace Catalogging;

/// <summary>
/// Test class for StochRsi catalog functionality.
/// </summary>
[TestClass]
public class StochRsiCatalogTests : TestBase
{
    [TestMethod]
    public void StochRsiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StochRsi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic RSI");
        listing.Uiid.Should().Be("STOCH-RSI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStochRsi");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        IndicatorParam rsiPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "rsiPeriods");
        rsiPeriodsParam.Should().NotBeNull();
        IndicatorParam stochPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "stochPeriods");
        stochPeriodsParam1.Should().NotBeNull();
        IndicatorParam signalPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam2.Should().NotBeNull();
        IndicatorParam smoothPeriodsParam3 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam3.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult stochrsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "StochRsi");
        stochrsiResult.Should().NotBeNull();
        stochrsiResult?.DisplayName.Should().Be("%K");
        stochrsiResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("%D");
        signalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void StochRsiStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StochRsi.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic RSI");
        listing.Uiid.Should().Be("STOCH-RSI");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStochRsiHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void StochRsiBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = StochRsi.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic RSI");
        listing.Uiid.Should().Be("STOCH-RSI");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStochRsiList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }
}
