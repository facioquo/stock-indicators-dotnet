namespace Catalogging;

/// <summary>
/// Test class for ChaikinOsc catalog functionality.
/// </summary>
[TestClass]
public class ChaikinOscCatalogTests : TestBase
{
    [TestMethod]
    public void ChaikinOscSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ChaikinOsc.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Chaikin Money Flow Oscillator");
        listing.Uiid.Should().Be("CHAIKIN-OSC");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.VolumeBased);
        listing.MethodName.Should().Be("ToChaikinOsc");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam fastPeriodsParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "fastPeriods");
        fastPeriodsParam.Should().NotBeNull();
        IndicatorParam slowPeriodsParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "slowPeriods");
        slowPeriodsParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);

        IndicatorResult moneyflowmultiplierResult = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowMultiplier");
        moneyflowmultiplierResult.Should().NotBeNull();
        moneyflowmultiplierResult?.DisplayName.Should().Be("Money Flow Multiplier");
        moneyflowmultiplierResult.IsReusable.Should().Be(false);
        IndicatorResult moneyflowvolumeResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "MoneyFlowVolume");
        moneyflowvolumeResult1.Should().NotBeNull();
        moneyflowvolumeResult1?.DisplayName.Should().Be("Money Flow Volume");
        moneyflowvolumeResult1.IsReusable.Should().Be(false);
        IndicatorResult adlResult2 = listing.Results.SingleOrDefault(static r => r.DataName == "Adl");
        adlResult2.Should().NotBeNull();
        adlResult2?.DisplayName.Should().Be("ADL");
        adlResult2.IsReusable.Should().Be(false);
        IndicatorResult oscillatorResult3 = listing.Results.SingleOrDefault(static r => r.DataName == "Oscillator");
        oscillatorResult3.Should().NotBeNull();
        oscillatorResult3?.DisplayName.Should().Be("Oscillator");
        oscillatorResult3.IsReusable.Should().Be(true);
    }
}
