using System.Text.Json;

namespace Catalogging;

/// <summary>
/// Catalog omission tests covering overloads whose meaning is "this argument was not
/// given" rather than "this argument took its default".
/// </summary>
/// <remarks>
/// <c>ToPrs(sourceEval, sourceBase)</c> computes no <c>PrsPercent</c> at all. No value
/// in the listing's advertised range reaches that; the only override that does is the
/// <c>int.MinValue</c> sentinel the overload forwards internally, which catalog
/// metadata gives a caller no way to discover. <c>WithoutParam</c> drops the argument
/// so overload resolution lands on the shorter form without the magic constant.
/// </remarks>
[TestClass]
public class CatalogOmissionTests : TestBase
{
    [TestMethod]
    public void OmittedParameterSelectsShorterOverload()
    {
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        IReadOnlyList<PrsResult> viaCatalog = listing
            .WithParamValue("sourceBase", OtherBars)
            .WithoutParam("lookbackPeriods")
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        IReadOnlyList<PrsResult> direct = ((IReadOnlyList<IReusable>)Bars).ToPrs(OtherBars);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog.Select(static r => r.Prs).Should().Equal(direct.Select(static r => r.Prs));
        viaCatalog.Select(static r => r.PrsPercent).Should().Equal(direct.Select(static r => r.PrsPercent));
    }

    [TestMethod]
    public void OmittedParameterDiffersFromTheDeclaredDefault()
    {
        // the point of the feature: the shorter overload is a different indicator,
        // not the declared default, so it could not be reached by overriding
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        IReadOnlyList<PrsResult> withDefault = listing
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        IReadOnlyList<PrsResult> omitted = listing
            .WithParamValue("sourceBase", OtherBars)
            .WithoutParam("lookbackPeriods")
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        withDefault.Should().Contain(static r => r.PrsPercent != null,
            "the declared default of 20 computes a percent change");

        omitted.Should().OnlyContain(static r => r.PrsPercent == null,
            "the two-argument overload computes no percent change at all");

        // the ratio itself is unaffected either way
        omitted.Select(static r => r.Prs).Should().Equal(withDefault.Select(static r => r.Prs));
    }

    [TestMethod]
    public void OmittedParameterFallsBackToTheMethodDefault()
    {
        // ALMA has no shorter overload, so dropping the trailing argument leaves the
        // method's own default to apply rather than failing to bind
        IndicatorListing listing = Catalog.Get("ALMA", Style.Series);

        IReadOnlyList<AlmaResult> viaCatalog = listing
            .WithoutParam("sigma")
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<AlmaResult>();

        IReadOnlyList<AlmaResult> direct = Bars.Use(CandlePart.Close).ToAlma(9, 0.85, 6);

        viaCatalog.Should().HaveCount(direct.Count);
        viaCatalog[^1].Alma.Should().Be(direct[^1].Alma);
    }

    [TestMethod]
    public void OmissionSurvivesLaterBuilderCalls()
    {
        // each builder call returns a new instance, so omissions must be carried
        // forward the way parameter overrides and bars are
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        ListingExecutionBuilder builder = listing
            .WithoutParam("lookbackPeriods")
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars);

        builder.OmittedParameters.Should().Contain("lookbackPeriods");

