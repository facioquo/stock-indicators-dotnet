namespace Catalogging;

/// <summary>
/// Test class for Beta catalog functionality.
/// </summary>
[TestClass]
public class BetaCatalogTests : TestBase
{
    [TestMethod]
    public void BetaSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Beta.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Beta");
        listing.Uiid.Should().Be("BETA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be("ToBeta");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(4);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(7);

        IndicatorResult betaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Beta");
        betaResult.Should().NotBeNull();
        betaResult?.DisplayName.Should().Be("Beta");
        betaResult.IsReusable.Should().Be(true);
        IndicatorResult betaupResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BetaUp");
        betaupResult1.Should().NotBeNull();
        betaupResult1?.DisplayName.Should().Be("Beta Up");
        betaupResult1.IsReusable.Should().Be(false);
        IndicatorResult betadownResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "BetaDown");
        betadownResult2.Should().NotBeNull();
        betadownResult2?.DisplayName.Should().Be("Beta Down");
        betadownResult2.IsReusable.Should().Be(false);
        IndicatorResult ratioResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Ratio");
        ratioResult3.Should().NotBeNull();
        ratioResult3?.DisplayName.Should().Be("Ratio");
        ratioResult3.IsReusable.Should().Be(false);
        IndicatorResult convexityResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Convexity");
        convexityResult4.Should().NotBeNull();
        convexityResult4?.DisplayName.Should().Be("Convexity");
        convexityResult4.IsReusable.Should().Be(false);
        IndicatorResult returnsevalResult5 = listing.Results.SingleOrDefault(static r => r.DataName == "ReturnsEval");
        returnsevalResult5.Should().NotBeNull();
        returnsevalResult5?.DisplayName.Should().Be("Returns Eval");
        returnsevalResult5.IsReusable.Should().Be(false);
        IndicatorResult returnsmrktResult6 = listing.Results.SingleOrDefault(static r => r.DataName == "ReturnsMrkt");
        returnsmrktResult6.Should().NotBeNull();
        returnsmrktResult6?.DisplayName.Should().Be("Returns Mrkt");
        returnsmrktResult6.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void BetaBuffer_InCatalog_ReturnsAllVariants()
        => AssertStyleListing(Beta.BufferListing, Style.Buffer, "ToBetaList");

    [TestMethod]
    public void BetaStream_InCatalog_ReturnsAllVariants()
        => AssertStyleListing(Beta.StreamListing, Style.Stream, "ToBetaHub");

    [TestMethod]
    public void BetaBufferAndStream_ExplicitSources_MatchSeries()
    {
        IReadOnlyList<BetaResult> expected = OtherBars.ToBeta(Bars, 50, BetaType.All);

        foreach (Style style in (Style[])[Style.Buffer, Style.Stream])
        {
            Catalog.Get("BETA", style)
                .WithParams(new Dictionary<string, object> {
                    ["sourceEval"] = OtherBars,
                    ["sourceMrkt"] = Bars,
                    ["type"] = BetaType.All
                })
                .FromSource((IEnumerable<IBar>)Bars)
                .Execute<BetaResult>()
                .IsExactly(expected);
        }
    }

    private static void AssertStyleListing(IndicatorListing listing, Style style, string methodName)
    {
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Beta");
        listing.Uiid.Should().Be("BETA");
        listing.Style.Should().Be(style);
        listing.Category.Should().Be(Category.PriceCharacteristic);
        listing.MethodName.Should().Be(methodName);

        listing.Parameters.Select(static p => p.ParameterName)
            .Should().Equal("sourceEval", "sourceMrkt", "lookbackPeriods", "type");
        listing.Results.Select(static r => r.DataName)
            .Should().Equal(
                nameof(BetaResult.Beta), nameof(BetaResult.BetaUp), nameof(BetaResult.BetaDown),
                nameof(BetaResult.Ratio), nameof(BetaResult.Convexity),
                nameof(BetaResult.ReturnsEval), nameof(BetaResult.ReturnsMrkt));
    }
}
