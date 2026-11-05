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

    [TestMethod]
    public void CorrelationBuffer_InCatalog_ReturnsAllVariants()
        => AssertStyleListing(Correlation.BufferListing, Style.Buffer, "ToCorrelationList");

    [TestMethod]
    public void CorrelationStream_InCatalog_ReturnsAllVariants()
        => AssertStyleListing(Correlation.StreamListing, Style.Stream, "ToCorrelationHub");

    [TestMethod]
    public void CorrelationBufferAndStream_ExplicitSources_MatchSeries()
    {
        IReadOnlyList<CorrResult> expected = OtherBars.ToCorrelation(Bars, 20);

        foreach (Style style in (Style[])[Style.Buffer, Style.Stream])
        {
            Catalog.Get("CORR", style)
                .WithParams(new Dictionary<string, object> {
                    ["sourceA"] = OtherBars,
                    ["sourceB"] = Bars
                })
                .FromSource((IEnumerable<IBar>)Bars)
                .Execute<CorrResult>()
                .IsExactly(expected);
        }
    }

    private static void AssertStyleListing(IndicatorListing listing, Style style, string methodName)
    {
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Correlation");
        listing.Uiid.Should().Be("CORR");
        listing.Style.Should().Be(style);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be(methodName);

        listing.Parameters.Select(static p => p.ParameterName)
            .Should().Equal("sourceA", "sourceB", "lookbackPeriods");
        listing.Results.Select(static r => r.DataName)
            .Should().Equal(
                nameof(CorrResult.VarianceA), nameof(CorrResult.VarianceB), nameof(CorrResult.Covariance),
                nameof(CorrResult.Correlation), nameof(CorrResult.RSquared));
    }
}