        builder.Execute<PrsResult>().Should().OnlyContain(static r => r.PrsPercent == null);
    }

    [TestMethod]
    public void SupplyingAValueReversesAnEarlierOmission()
    {
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        ListingExecutionBuilder builder = listing
            .WithoutParam("lookbackPeriods")
            .WithParamValue("lookbackPeriods", 20)
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars);

        builder.OmittedParameters.Should().BeEmpty();

        builder.Execute<PrsResult>().Should().Contain(static r => r.PrsPercent != null);
    }

    [TestMethod]
    public void NonTrailingOmissionIsRejected()
    {
        // arguments bind positionally, so dropping lookbackPeriods while offset and
        // sigma are still supplied would shift both one slot left
        IndicatorListing listing = Catalog.Get("ALMA", Style.Series);

        Action act = () => listing
            .WithoutParam("lookbackPeriods")
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<AlmaResult>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot omit 'lookbackPeriods'*")
            .WithMessage("*'offset' after it is still supplied*")
            .WithMessage("*only a trailing run*");
    }

    [TestMethod]
    public void OmissionSurvivesAConfigurationRoundTrip()
    {
        // a saved configuration that dropped the omission would silently reload as a
        // different indicator, since leaving a name out of Parameters means "use the
        // declared default" rather than "pass no argument"
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        ListingExecutionBuilder original = listing
            .WithParamValue("sourceBase", OtherBars)
            .WithoutParam("lookbackPeriods");

        IndicatorConfig config = IndicatorConfig.FromBuilder(original);
        config.OmittedParameters.Should().Contain("lookbackPeriods");

        // through JSON, which is what the type exists for
        string json = JsonSerializer.Serialize(config);
        IndicatorConfig restored = JsonSerializer.Deserialize<IndicatorConfig>(json)!;
        restored.OmittedParameters.Should().Contain("lookbackPeriods");

        IReadOnlyList<PrsResult> reloaded = restored
            .ToBuilder()
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        reloaded.Should().OnlyContain(static r => r.PrsPercent == null,
            "the reloaded configuration must select the same overload as the original");
    }

    [TestMethod]
    public void ConfiguredOmissionOfAnUnknownParameterIsRejected()
    {
        // a deserialized configuration never passed through WithoutParam, so the
        // executor is where these names are first checked
        IndicatorConfig config = new() {
            Id = "PRS",
            Style = Style.Series,
            OmittedParameters = { "nonsense" }
        };

        Action act = () => config
            .ToBuilder()
            .WithParamValue("sourceBase", OtherBars)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot omit 'nonsense'*")
            .WithMessage("*not defined for indicator 'PRS'*");
    }

    [TestMethod]
    public void ConfiguredOmissionOfASeriesParameterIsRejected()
    {
        IndicatorConfig config = new() {
            Id = "PRS",
            Style = Style.Series,
            OmittedParameters = { "sourceBase" }
        };

        Action act = () => config
            .ToBuilder()
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Series parameter 'sourceBase' cannot be omitted*");
    }

    [TestMethod]
    public void OmittingAMandatoryParameterReportsTheRequestNotAnArgumentCount()
    {
        // EMA's only overload requires lookbackPeriods, so the omission cannot be met;
        // the caller should learn which request failed, not an internal arity
        IndicatorListing listing = Catalog.Get("EMA", Style.Series);

        Action act = () => listing
            .WithoutParam("lookbackPeriods")
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<EmaResult>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No form of 'ToEma' omits 'lookbackPeriods'*")
            .WithMessage("*mandatory, so supply a value instead*");
    }

    [TestMethod]
    public void OmittedParametersCannotBeMutatedThroughTheExposedSet()
    {
        // the property must not hand out the builder's own collection; mutating it
        // would reach every builder derived from this one
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        ListingExecutionBuilder builder = listing
            .WithParamValue("sourceBase", OtherBars)
            .WithoutParam("lookbackPeriods");

        ((HashSet<string>)builder.OmittedParameters).Clear();

        builder.OmittedParameters.Should().Contain("lookbackPeriods");

        builder
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<PrsResult>()
            .Should().OnlyContain(static r => r.PrsPercent == null);
    }

    [TestMethod]
    public void OmittingASeriesParameterIsRejected()
    {
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        Action act = () => listing.WithoutParam("sourceBase");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Series parameter 'sourceBase' cannot be omitted*")
            .WithMessage("*supplies the data the indicator reads*");
    }

    [TestMethod]
    public void OmittingAnUnknownParameterIsRejected()
    {
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        Action act = () => listing.WithoutParam("nonsense");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*'nonsense' not found in indicator 'PRS'*");
    }

    [TestMethod]
    public void NullParameterValueIsRejectedAndPointsAtOmission()
    {
        // a null cannot bind to these value-typed parameters, and previously failed
        // deep inside reflection with nothing naming the cause
        IndicatorListing listing = Catalog.Get("PRS", Style.Series);

        Action act = () => listing.WithParamValue("lookbackPeriods", null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*cannot be set to null*")
            .WithMessage("*WithoutParam(\"lookbackPeriods\")*");
    }
}
