namespace Catalogging;

/// <summary>
/// Test class for Rsi catalog functionality.
/// </summary>
[TestClass]
public class RsiCatalogTests : TestBase
{
    [TestMethod]
    public void RsiSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Rsi.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Relative Strength Index");
        listing.Uiid.Should().Be("RSI");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRsi");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Rsi");
        rsiResult.Should().NotBeNull();
        rsiResult?.DisplayName.Should().Be("RSI");
        rsiResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void RsiBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Rsi.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Relative Strength Index");
        listing.Uiid.Should().Be("RSI");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRsiList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Rsi");
        rsiResult.Should().NotBeNull();
        rsiResult?.DisplayName.Should().Be("RSI");
        rsiResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void RsiStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Rsi.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Relative Strength Index");
        listing.Uiid.Should().Be("RSI");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRsiHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rsiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Rsi");
        rsiResult.Should().NotBeNull();
        rsiResult?.DisplayName.Should().Be("RSI");
        rsiResult.IsReusable.Should().Be(true);
    }
}
