namespace Catalogging;

/// <summary>
/// Test class for Cmo catalog functionality.
/// </summary>
[TestClass]
public class CmoCatalogTests : TestBase
{
    [TestMethod]
    public void CmoSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Cmo.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chande Momentum Oscillator");
        listing.Uiid.Should().Be("CMO");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToCmo");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult cmoResult = listing.Results.SingleOrDefault(static r => r.DataName == "Cmo");
        cmoResult.Should().NotBeNull();
        cmoResult?.DisplayName.Should().Be("CMO");
        cmoResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void CmoBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Cmo.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chande Momentum Oscillator");
        listing.Uiid.Should().Be("CMO");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToCmoList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult cmoResult = listing.Results.SingleOrDefault(static r => r.DataName == "Cmo");
        cmoResult.Should().NotBeNull();
        cmoResult?.DisplayName.Should().Be("CMO");
        cmoResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void CmoStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Cmo.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chande Momentum Oscillator");
        listing.Uiid.Should().Be("CMO");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToCmoHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult cmoResult = listing.Results.SingleOrDefault(static r => r.DataName == "Cmo");
        cmoResult.Should().NotBeNull();
        cmoResult?.DisplayName.Should().Be("CMO");
        cmoResult.IsReusable.Should().Be(true);
    }
}
