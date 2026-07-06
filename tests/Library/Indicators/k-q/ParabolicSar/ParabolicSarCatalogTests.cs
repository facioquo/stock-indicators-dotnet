namespace Catalogging;

/// <summary>
/// Test class for ParabolicSar catalog functionality.
/// </summary>
[TestClass]
public class ParabolicSarCatalogTests : TestBase
{
    [TestMethod]
    public void ParabolicSarSeries_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ParabolicSar.SeriesListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Parabolic SAR");
        listing.Uiid.Should().Be("PSAR");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToParabolicSar");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam accelerationStepParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "accelerationStep");
        accelerationStepParam.Should().NotBeNull();
        IndicatorParam maxAccelerationFactorParam1 = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "maxAccelerationFactor");
        maxAccelerationFactorParam1.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult sarResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sar");
        sarResult.Should().NotBeNull();
        sarResult?.DisplayName.Should().Be("Parabolic SAR");
        sarResult.IsReusable.Should().Be(true);
        IndicatorResult isreversalResult1 = listing.Results.SingleOrDefault(static r => r.DataName == "IsReversal");
        isreversalResult1.Should().NotBeNull();
        isreversalResult1?.DisplayName.Should().Be("Is Reversal");
        isreversalResult1.IsReusable.Should().Be(false);
    }

    [TestMethod]
    public void ParabolicSarBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = ParabolicSar.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Parabolic SAR");
        listing.Uiid.Should().Be("PSAR");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.StopAndReverse);
        listing.MethodName.Should().Be("ToParabolicSarList");

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(2);

        IndicatorParam accelerationStepParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "accelerationStep");
        accelerationStepParam.Should().NotBeNull();
        IndicatorParam maxAccelerationFactorParam = listing.Parameters.SingleOrDefault(static p => p.ParameterName == "maxAccelerationFactor");
        maxAccelerationFactorParam.Should().NotBeNull();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(2);

        IndicatorResult sarResult = listing.Results.SingleOrDefault(static r => r.DataName == "Sar");
        sarResult.Should().NotBeNull();
        sarResult?.DisplayName.Should().Be("Parabolic SAR");
        sarResult.IsReusable.Should().Be(true);
        IndicatorResult isreversalResult = listing.Results.SingleOrDefault(static r => r.DataName == "IsReversal");
        isreversalResult.Should().NotBeNull();
        isreversalResult?.DisplayName.Should().Be("Is Reversal");
        isreversalResult.IsReusable.Should().Be(false);
    }
}
