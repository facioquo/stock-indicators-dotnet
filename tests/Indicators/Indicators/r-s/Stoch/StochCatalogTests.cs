namespace Catalogging;

/// <summary>
/// Test class for Stoch catalog functionality.
/// </summary>
[TestClass]
public class StochCatalogTests : TestBase
{
    [TestMethod]
    public void StochSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stoch.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Oscillator");
        listing.Uiid.Should().Be("STOCH");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStoch");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam signalPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam1.Should().NotBeNull();
        IndicatorParam smoothPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam2.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult oscillatorResult = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult.Should().NotBeNull();
        oscillatorResult?.DisplayName.Should().Be("%K");
        oscillatorResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("%D");
        signalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void StochStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stoch.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Oscillator");
        listing.Uiid.Should().Be("STOCH");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStochHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void StochBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Stoch.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Oscillator");
        listing.Uiid.Should().Be("STOCH");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToStochList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }
}
