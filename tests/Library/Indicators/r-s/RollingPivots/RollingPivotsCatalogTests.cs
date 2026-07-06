namespace Catalogging;

/// <summary>
/// Test class for RollingPivots catalog functionality.
/// </summary>
[TestClass]
public class RollingPivotsCatalogTests : TestBase
{
    [TestMethod]
    public void RollingPivotsSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = RollingPivots.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Rolling Pivots");
        listing.Uiid.Should().Be("ROLLING-PIVOTS");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToRollingPivots");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(3);

        IndicatorParam windowPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "windowPeriods");
        windowPeriodsParam.Should().NotBeNull();
        IndicatorParam offsetPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "offsetPeriods");
        offsetPeriodsParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(7);

        IndicatorResult r3Result = listing.Results.SingleOrDefault(static r => r.DataName == "R3");
        r3Result.Should().NotBeNull();
        r3Result?.DisplayName.Should().Be("Resistance 3");
        r3Result.IsReusable.Should().Be(false);
        IndicatorResult r2Result1 = listing.Results.SingleOrDefault(static r => r.DataName == "R2");
        r2Result1.Should().NotBeNull();
        r2Result1?.DisplayName.Should().Be("Resistance 2");
        r2Result1.IsReusable.Should().Be(false);
        IndicatorResult r1Result2 = listing.Results.SingleOrDefault(static r => r.DataName == "R1");
        r1Result2.Should().NotBeNull();
        r1Result2?.DisplayName.Should().Be("Resistance 1");
        r1Result2.IsReusable.Should().Be(false);
        IndicatorResult ppResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "PP");
        ppResult3.Should().NotBeNull();
        ppResult3?.DisplayName.Should().Be("Pivot Point");
        ppResult3.IsReusable.Should().Be(true);
        IndicatorResult s1Result4 = listing.Results.SingleOrDefault(static r => r.DataName == "S1");
        s1Result4.Should().NotBeNull();
        s1Result4?.DisplayName.Should().Be("Support 1");
        s1Result4.IsReusable.Should().Be(false);
        IndicatorResult s2Result5 = listing.Results.SingleOrDefault(static r => r.DataName == "S2");
        s2Result5.Should().NotBeNull();
        s2Result5?.DisplayName.Should().Be("Support 2");
        s2Result5.IsReusable.Should().Be(false);
        IndicatorResult s3Result6 = listing.Results.SingleOrDefault(static r => r.DataName == "S3");
        s3Result6.Should().NotBeNull();
        s3Result6?.DisplayName.Should().Be("Support 3");
        s3Result6.IsReusable.Should().Be(false);
    }
}
