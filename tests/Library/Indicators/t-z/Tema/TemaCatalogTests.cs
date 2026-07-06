namespace Catalogging;

/// <summary>
/// Test class for Tema catalog functionality.
/// </summary>
[TestClass]
public class TemaCatalogTests : TestBase
{
    [TestMethod]
    public void TemaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tema.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Triple Exponential Moving Average");
        listing.Uiid.Should().Be("TEMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToTema");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult temaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tema");
        temaResult.Should().NotBeNull();
        temaResult?.DisplayName.Should().Be("TEMA");
        temaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void TemaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tema.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Triple Exponential Moving Average");
        listing.Uiid.Should().Be("TEMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToTemaList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult temaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tema");
        temaResult.Should().NotBeNull();
        temaResult?.DisplayName.Should().Be("TEMA");
        temaResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void TemaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Tema.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Triple Exponential Moving Average");
        listing.Uiid.Should().Be("TEMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToTemaHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult temaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Tema");
        temaResult.Should().NotBeNull();
        temaResult?.DisplayName.Should().Be("TEMA");
        temaResult.IsReusable.Should().Be(true);
    }
}
