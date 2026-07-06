namespace Catalogging;

/// <summary>
/// Test class for Roc catalog functionality.
/// </summary>
[TestClass]
public class RocCatalogTests : TestBase
{
    [TestMethod]
    public void RocSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Roc.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Rate of Change");
        listing.Uiid.Should().Be("ROC");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRoc");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void RocStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Roc.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Rate of Change");
        listing.Uiid.Should().Be("ROC");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRocHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void RocBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Roc.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Rate of Change");
        listing.Uiid.Should().Be("ROC");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToRocList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult rocResult = listing.Results.SingleOrDefault(static r => r.DataName == "Roc");
        rocResult.Should().NotBeNull();
        rocResult?.DisplayName.Should().Be("ROC");
        rocResult.IsReusable.Should().Be(true);
    }
}
