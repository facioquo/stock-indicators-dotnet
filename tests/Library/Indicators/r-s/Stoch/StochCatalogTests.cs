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
        listing.Parameters.Should().HaveCount(6);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam signalPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam1.Should().NotBeNull();
        IndicatorParam smoothPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "smoothPeriods");
        smoothPeriodsParam2.Should().NotBeNull();

        IndicatorParam kFactorParam = listing.Parameters.Single(static p => p.ParameterName == "kFactor");
        kFactorParam.DefaultValue.Should().Be(3.0);
        IndicatorParam dFactorParam = listing.Parameters.Single(static p => p.ParameterName == "dFactor");
        dFactorParam.DefaultValue.Should().Be(2.0);
        IndicatorParam movingAverageTypeParam = listing.Parameters.Single(static p => p.ParameterName == "movingAverageType");
        movingAverageTypeParam.DefaultValue.Should().Be((int)MaType.SMA);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult oscillatorResult = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult.Should().NotBeNull();
        oscillatorResult?.DisplayName.Should().Be("%K");
        oscillatorResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("%D");
        signalResult1.IsReusable.Should().Be(false);
        IndicatorResult percentJResult = listing.Results.Single(static r => r.DataName == "PercentJ");
        percentJResult.DisplayName.Should().Be("%J");
        percentJResult.IsReusable.Should().Be(false);
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
        listing.Parameters.Should().HaveCount(6);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);
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
        listing.Parameters.Should().HaveCount(6);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);
    }
}
