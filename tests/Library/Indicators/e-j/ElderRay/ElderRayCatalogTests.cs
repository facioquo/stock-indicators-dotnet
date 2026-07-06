namespace Catalogging;

/// <summary>
/// Test class for ElderRay catalog functionality.
/// </summary>
[TestClass]
public class ElderRayCatalogTests : TestBase
{
    [TestMethod]
    public void ElderRaySeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ElderRay.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Elder-ray Index");
        listing.Uiid.Should().Be("ELDER-RAY");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToElderRay");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult emaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Ema");
        emaResult.Should().NotBeNull();
        emaResult?.DisplayName.Should().Be("EMA");
        emaResult.IsReusable.Should().Be(false);
        IndicatorResult bullpowerResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BullPower");
        bullpowerResult1.Should().NotBeNull();
        bullpowerResult1?.DisplayName.Should().Be("Bull Power");
        bullpowerResult1.IsReusable.Should().Be(false);
        IndicatorResult bearpowerResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "BearPower");
        bearpowerResult2.Should().NotBeNull();
        bearpowerResult2?.DisplayName.Should().Be("Bear Power");
        bearpowerResult2.IsReusable.Should().Be(false);
        IndicatorResult valueResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Value");
        valueResult3.Should().NotBeNull();
        valueResult3?.DisplayName.Should().Be("Elder Ray");
        valueResult3.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void ElderRayStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ElderRay.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Elder-ray Index");
        listing.Uiid.Should().Be("ELDER-RAY");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToElderRayHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult emaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Ema");
        emaResult.Should().NotBeNull();
        emaResult?.DisplayName.Should().Be("EMA");
        emaResult.IsReusable.Should().Be(false);
        IndicatorResult bullpowerResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BullPower");
        bullpowerResult1.Should().NotBeNull();
        bullpowerResult1?.DisplayName.Should().Be("Bull Power");
        bullpowerResult1.IsReusable.Should().Be(false);
        IndicatorResult bearpowerResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "BearPower");
        bearpowerResult2.Should().NotBeNull();
        bearpowerResult2?.DisplayName.Should().Be("Bear Power");
        bearpowerResult2.IsReusable.Should().Be(false);
        IndicatorResult valueResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Value");
        valueResult3.Should().NotBeNull();
        valueResult3?.DisplayName.Should().Be("Elder Ray");
        valueResult3.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void ElderRayBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ElderRay.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Elder-ray Index");
        listing.Uiid.Should().Be("ELDER-RAY");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToElderRayList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult emaResult = listing.Results.SingleOrDefault(static r => r.DataName == "Ema");
        emaResult.Should().NotBeNull();
        emaResult?.DisplayName.Should().Be("EMA");
        emaResult.IsReusable.Should().Be(false);
        IndicatorResult bullpowerResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "BullPower");
        bullpowerResult1.Should().NotBeNull();
        bullpowerResult1?.DisplayName.Should().Be("Bull Power");
        bullpowerResult1.IsReusable.Should().Be(false);
        IndicatorResult bearpowerResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "BearPower");
        bearpowerResult2.Should().NotBeNull();
        bearpowerResult2?.DisplayName.Should().Be("Bear Power");
        bearpowerResult2.IsReusable.Should().Be(false);
        IndicatorResult valueResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Value");
        valueResult3.Should().NotBeNull();
        valueResult3?.DisplayName.Should().Be("Elder Ray");
        valueResult3.IsReusable.Should().Be(true);
    }
}
