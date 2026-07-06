namespace Catalogging;

/// <summary>
/// Test class for Cmf catalog functionality.
/// </summary>
[TestClass]
public class CmfCatalogTests : TestBase
{
    [TestMethod]
    public void CmfSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Cmf.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chaikin Money Flow (CMF)");
        listing.Uiid.Should().Be("CMF");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToCmf");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult moneyflowmultiplierResult = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowMultiplier");
        moneyflowmultiplierResult.Should().NotBeNull();
        moneyflowmultiplierResult?.DisplayName.Should().Be("Money Flow Multiplier");
        moneyflowmultiplierResult.IsReusable.Should().Be(false);
        IndicatorResult moneyflowvolumeResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowVolume");
        moneyflowvolumeResult1.Should().NotBeNull();
        moneyflowvolumeResult1?.DisplayName.Should().Be("Money Flow Volume");
        moneyflowvolumeResult1.IsReusable.Should().Be(false);
        IndicatorResult cmfResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Cmf");
        cmfResult2.Should().NotBeNull();
        cmfResult2?.DisplayName.Should().Be("CMF");
        cmfResult2.IsReusable.Should().Be(true);
    }

    [TestMethod]
    public void CmfStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Cmf.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chaikin Money Flow (CMF)");
        listing.Uiid.Should().Be("CMF");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToCmfHub");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam lookbackPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "lookbackPeriods");
        lookbackPeriodsParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(3);

        IndicatorResult moneyflowmultiplierResult = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowMultiplier");
        moneyflowmultiplierResult.Should().NotBeNull();
        moneyflowmultiplierResult?.DisplayName.Should().Be("Money Flow Multiplier");
        moneyflowmultiplierResult.IsReusable.Should().Be(false);
        IndicatorResult moneyflowvolumeResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowVolume");
        moneyflowvolumeResult1.Should().NotBeNull();
        moneyflowvolumeResult1?.DisplayName.Should().Be("Money Flow Volume");
        moneyflowvolumeResult1.IsReusable.Should().Be(false);
        IndicatorResult cmfResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Cmf");
        cmfResult2.Should().NotBeNull();
        cmfResult2?.DisplayName.Should().Be("CMF");
        cmfResult2.IsReusable.Should().Be(true);
    }
}
