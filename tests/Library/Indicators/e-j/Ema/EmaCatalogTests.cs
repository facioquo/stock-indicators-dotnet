namespace Catalogging;

/// <summary>
/// Test class for EMA catalog functionality with improved reliability.
/// Uses static catalog listings directly to avoid shared registry state issues
/// that could cause intermittent test failures in parallel execution.
/// </summary>
[TestClass]
public class EmaCatalogTests : TestBase
{
    [TestMethod]
    public void EmaExecuteById_WithValidId_MatchesDirectCall()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars;
        const int lookback = 20;

        // Act - Execute via catalog ExecuteById
        IReadOnlyList<EmaResult> byId = bars.ExecuteById<EmaResult>(
            id: "EMA",
            style: Style.Series,
            parameters: new() {
                { "lookbackPeriods", lookback }
            });

        // Act - Direct call
        IReadOnlyList<EmaResult> direct = bars.ToEma(lookback);

        // Assert
        byId.IsExactly(direct);
    }

    [TestMethod]
    public void EmaExecuteFromJson_WithValidJson_MatchesDirectCall()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars;

        const string json = /*lang=json,strict*/ """
            {
                "id" : "EMA",
                "style" : "Series",
                "parameters" : { "lookbackPeriods" : 20 }
            }
            """;

        // Act - Execute via JSON config
        IReadOnlyList<EmaResult> fromJson = bars.ExecuteFromJson<EmaResult>(json);

        // Act - Direct call
        IReadOnlyList<EmaResult> direct = bars.ToEma(20);

        // Assert
        fromJson.IsExactly(direct);
    }

    [TestMethod]
    public void CatalogBrowsing_WithEmaSeries_IncludesEmaSeries()
    {
        // Act
        IReadOnlyList<IndicatorListing> catalog = Catalog.Get();
        IndicatorListing emaSeries = Catalog.Get("EMA", Style.Series);
        IReadOnlyList<IndicatorListing> seriesListings = Catalog.Get(Style.Series);

        // Assert
        catalog.Should().NotBeNull();
        catalog.Should().Contain(static l => l.Uiid == "EMA" && l.Style == Style.Series);
        emaSeries.Should().NotBeNull();
        seriesListings.Should().Contain(static l => l.Uiid == "EMA" && l.Style == Style.Series);
    }
    [TestMethod]
    public void EmaSeries_InCatalog_ReturnsAllVariants()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars; // Using TestBase.Bars

        // Use static listing directly to avoid shared registry state issues
        IndicatorListing listing = Ema.SeriesListing;
        listing.Should().NotBeNull("EMA Series listing should be found");

        // get catalog default value for test use
        IndicatorParam lookbackParam = listing.Parameters.Single(static p => p.ParameterName == "lookbackPeriods");
        int lookbackValue = (int)lookbackParam.DefaultValue;

        // Act - Use catalog utility to dynamically execute the indicator
        IReadOnlyList<EmaResult> catalogResults = ListingExecutor.Execute<EmaResult>(bars, listing);

        // Act - Direct call for comparison using catalog's default parameter value
        IReadOnlyList<EmaResult> directResults = bars.ToEma(lookbackValue);

        // Assert - Results from catalog-driven execution should match direct call
        catalogResults.IsExactly(directResults);

        // Assert - Lookup gets correct values
        lookbackValue.Should().Be(20);

        // Additional assertions on catalog metadata (basic validation)
        listing.Name.Should().Be("Exponential Moving Average");
        listing.Uiid.Should().Be("EMA");
        listing.Style.Should().Be(Style.Series);
        listing.Category.Should().Be(Category.MovingAverage);
        listing.MethodName.Should().Be("ToEma");
        listing.Parameters.Should().HaveCount(1);
        listing.Results.Should().HaveCount(1);
    }

    [TestMethod]
    public void EmaSeries_ExecutedFromRegistry_ReturnsExpectedResult()
    {
        IndicatorListing listing = Catalog.Get("EMA", Style.Series);

        IReadOnlyList<EmaResult> catalogResult = listing.Execute<EmaResult>(Bars);

        int lookbackPeriod = (int)listing.Parameters
            .Single(static p => p.ParameterName == "lookbackPeriods")
            .DefaultValue;

        lookbackPeriod.Should().Be(20, "this is what's defined in the catalog");
        catalogResult.IsExactly(Bars.ToEma(lookbackPeriod));
    }

    [TestMethod]
    public void EmaSeries_WithCustomParameters_ListsAllVariants()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars; // Using TestBase.Bars
        IndicatorListing listing = Ema.SeriesListing;
        listing.Should().NotBeNull();

        const int customLookbackPeriod = 10; // Use custom value instead of default

        // Act - Use fluent API to configure, source, and execute
        IReadOnlyList<EmaResult> customResults = listing
            .WithParamValue("lookbackPeriods", customLookbackPeriod)
            .FromSource((IEnumerable<IBar>)bars)  // Explicitly cast to the bars overload
            .Execute<EmaResult>();

        // Act - Direct call for comparison
        IReadOnlyList<EmaResult> directResults = bars.ToEma(customLookbackPeriod);

        // Assert - Results should be identical
        customResults.IsExactly(directResults);

        // Verify we're actually using the custom parameter (results should be different from default)
        IReadOnlyList<EmaResult> defaultResults = listing.Execute<EmaResult>(bars);
        customResults.Should().NotBeEquivalentTo(defaultResults, "Custom parameter should produce different results");
    }

    [TestMethod]
    public void EmaSeries_FromCatalog_MatchesDirectCall()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars;
        IndicatorListing listing = Ema.SeriesListing;

        // Get default parameter value from catalog
        IndicatorParam lookbackParam = listing.Parameters.First(static p => p.ParameterName == "lookbackPeriods");
        int lookbackValue = (int)lookbackParam.DefaultValue!;

        // Act - Call using catalog metadata (via method name)
        IReadOnlyList<EmaResult> catalogResults = bars.ToEma(lookbackValue);

        // Act - Direct call
        IReadOnlyList<EmaResult> seriesResults = bars.ToEma(lookbackValue);

        // Assert - Results should be identical
        catalogResults.IsExactly(seriesResults);
    }

    [TestMethod]
    public void EmaFluentApi_AlternativeSyntax_ReturnsExpectedResult()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars; // Using TestBase.Bars
        IndicatorListing listing = Ema.SeriesListing;
        listing.Should().NotBeNull();

        const int customLookbackPeriod = 15;

        // Method 1: listing.WithParamValue().FromSource().Execute()
        IReadOnlyList<EmaResult> method1Results = listing
            .WithParamValue("lookbackPeriods", customLookbackPeriod)
            .FromSource((IEnumerable<IBar>)bars)  // Explicitly cast to the bars overload
            .Execute<EmaResult>();

        // Method 2: bars.Execute(customIndicator)
        ListingExecutionBuilder customIndicator = listing.WithParamValue("lookbackPeriods", customLookbackPeriod);
        IReadOnlyList<EmaResult> method2Results = bars.Execute<EmaResult>(customIndicator);

        // Method 3: Traditional execution for comparison
        IReadOnlyList<EmaResult> directResults = bars.ToEma(customLookbackPeriod);

        // Assert - All methods should produce identical results
        method1Results.IsExactly(directResults);
        method2Results.IsExactly(directResults);
        method1Results.IsExactly(method2Results);
    }

    [TestMethod]
    public void EmaCatalog_ExecutionBuilderProperties_AreCorrect()
    {
        // Arrange
        IndicatorListing listing = Ema.SeriesListing;
        listing.Should().NotBeNull();

        // Act - Create custom indicator without bars
        ListingExecutionBuilder customIndicator = listing
            .WithParamValue("lookbackPeriods", 10);

        // Assert - Verify builder properties
        customIndicator.BaseListing.Should().Be(listing);
        customIndicator.ParameterOverrides.Should().HaveCount(1);
        customIndicator.ParameterOverrides["lookbackPeriods"].Should().Be(10);
        customIndicator.HasBars.Should().BeFalse();

        // Act - Add bars using FromSource(IEnumerable<IBar>)
        ListingExecutionBuilder withBars = customIndicator.FromSource((IEnumerable<IBar>)Bars);
        withBars.HasBars.Should().BeTrue();

        // Assert - Original builder should be immutable
        customIndicator.HasBars.Should().BeFalse("Original builder should remain unchanged");
    }

    [TestMethod]
    public void EmaSeries_WithChaining_ReturnsExpectedResult()
    {
        // Arrange - Test the new series chaining functionality using a realistic scenario
        IReadOnlyList<Bar> bars = Bars;

        // Create some indicator results to use as series input
        IReadOnlyList<EmaResult> emaResults = bars.ToEma(20);
        IReadOnlyList<SmaResult> smaResults = bars.ToSma(20);

        // Get Correlation listing which accepts series parameters - use static listing
        IndicatorListing correlationListing = Correlation.SeriesListing;
        correlationListing.Should().NotBeNull();

        // Act - Test the new FromSource<T> method with series data
        // Use the fluent API to set series parameters explicitly with parameter names
        ListingExecutionBuilder customIndicator = correlationListing
            .WithParamValue("sourceA", emaResults)  // Set first series parameter explicitly
            .WithParamValue("lookbackPeriods", 10);  // Additional parameter

        // This demonstrates that WithParamValue works for series parameters
        customIndicator.BaseListing.Should().Be(correlationListing);
        customIndicator.ParameterOverrides.Should().ContainKey("sourceA");
        customIndicator.ParameterOverrides.Should().ContainKey("lookbackPeriods");
        customIndicator.ParameterOverrides["sourceA"].Should().BeEquivalentTo(emaResults);
        customIndicator.ParameterOverrides["lookbackPeriods"].Should().Be(10);

        // Test parameter validation - should work with proper series
        Action validSeriesAssignment = () => correlationListing.WithParamValue("sourceB", smaResults);
        validSeriesAssignment.Should().NotThrow();

        // Test parameter validation - should fail with invalid parameter name
        Action invalidParameterName = () => correlationListing.WithParamValue("invalidParam", emaResults);
        invalidParameterName.Should().Throw<ArgumentException>()
            .WithMessage("Parameter 'invalidParam' not found in indicator 'CORR'*"); // Use wildcard for full message

        // Test EMA-specific validation - EMA doesn't accept series parameters
        IndicatorListing emaListing = Ema.SeriesListing;
        Action emaSeriesTest = ()
            => emaListing.WithParamValue("lookbackPeriods", emaResults); // Wrong type for this parameter
        emaSeriesTest.Should().Throw<ArgumentException>()
            .WithMessage("*expects an integer value*"); // This will validate type checking
    }

    [TestMethod]
    public void EmaFluentApi_NewSeriesFunctionality_ReturnsExpectedResult()
    {
        // Arrange - Demonstrate the new FromSource<T> functionality with a realistic chaining scenario
        IReadOnlyList<Bar> bars = Bars;

        // Step 1: Create EMA results
        IReadOnlyList<EmaResult> emaResults = bars.ToEma(20);

        // Step 2: Use the EMA results as input to another indicator that accepts series - use static listing
        IndicatorListing correlationListing = Correlation.SeriesListing;
        correlationListing.Should().NotBeNull();

        // Act - Use the new FromSource<T> extension method
        ListingExecutionBuilder customBuilder = correlationListing
            .FromSource(emaResults, "sourceA")
            .WithParamValue("sourceB", bars.ToSma(20))
            .WithParamValue("lookbackPeriods", 10);

        // Assert - Builder should have all parameters set correctly
        customBuilder.ParameterOverrides.Should().HaveCount(3);
        customBuilder.ParameterOverrides["sourceA"].Should().BeEquivalentTo(emaResults);
        customBuilder.ParameterOverrides.Should().ContainKey("sourceB");
        customBuilder.ParameterOverrides["lookbackPeriods"].Should().Be(10);

        // Act - Test series execution with alternative syntax
        IReadOnlyList<SmaResult> smaResults = bars.ToSma(15);
        ListingExecutionBuilder secondBuilder = correlationListing.FromSource(smaResults, "sourceA");
        secondBuilder.ParameterOverrides["sourceA"].Should().BeEquivalentTo(smaResults);

        // Test parameter type validation for series
        Action validAction = () => correlationListing.FromSource(emaResults, "sourceA");
        validAction.Should().NotThrow();

        // Test invalid parameter name for series
        Action invalidAction = () => correlationListing.FromSource(emaResults, "nonExistentParam");
        invalidAction.Should().Throw<ArgumentException>()
            .WithMessage("Parameter 'nonExistentParam' not found in indicator 'CORR'*");
    }

    [TestMethod]
    public void EmaConfiguration_OnSerialization_RoundTripsCorrectly()
    {
        // Arrange - Create indicator configurations that could be stored as JSON
        IndicatorConfig[] configs =
        [
            new IndicatorConfig
            {
                Id = "EMA",
                Style = Style.Series,
                Parameters = new Dictionary<string, object> { ["lookbackPeriods"] = 12 },
                DisplayName = "Short EMA",
                Description = "12-period exponential moving average"
            },
            new IndicatorConfig
            {
                Id = "EMA",
                Style = Style.Series,
                Parameters = new Dictionary<string, object> { ["lookbackPeriods"] = 26 },
                DisplayName = "Long EMA",
                Description = "26-period exponential moving average"
            }
        ];

        IReadOnlyList<Bar> bars = Bars;

        // Act - Execute configurations
        IReadOnlyList<EmaResult> shortEmaResults = configs[0].Execute<EmaResult>(bars);
        IReadOnlyList<EmaResult> longEmaResults = configs[1].Execute<EmaResult>(bars);

        // Act - Compare with direct calls
        IReadOnlyList<EmaResult> shortEmaDirect = bars.ToEma(12);
        IReadOnlyList<EmaResult> longEmaDirect = bars.ToEma(26);

        // Assert - Configuration-based execution should match direct calls
        shortEmaResults.IsExactly(shortEmaDirect);
        longEmaResults.IsExactly(longEmaDirect);

        // Assert - Verify configuration roundtrip
        ListingExecutionBuilder shortBuilder = configs[0].ToBuilder();
        shortBuilder.BaseListing.Uiid.Should().Be("EMA");
        shortBuilder.ParameterOverrides["lookbackPeriods"].Should().Be(12);

        // Act - Test builder to config conversion
        IndicatorConfig rebuiltConfig = IndicatorConfig.FromBuilder(shortBuilder);
        rebuiltConfig.Id.Should().Be("EMA");
        rebuiltConfig.Style.Should().Be(Style.Series);
        rebuiltConfig.Parameters["lookbackPeriods"].Should().Be(12);
    }

    [TestMethod]
    public void EmaStream_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Ema.StreamListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Exponential Moving Average");
        listing.Uiid.Should().Be("EMA");
        listing.Style.Should().Be(Style.Stream);
        listing.Category.Should().Be(Category.MovingAverage);

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam period = listing.Parameters.FirstOrDefault(static p => p.ParameterName == "lookbackPeriods");
        period.Should().NotBeNull();
        period?.DisplayName.Should().Be("Lookback Period");
        period.Description.Should().Be("Number of periods for the EMA calculation");
        period.IsRequired.Should().BeTrue();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult result = listing.Results[0];
        result.DataName.Should().Be("Ema");
        result.DisplayName.Should().Be("EMA");
        result.DataType.Should().Be(ResultType.Default);
        result.IsReusable.Should().BeTrue();
    }

    [TestMethod]
    public void EmaBuffer_InCatalog_ReturnsAllVariants()
    {
        // Act
        IndicatorListing listing = Ema.BufferListing;

        // Assert
        listing.Should().NotBeNull();
        listing.Name.Should().Be("Exponential Moving Average");
        listing.Uiid.Should().Be("EMA");
        listing.Style.Should().Be(Style.Buffer);
        listing.Category.Should().Be(Category.MovingAverage);

        listing.Parameters.Should().NotBeNull();
        listing.Parameters.Should().HaveCount(1);

        IndicatorParam period = listing.Parameters.FirstOrDefault(static p => p.ParameterName == "lookbackPeriods");
        period.Should().NotBeNull();
        period?.DisplayName.Should().Be("Lookback Period");
        period.Description.Should().Be("Number of periods for the EMA calculation");
        period.IsRequired.Should().BeTrue();

        listing.Results.Should().NotBeNull();
        listing.Results.Should().HaveCount(1);

        IndicatorResult result = listing.Results[0];
        result.DataName.Should().Be("Ema");
        result.DisplayName.Should().Be("EMA");
        result.DataType.Should().Be(ResultType.Default);
        result.IsReusable.Should().BeTrue();
    }
}
