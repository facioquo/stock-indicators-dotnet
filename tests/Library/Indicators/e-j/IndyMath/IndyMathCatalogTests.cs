namespace Catalogging;

/// <summary>
/// Test class for IndyMath catalog functionality.
/// </summary>
[TestClass]
public class IndyMathCatalogTests : TestBase
{
    // single-series operations: ID, name, Series method, parameter
    private static readonly (string Uiid, string Name, string Method, string Parameter)[] SingleSeries = [
        ("PLUS", "Plus Amount", "Plus", "amount"),
        ("MINUS", "Minus Amount", "Minus", "amount"),
        ("TIMES", "Times Factor", "Times", "factor"),
        ("DIVIDED-BY", "Divided By Divisor", "DividedBy", "divisor"),
        ("PLUS-PERCENT", "Plus Percent", "PlusPercent", "percentOffset"),
        ("ROLLING-MAX", "Rolling Max", "RollingMax", "lookbackPeriods"),
        ("ROLLING-MIN", "Rolling Min", "RollingMin", "lookbackPeriods"),
        ("ROLLING-MEDIAN", "Rolling Median", "RollingMedian", "lookbackPeriods")
    ];

    // two-series operations: ID, name, Series method
    private static readonly (string Uiid, string Name, string Method)[] Pairwise = [
        ("PLUS-SERIES", "Plus Series", "Plus"),
        ("MINUS-SERIES", "Minus Series", "Minus"),
        ("TIMES-SERIES", "Times Series", "Times"),
        ("DIVIDED-BY-SERIES", "Divided By Series", "DividedBy"),
        ("MAX-WITH", "Max With Series", "MaxWith"),
        ("MIN-WITH", "Min With Series", "MinWith")
    ];

    [TestMethod]
    public void IndyMathSeries_InCatalog_ReturnsAllVariants()
    {
        foreach ((string uiid, string name, string method, string parameter) in SingleSeries)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Series);

            AssertListing(listing, uiid, name, Style.Series, method);
            listing.Parameters.Should().ContainSingle()
                .Which.ParameterName.Should().Be(parameter);
        }

        foreach ((string uiid, string name, string method) in Pairwise)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Series);

            AssertListing(listing, uiid, name, Style.Series, method);
            listing.Parameters.Select(static p => p.ParameterName)
                .Should().Equal("source", "other");
        }
    }

    [TestMethod]
    public void IndyMathBuffer_InCatalog_ReturnsAllVariants()
    {
        foreach ((string uiid, string name, string method, string parameter) in SingleSeries)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Buffer);

            AssertListing(listing, uiid, name, Style.Buffer, $"To{method}List");
            listing.Parameters.Should().ContainSingle()
                .Which.ParameterName.Should().Be(parameter);
        }

        foreach ((string uiid, string name, string method) in Pairwise)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Buffer);

            AssertListing(listing, uiid, name, Style.Buffer, $"To{method}List");
            listing.Parameters.Select(static p => p.ParameterName)
                .Should().Equal("source", "other");
        }
    }

    [TestMethod]
    public void IndyMathStream_InCatalog_ReturnsAllVariants()
    {
        foreach ((string uiid, string name, string method, string parameter) in SingleSeries)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Stream);

            AssertListing(listing, uiid, name, Style.Stream, $"To{method}Hub");
            listing.Parameters.Should().ContainSingle()
                .Which.ParameterName.Should().Be(parameter);
        }

        foreach ((string uiid, string name, string method) in Pairwise)
        {
            IndicatorListing listing = Catalog.Get(uiid, Style.Stream);

            AssertListing(listing, uiid, name, Style.Stream, $"To{method}Hub");
            listing.Parameters.Select(static p => p.ParameterName)
                .Should().Equal("source", "other");
        }
    }

    [TestMethod]
    public void IndyMathPairwiseSeries_ExplicitSources_MatchesDirectCall()
    {
        IReadOnlyList<MathResult> viaCatalog = Catalog.Get("MINUS-SERIES", Style.Series)
            .WithParamValue("other", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<MathResult>();

        viaCatalog.IsExactly(Bars.Minus(OtherBars));
    }

    [TestMethod]
    public void IndyMathPairwiseBuffer_ExplicitSources_MatchesDirectCall()
    {
        IReadOnlyList<MathResult> viaCatalog = Catalog.Get("MINUS-SERIES", Style.Buffer)
            .WithParamValue("other", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<MathResult>();

        viaCatalog.IsExactly(Bars.Minus(OtherBars));
    }

    [TestMethod]
    public void IndyMathPairwiseStream_ExplicitSources_MatchesDirectCall()
    {
        IReadOnlyList<MathResult> viaCatalog = Catalog.Get("MINUS-SERIES", Style.Stream)
            .WithParamValue("other", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<MathResult>();

        viaCatalog.IsExactly(Bars.Minus(OtherBars));
    }

    [TestMethod]
    public void IndyMathPairwiseStream_NonBarSeries_Throws()
    {
        IReadOnlyList<IReusable> sma = OtherBars.ToSma(10);

        FluentActions.Invoking(() => Catalog.Get("MINUS-SERIES", Style.Stream)
                .WithParamValue("other", sma)
                .FromSource((IEnumerable<IBar>)Bars)
                .Execute<MathResult>())
            .Should().Throw<ArgumentException>()
            .WithMessage("*'other'*needs bars*");
    }

    [TestMethod]
    public void IndyMathScalarSeries_SharedMethodName_BindsScalarOverload()
    {
        // PLUS and PLUS-SERIES both bind "Plus"; the double argument selects the scalar form
        IReadOnlyList<MathResult> viaCatalog = Catalog.Get("PLUS", Style.Series)
            .WithParamValue("amount", 5.0)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<MathResult>();

        viaCatalog.IsExactly(Bars.Plus(5));
    }

    private static void AssertListing(
        IndicatorListing listing, string uiid, string name, Style style, string methodName)
    {
        listing.Should().NotBeNull();
        listing.Name.Should().Be(name);
        listing.Uiid.Should().Be(uiid);
        listing.Style.Should().Be(style);
        listing.Category.Should().Be(Category.PriceTransform);
        listing.MethodName.Should().Be(methodName);

        IndicatorResult result = listing.Results.Should().ContainSingle().Subject;
        result.DataName.Should().Be(nameof(MathResult.Result));
        result.IsReusable.Should().BeTrue();
    }
}
