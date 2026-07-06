namespace Catalogging;

/// <summary>
/// Test class for Correlation catalog functionality.
/// </summary>
[TestClass]
public class CorrelationCatalogTests : TestBase
{
    [TestMethod]
    public void CorrelationSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Correlation.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Correlation");
        listing.Uiid.Should().Be("CORR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be("ToCorrelation");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult varianceaResult = listing.Results.SingleOrDefault(static r => r.DataName == "VarianceA");
        varianceaResult.Should().NotBeNull();
        varianceaResult?.DisplayName.Should().Be("Variance A");
        varianceaResult.IsReusable.Should().Be(false);
        IndicatorResult variancebResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "VarianceB");
        variancebResult1.Should().NotBeNull();
        variancebResult1?.DisplayName.Should().Be("Variance B");
        variancebResult1.IsReusable.Should().Be(false);
        IndicatorResult covarianceResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Covariance");
        covarianceResult2.Should().NotBeNull();
        covarianceResult2?.DisplayName.Should().Be("Covariance");
        covarianceResult2.IsReusable.Should().Be(false);
        IndicatorResult correlationResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Correlation");
        correlationResult3.Should().NotBeNull();
        correlationResult3?.DisplayName.Should().Be("Correlation");
        correlationResult3.IsReusable.Should().Be(true);
        IndicatorResult rsquaredResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "RSquared");
        rsquaredResult4.Should().NotBeNull();
        rsquaredResult4?.DisplayName.Should().Be("R-squared");
        rsquaredResult4.IsReusable.Should().Be(false);
    }
}
