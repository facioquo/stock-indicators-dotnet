namespace Catalogging;

/// <summary>
/// Test class for TtmSqueeze catalog functionality.
/// </summary>
[TestClass]
public class TtmSqueezeCatalogTests : TestBase
{
    [TestMethod]
    public void TtmSqueezeSeries_InCatalog_ReturnsAllVariants()
        => AssertListing(TtmSqueeze.SeriesListing, Style.Series, "ToTtmSqueeze");

    [TestMethod]
    public void TtmSqueezeBuffer_InCatalog_ReturnsAllVariants()
        => AssertListing(TtmSqueeze.BufferListing, Style.Buffer, "ToTtmSqueezeList");

    [TestMethod]
    public void TtmSqueezeStream_InCatalog_ReturnsAllVariants()
        => AssertListing(TtmSqueeze.StreamListing, Style.Stream, "ToTtmSqueezeHub");

    private static void AssertListing(IndicatorListing listing, Style style, string methodName)
    {
        listing.Should().NotBeNull();
        listing.Name.Should().Be("TTM Squeeze");
        listing.Uiid.Should().Be("TTM-SQUEEZE");
        listing.Style.Should().Be(style);
        listing.Category.Should().Be(Category.Oscillator);
        listing.MethodName.Should().Be(methodName);

        listing.Parameters.Should().HaveCount(3);
        listing.Parameters.Select(static p => p.ParameterName)
            .Should().Equal("lookbackPeriods", "standardDeviations", "multiplier");

        listing.Results.Should().HaveCount(2);

        IndicatorResult momentumResult = listing.Results.SingleOrDefault(static r => r.DataName == "Momentum");
        momentumResult.Should().NotBeNull();
        momentumResult.DisplayName.Should().Be("Momentum");
        momentumResult.IsReusable.Should().BeTrue();

        IndicatorResult squeezeResult = listing.Results.SingleOrDefault(static r => r.DataName == "IsSqueezeOn");
        squeezeResult.Should().NotBeNull();
        squeezeResult.DisplayName.Should().Be("Squeeze On");
        squeezeResult.IsReusable.Should().BeFalse();
    }
}
