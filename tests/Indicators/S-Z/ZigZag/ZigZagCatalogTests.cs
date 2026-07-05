namespace Catalogging;

/// <summary>
/// Test class for ZigZag catalog functionality.
/// </summary>
[TestClass]
public class ZigZagCatalogTests : TestBase
{
    [TestMethod]
    public void ZigZagSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ZigZag.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Zig Zag");
        listing.Uiid.Should().Be("ZIGZAG");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be("ToZigZag");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam percentChangeParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "percentChange");
        percentChangeParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult zigzagResult = listing.Results.SingleOrDefault(static r => r.DataName == "ZigZag");
        zigzagResult.Should().NotBeNull();
        zigzagResult?.DisplayName.Should().Be("Zig Zag");
        zigzagResult.IsReusable.Should().Be(true);
        IndicatorResult pointtypeResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "PointType");
        pointtypeResult1.Should().NotBeNull();
        pointtypeResult1?.DisplayName.Should().Be("Point Type");
        pointtypeResult1.IsReusable.Should().Be(false);
        IndicatorResult retracehighResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "RetraceHigh");
        retracehighResult2.Should().NotBeNull();
        retracehighResult2?.DisplayName.Should().Be("Retrace High");
        retracehighResult2.IsReusable.Should().Be(false);
        IndicatorResult retracelowResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "RetraceLow");
        retracelowResult3.Should().NotBeNull();
        retracelowResult3?.DisplayName.Should().Be("Retrace Low");
        retracelowResult3.IsReusable.Should().Be(false);
    }
}
