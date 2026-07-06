namespace Catalogging;

/// <summary>
/// Test class for Alligator catalog functionality.
/// </summary>
[TestClass]
public class AlligatorCatalogTests : TestBase
{
    [TestMethod]
    public void AlligatorSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alligator.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Alligator");
        listing.Uiid.Should().Be("ALLIGATOR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAlligator");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(6);

        IndicatorParam jawPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawPeriods");
        jawPeriodsParam.Should().NotBeNull();
        IndicatorParam jawOffsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawOffset");
        jawOffsetParam1.Should().NotBeNull();
        IndicatorParam teethPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethPeriods");
        teethPeriodsParam2.Should().NotBeNull();
        IndicatorParam teethOffsetParam3 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethOffset");
        teethOffsetParam3.Should().NotBeNull();
        IndicatorParam lipsPeriodsParam4 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsPeriods");
        lipsPeriodsParam4.Should().NotBeNull();
        IndicatorParam lipsOffsetParam5 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsOffset");
        lipsOffsetParam5.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult jawResult = listing.Results.SingleOrDefault(static r => r.DataName == "Jaw");
        jawResult.Should().NotBeNull();
        jawResult?.DisplayName.Should().Be("Jaw");
        jawResult.IsReusable.Should().Be(false);
        IndicatorResult teethResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Teeth");
        teethResult1.Should().NotBeNull();
        teethResult1?.DisplayName.Should().Be("Teeth");
        teethResult1.IsReusable.Should().Be(false);
        IndicatorResult lipsResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Lips");
        lipsResult2.Should().NotBeNull();
        lipsResult2?.DisplayName.Should().Be("Lips");
        lipsResult2.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AlligatorStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alligator.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Alligator");
        listing.Uiid.Should().Be("ALLIGATOR");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAlligatorHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(6);

        IndicatorParam jawPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawPeriods");
        jawPeriodsParam.Should().NotBeNull();
        IndicatorParam jawOffsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawOffset");
        jawOffsetParam1.Should().NotBeNull();
        IndicatorParam teethPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethPeriods");
        teethPeriodsParam2.Should().NotBeNull();
        IndicatorParam teethOffsetParam3 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethOffset");
        teethOffsetParam3.Should().NotBeNull();
        IndicatorParam lipsPeriodsParam4 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsPeriods");
        lipsPeriodsParam4.Should().NotBeNull();
        IndicatorParam lipsOffsetParam5 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsOffset");
        lipsOffsetParam5.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult jawResult = listing.Results.SingleOrDefault(static r => r.DataName == "Jaw");
        jawResult.Should().NotBeNull();
        jawResult?.DisplayName.Should().Be("Jaw");
        jawResult.IsReusable.Should().Be(false);
        IndicatorResult teethResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Teeth");
        teethResult1.Should().NotBeNull();
        teethResult1?.DisplayName.Should().Be("Teeth");
        teethResult1.IsReusable.Should().Be(false);
        IndicatorResult lipsResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Lips");
        lipsResult2.Should().NotBeNull();
        lipsResult2?.DisplayName.Should().Be("Lips");
        lipsResult2.IsReusable.Should().Be(true);
    }
    [TestMethod]
    public void AlligatorBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Alligator.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Williams Alligator");
        listing.Uiid.Should().Be("ALLIGATOR");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAlligatorList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(6);

        IndicatorParam jawPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawPeriods");
        jawPeriodsParam.Should().NotBeNull();
        IndicatorParam jawOffsetParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "jawOffset");
        jawOffsetParam1.Should().NotBeNull();
        IndicatorParam teethPeriodsParam2 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethPeriods");
        teethPeriodsParam2.Should().NotBeNull();
        IndicatorParam teethOffsetParam3 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "teethOffset");
        teethOffsetParam3.Should().NotBeNull();
        IndicatorParam lipsPeriodsParam4 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsPeriods");
        lipsPeriodsParam4.Should().NotBeNull();
        IndicatorParam lipsOffsetParam5 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lipsOffset");
        lipsOffsetParam5.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult jawResult = listing.Results.SingleOrDefault(static r => r.DataName == "Jaw");
        jawResult.Should().NotBeNull();
        jawResult?.DisplayName.Should().Be("Jaw");
        jawResult.IsReusable.Should().Be(false);
        IndicatorResult teethResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Teeth");
        teethResult1.Should().NotBeNull();
        teethResult1?.DisplayName.Should().Be("Teeth");
        teethResult1.IsReusable.Should().Be(false);
        IndicatorResult lipsResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Lips");
        lipsResult2.Should().NotBeNull();
        lipsResult2?.DisplayName.Should().Be("Lips");
        lipsResult2.IsReusable.Should().Be(true);
    }
}
