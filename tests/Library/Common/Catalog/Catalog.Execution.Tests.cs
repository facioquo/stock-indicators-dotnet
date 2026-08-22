using System.Text.Json;

namespace Catalogging;

/// <summary>
/// Catalog execution tests validating dynamic invocation by ID and JSON config:
/// - ExecuteById for RSI/EMA/SMA (with/without parameters) matching direct calls
/// - JSON-based execution (valid, minimal, invalid inputs)
/// - Error handling for invalid id/style, null bars, bad parameter types
/// - Defaults usage when required parameters are omitted
/// </summary>
[TestClass]
public class CatalogExecutionTests : TestBase
{
    [TestMethod]
    public void ExecuteByIdRsiDefault()
    {
        // Arrange
        const string id = "RSI";
        const Style style = Style.Series;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>(id, style);

        // Assert
        sut.Should().NotBeNullOrEmpty();

        IReadOnlyList<RsiResult> directResults = bars.ToRsi();
        sut.Should().HaveCount(directResults.Count);

        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteByIdRsiWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 10 } };

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>("RSI", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> directResults = bars.ToRsi(10);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteByIdEmaWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 20 } };

        // Act
        IReadOnlyList<EmaResult> sut = bars.ExecuteById<EmaResult>("EMA", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<EmaResult> directResults = bars.ToEma(20);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Ema.Should().Be(directResults[i].Ema);
        }
    }

    [TestMethod]
    public void ExecuteByIdSmaWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 10 } };

        // Act
        IReadOnlyList<SmaResult> sut = bars.ExecuteById<SmaResult>("SMA", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<SmaResult> directResults = bars.ToSma(10);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Sma.Should().Be(directResults[i].Sma);
        }
    }

    [TestMethod]
    public void ExecuteByIdInvalidInputs()
    {
        // invalid id
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action a1 = () => bars.ExecuteById<RsiResult>("INVALID_INDICATOR", Style.Series);
        a1.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");

        // null bars
        IEnumerable<IBar> nullBars = null!;
        Action a2 = () => nullBars.ExecuteById<RsiResult>("RSI", Style.Series);
        a2.Should().Throw<ArgumentNullException>().WithMessage("*bars*");

        // empty id
        Action a3 = () => bars.ExecuteById<RsiResult>(string.Empty, Style.Series);
        a3.Should().Throw<ArgumentException>().WithMessage("*ID cannot be null or empty*");

        // invalid style
        Action a4 = () => bars.ExecuteById<object>("RSI", (Style)999);
        a4.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");

        // mismatched parameter type
        Dictionary<string, object> badParams = new() { { "lookbackPeriods", "invalid_string" } };
        Action a5 = () => bars.ExecuteById<RsiResult>("RSI", Style.Series, badParams);
        a5.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ExecuteByIdMissingRequiredParametersUsesDefaults()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = [];

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>("RSI", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> defaultResults = bars.ToRsi();
        sut.Should().HaveCount(defaultResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(defaultResults[i].Timestamp);
            sut[i].Rsi.Should().Be(defaultResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteFromJsonRsiValid()
    {
        // Arrange
        IndicatorConfig config = new() {
            Id = "RSI",
            Style = Style.Series,
            Parameters = new Dictionary<string, object> { { "lookbackPeriods", 14 } }
        };
        string json = JsonSerializer.Serialize(config);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteFromJson<RsiResult>(json);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> directResults = bars.ToRsi(14);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteFromJsonMinimalConfigWorks()
    {
        IndicatorConfig config = new() { Id = "EMA", Style = Style.Series };
        string json = JsonSerializer.Serialize(config);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        IReadOnlyList<EmaResult> sut = bars.ExecuteFromJson<EmaResult>(json);
        sut.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidInputs()
    {
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        Action a1 = () => bars.ExecuteFromJson<object>("{ invalid json }");
        a1.Should().Throw<ArgumentException>().WithMessage("*Invalid JSON configuration*");

        const string nullJson = null!;
        Action a2 = () => bars.ExecuteFromJson<object>(nullJson);
        a2.Should().Throw<ArgumentNullException>().WithMessage("*json*");

        Action a3 = () => bars.ExecuteFromJson<object>(string.Empty);
        a3.Should().Throw<ArgumentException>().WithMessage("*JSON configuration cannot be null or empty*");

        IEnumerable<IBar> nullBars = null!;
        IndicatorConfig cfg = new() { Id = "RSI", Style = Style.Series };
        string json = JsonSerializer.Serialize(cfg);
        Action a4 = () => nullBars.ExecuteFromJson<object>(json);
        a4.Should().Throw<ArgumentNullException>().WithMessage("*bars*");
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidParameterTypesThrows()
    {
        const string json = /*lang=json,strict*/ """
        {
          "Id": "RSI",
          "Style": 0,
          "Parameters": {
            "lookbackPeriods": "not_a_number"
          }
        }
        """;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action act = () => bars.ExecuteFromJson<RsiResult>(json);
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidIdThrows()
    {
        const string json = /*lang=json,strict*/ """
        {
          "Id": "NONEXISTENT",
          "Style": 0
        }
        """;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action act = () => bars.ExecuteFromJson<RsiResult>(json);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");
    }

    [TestMethod]
    public void ExecuteRejectsUnknownParameterName()
    {
        // Arrange
        IndicatorListing listing = Catalog.Get("EMA", Style.Series);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act — a name the listing does not define, e.g. one left over from a
        // renamed catalog parameter, rejected at the call that is wrong
        Action act = () => listing
            .WithParams(new Dictionary<string, object> { ["lookbackPeriodz"] = 10 });

        // Assert — must fail loudly rather than silently substituting the default
        act.Should().Throw<ArgumentException>()
            .WithMessage("*lookbackPeriodz*not found in indicator*");

        // A deserialized config bypasses the builder's validation, so the executor
        // must reject the same mistake on that path too
        IndicatorConfig config = new() {
            Id = "EMA",
            Style = Style.Series,
            Parameters = new Dictionary<string, object> { ["lookbackPeriodz"] = 10 }
        };

        Action actConfig = () => config.Execute<EmaResult>(bars);

        actConfig.Should().Throw<InvalidOperationException>()
            .WithMessage("*lookbackPeriodz*is not defined*")
            .WithMessage("*Expected one of: lookbackPeriods*");
    }

    [TestMethod]
    public void ExecuteRejectsUnknownParameterNameOnParameterlessListing()
    {
        // Arrange — a listing that declares no parameters at all
        IndicatorListing listing = Catalog.Get("GATOR", Style.Series);
        listing.Should().NotBeNull();
        listing.Parameters.Should().BeNull("this listing declares no parameters");

        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act — via a deserialized config, which bypasses builder validation and
        // exercises the executor's own rejection
        IndicatorConfig config = new() {
            Id = "GATOR",
            Style = Style.Series,
            Parameters = new Dictionary<string, object> { ["lookbackPeriods"] = 10 }
        };

        Action act = () => config.Execute<GatorResult>(bars);

        // Assert — the message must say the indicator takes none, not trail off
        // with an empty list
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*lookbackPeriods*is not defined*")
            .WithMessage("*this indicator takes no parameters*");
    }

    [TestMethod]
    public void FluentBuilderKeepsBarsWhateverTheCallOrder()
    {
        IndicatorListing listing = Catalog.Get("EMA", Style.Series);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        IReadOnlyList<EmaResult> expected = bars.ToEma(10);

        // parameters first, then source
        IReadOnlyList<EmaResult> paramsFirst = listing
            .WithParamValue("lookbackPeriods", 10)
            .FromSource((IEnumerable<IBar>)bars)
            .Execute<EmaResult>();

        // source first, then parameters — the natural fluent order, which used to
        // drop the bars and throw at Execute
        IReadOnlyList<EmaResult> sourceFirst = listing
            .From((IEnumerable<IBar>)bars)
            .WithParamValue("lookbackPeriods", 10)
            .Execute<EmaResult>();

        // and via the dictionary overload, which copies state the same way
        IReadOnlyList<EmaResult> viaDictionary = listing
            .From((IEnumerable<IBar>)bars)
            .WithParams(new Dictionary<string, object> { ["lookbackPeriods"] = 10 })
            .Execute<EmaResult>();

        foreach (IReadOnlyList<EmaResult> actual in new[] { paramsFirst, sourceFirst, viaDictionary })
        {
            actual.Should().HaveCount(expected.Count);
            actual[^1].Ema.Should().Be(expected[^1].Ema);
        }
    }

    [TestMethod]
    public void AmbiguousArityResolvesToTheOverloadThatAcceptsBars()
    {
        // ToGator has two arity-one overloads — IReadOnlyList<IReusable> and
        // IReadOnlyList<AlligatorResult>. Only the first accepts bars, and which one
        // reflection returns first is unspecified, so selection must not depend on it.
        IndicatorListing listing = Catalog.Get("GATOR", Style.Series);
        listing.Should().NotBeNull();
        listing.Parameters.Should().BeNull("GATOR declares no catalog parameters");

        IReadOnlyList<Bar> bars = Bars.Take(200).ToList();

        IReadOnlyList<GatorResult> viaCatalog = listing.Execute<GatorResult>(bars);
        IReadOnlyList<GatorResult> direct = bars.ToGator();

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Upper.Should().Be(direct[^1].Upper);
        viaCatalog[^1].Lower.Should().Be(direct[^1].Lower);
    }

    [TestMethod]
    public void TwoSeriesIndicatorExecutesWithExplicitSources()
    {
        IndicatorListing listing = Catalog.Get("CORR", Style.Series);
        listing.Should().NotBeNull();

        IReadOnlyList<CorrResult> viaCatalog = listing
            .WithParams(new Dictionary<string, object> {
                ["sourceA"] = Bars,
                ["sourceB"] = OtherBars
            })
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<CorrResult>();

        IReadOnlyList<CorrResult> direct = ((IReadOnlyList<IReusable>)Bars).ToCorrelation(OtherBars, 20);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Correlation.Should().Be(direct[^1].Correlation);

        // Correlation is symmetric, so it cannot detect an A/B slot swap;
        // the per-side variances can
        viaCatalog[^1].VarianceA.Should().Be(direct[^1].VarianceA);
        viaCatalog[^1].VarianceB.Should().Be(direct[^1].VarianceB);
    }

    [TestMethod]
    public void TwoSeriesIndicatorFillsFirstSourceFromBars()
    {
        // supplying only the second series binds bars to the first, so the natural
        // "compare my bars against this benchmark" call needs one override, not two
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);
        listing.Should().NotBeNull();

        IReadOnlyList<PrsResult> viaCatalog = listing
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        IReadOnlyList<PrsResult> direct = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars, 20);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Prs.Should().Be(direct[^1].Prs);
    }

    [TestMethod]
    public void TwoSeriesIndicatorUsesCatalogDefaultsForValueParameters()
    {
        IndicatorListing listing = Catalog.Get("BETA", Style.Series);
        listing.Should().NotBeNull();

        // lookbackPeriods (50) and type (Standard) come from catalog defaults;
        // the enum default is stored as a boxed int and coerced by the binder
        IReadOnlyList<BetaResult> viaCatalog = listing
            .WithParamValue("sourceMrkt", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<BetaResult>();

        IReadOnlyList<BetaResult> direct = ((IReadOnlyList<IReusable>)Bars).ToBeta(OtherBars, 50);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Beta.Should().Be(direct[^1].Beta);
    }

    [TestMethod]
    public void TwoSeriesIndicatorRejectsMissingSecondSource()
    {
        // bars stand in for one series input only; filling a second with the same
        // data would compute a series against itself, so it must fail loudly
        IndicatorListing listing = Catalog.Get("CORR", Style.Series);

        Action act = () => listing.Execute<CorrResult>(Bars);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*'sourceB' must be supplied*")
            .WithMessage("*fills only the first series parameter*");
    }

    [TestMethod]
    public void TwoSeriesIndicatorFillsSecondSourceFromBarsWhenFirstIsNamed()
    {
        // naming the first slot leaves bars for the second — the inverse ratio,
        // reachable only by explicit choice
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        IReadOnlyList<PrsResult> viaCatalog = listing
            .WithParamValue("sourceEval", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        IReadOnlyList<PrsResult> direct = ((IReadOnlyList<IReusable>)OtherBars).ToPrs(Bars, 20);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Prs.Should().Be(direct[^1].Prs);
    }

    [TestMethod]
    public void UnnamedSeriesSourceIsRejectedOnTwoSeriesIndicators()
    {
        // an unnamed FromSource(series) would bind the first slot and demote bars
        // to the second, silently inverting an asymmetric calculation — so a
        // listing with more than one series parameter demands the name
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        Action act = () => listing.FromSource(OtherBars);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*2 series parameters*")
            .WithMessage("*sourceEval, sourceBase*")
            .WithMessage("*name the one to bind*");
    }
}
