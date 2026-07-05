namespace Catalogging;

/// <summary>
/// Test class for Adx catalog functionality.
/// </summary>
[TestClass]
public class AdxCatalogTests : TestBase
{
    [TestMethod]
    public void AdxSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adx.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average Directional Index (ADX)");
        listing.Uiid.Should().Be("ADX");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAdx");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult pdiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pdi");
        pdiResult.Should().NotBeNull();
        pdiResult?.DisplayName.Should().Be("+DI");
        pdiResult.IsReusable.Should().Be(false);
        IndicatorResult mdiResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mdi");
        mdiResult1.Should().NotBeNull();
        mdiResult1?.DisplayName.Should().Be("-DI");
        mdiResult1.IsReusable.Should().Be(false);
        IndicatorResult dxResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Dx");
        dxResult2.Should().NotBeNull();
        dxResult2?.DisplayName.Should().Be("DX");
        dxResult2.IsReusable.Should().Be(false);
        IndicatorResult adxResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Adx");
        adxResult3.Should().NotBeNull();
        adxResult3?.DisplayName.Should().Be("ADX");
        adxResult3.IsReusable.Should().Be(true);
        IndicatorResult adxrResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Adxr");
        adxrResult4.Should().NotBeNull();
        adxrResult4?.DisplayName.Should().Be("ADXR");
        adxrResult4.IsReusable.Should().Be(false);
    }
    [TestMethod]
    public void AdxStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adx.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average Directional Index (ADX)");
        listing.Uiid.Should().Be("ADX");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAdxHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult pdiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pdi");
        pdiResult.Should().NotBeNull();
        pdiResult?.DisplayName.Should().Be("+DI");
        pdiResult.IsReusable.Should().Be(false);
        IndicatorResult mdiResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mdi");
        mdiResult1.Should().NotBeNull();
        mdiResult1?.DisplayName.Should().Be("-DI");
        mdiResult1.IsReusable.Should().Be(false);
        IndicatorResult dxResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Dx");
        dxResult2.Should().NotBeNull();
        dxResult2?.DisplayName.Should().Be("DX");
        dxResult2.IsReusable.Should().Be(false);
        IndicatorResult adxResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Adx");
        adxResult3.Should().NotBeNull();
        adxResult3?.DisplayName.Should().Be("ADX");
        adxResult3.IsReusable.Should().Be(true);
        IndicatorResult adxrResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Adxr");
        adxrResult4.Should().NotBeNull();
        adxrResult4?.DisplayName.Should().Be("ADXR");
        adxrResult4.IsReusable.Should().Be(false);
    }
    [TestMethod]
    public void AdxBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Adx.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Average Directional Index (ADX)");
        listing.Uiid.Should().Be("ADX");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.PriceTrend);
        listing.MethodName.Should().Be("ToAdxList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(5);

        IndicatorResult pdiResult = listing.Results.SingleOrDefault(static r => r.DataName == "Pdi");
        pdiResult.Should().NotBeNull();
        pdiResult?.DisplayName.Should().Be("+DI");
        pdiResult.IsReusable.Should().Be(false);
        IndicatorResult mdiResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "Mdi");
        mdiResult1.Should().NotBeNull();
        mdiResult1?.DisplayName.Should().Be("-DI");
        mdiResult1.IsReusable.Should().Be(false);
        IndicatorResult dxResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Dx");
        dxResult2.Should().NotBeNull();
        dxResult2?.DisplayName.Should().Be("DX");
        dxResult2.IsReusable.Should().Be(false);
        IndicatorResult adxResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Adx");
        adxResult3.Should().NotBeNull();
        adxResult3?.DisplayName.Should().Be("ADX");
        adxResult3.IsReusable.Should().Be(true);
        IndicatorResult adxrResult4 = listing.Results.SingleOrDefault(static r => r.DataName == "Adxr");
        adxrResult4.Should().NotBeNull();
        adxrResult4?.DisplayName.Should().Be("ADXR");
        adxrResult4.IsReusable.Should().Be(false);
    }
}
