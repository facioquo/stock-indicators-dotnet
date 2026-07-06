namespace Catalogging;

/// <summary>
/// Test class for Smi catalog functionality.
/// </summary>
[TestClass]
public class SmiCatalogTests : TestBase
{
    [TestMethod]
    public void SmiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Momentum Index");
        listing.Uiid.Should().Be("SMI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToSmi");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();
        IndicatorParam firstSmoothPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "firstSmoothPeriods");
        firstSmoothPeriodsParam1.Should().NotBeNull();
        IndicatorParam secondSmoothPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "secondSmoothPeriods");
        secondSmoothPeriodsParam2.Should().NotBeNull();
        IndicatorParam signalPeriodsParam3 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "signalPeriods");
        signalPeriodsParam3.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult smiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Smi");
        smiResult.Should().NotBeNull();
        smiResult?.DisplayName.Should().Be("SMI");
        smiResult.IsReusable.Should().Be(true);
        IndicatorResult signalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Signal");
        signalResult1.Should().NotBeNull();
        signalResult1?.DisplayName.Should().Be("Signal");
        signalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void SmiBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smi.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Momentum Index");
        listing.Uiid.Should().Be("SMI");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToSmiList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }

    [TestMethod]
    public void SmiStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Smi.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Stochastic Momentum Index");
        listing.Uiid.Should().Be("SMI");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToSmiHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);
    }
}
