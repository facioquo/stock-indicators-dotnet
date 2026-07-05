namespace Catalogging;

/// <summary>
/// Tests for <see cref="CatalogListingBuilder"/> covering happy paths and validation:
/// - building a valid listing with params/results
/// - adding standard H/L/C results helper
/// - required fields (name/id/results)
/// - duplicate parameters/results handling
/// - multiple non-reusable results support
/// </summary>
[TestClass]
public class CatalogListingBuilderTests : TestBase
{
    [TestMethod]
    public void CreateValidListings()
    {
        // Arrange & Act
        IndicatorListing listing = new CatalogListingBuilder()
            .WithName("Test Indicator")
            .WithId("TEST")
            .WithStyle(Style.Series)
            .WithCategory(Category.MovingAverage)
            .AddParameter<int>("lookbackPeriods", "Lookback Period",
                description: "Test description", isRequired: true)
            .AddResult("TestResult", "Test Result", ResultType.Default, isReusable: true)
            .Build();        // Assert
        listing.Name.Should().Be("Test Indicator");
        listing.Uiid.Should().Be("TEST");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);
        IndicatorParam param = listing.Parameters[0];
        param.ParameterName.Should().Be("lookbackPeriods");
        param.DisplayName.Should().Be("Lookback Period");
        param.Description.Should().Be("Test description");
        param.DataType.Should().Be("Int32");
        param.IsRequired.Should().BeTrue();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);
        IndicatorResult result = listing.Results[0];
        result.DataName.Should().Be("TestResult");
        result.DisplayName.Should().Be("Test Result");
        result.DataType.Should().Be(ResultType.Default);
        result.IsReusable.Should().BeTrue();
    }

    [TestMethod]
    public void AddPriceHlcResult()
    {
        // Arrange & Act
        IndicatorListing listing = new CatalogListingBuilder()
            .WithName("Test Indicator")
            .WithId("TEST")
            .WithStyle(Style.Series)
            .WithCategory(Category.MovingAverage)
            .AddResult("TestResult", "Test Result", ResultType.Default, isReusable: true)
            .AddPriceHlcResult()
            .Build();

        // Assert
        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(4);
        listing.Results[0].DataName.Should().Be("TestResult");
        listing.Results[1].DataName.Should().Be("High");
        listing.Results[2].DataName.Should().Be("Low");
        listing.Results[3].DataName.Should().Be("Close");
    }

    [TestMethod]
    public void MissingName() =>
        // Arrange & Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            static () => new CatalogListingBuilder()
                .WithId("TEST")
                .WithStyle(Style.Series)
                .WithCategory(Category.MovingAverage)
                .AddResult("TestResult", "Test Result", ResultType.Default, isReusable: true)
                .Build());

    [TestMethod]
    public void MissingId() =>
        // Arrange & Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            static () => new CatalogListingBuilder()
                .WithName("Test Indicator")
                .WithStyle(Style.Series)
                .WithCategory(Category.MovingAverage)
                .AddResult("TestResult", "Test Result", ResultType.Default, isReusable: true)
                .Build());

    [TestMethod]
    public void MissingResults() =>
        // Arrange & Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            static () => new CatalogListingBuilder()
                .WithName("Test Indicator")
                .WithId("TEST")
                .WithStyle(Style.Series)
                .WithCategory(Category.MovingAverage)
                .Build());

    [TestMethod]
    public void DuplicateParameters() =>
        // Arrange & Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            static () => new CatalogListingBuilder()
                .WithName("Test Indicator")
                .WithId("TEST")
                .WithStyle(Style.Series)
                .WithCategory(Category.MovingAverage)
                .AddParameter<int>("param", "Parameter 1")
                .AddParameter<string>("param", "Parameter 2")
                .AddResult("TestResult", "Test Result", ResultType.Default, isReusable: true)
                .Build());

    [TestMethod]
    public void DuplicateResults() =>
        // Arrange & Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            static () => new CatalogListingBuilder()
                .WithName("Test Indicator")
                .WithId("TEST")
                .WithStyle(Style.Series)
                .WithCategory(Category.MovingAverage)
                .AddResult("result", "Result 1", ResultType.Default, isReusable: true)
                .AddResult("result", "Result 2", ResultType.Default)
                .Build());

    [TestMethod]
    public void MultipleResultsNoneReusable()
    {
        // Arrange & Act
        IndicatorListing result = new CatalogListingBuilder()
            .WithName("Test Indicator")
            .WithId("TEST")
            .WithStyle(Style.Series)
            .WithCategory(Category.MovingAverage)
            .AddResult("result1", "Result 1", ResultType.Default)
            .AddResult("result2", "Result 2", ResultType.Default)
            .Build();

        // Assert - This should now be valid for ISeries models
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(2);
        result.Results.Should().AllSatisfy(static r => r.IsReusable.Should().BeFalse());
    }
}
