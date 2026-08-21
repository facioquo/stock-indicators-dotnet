using System.Reflection;

namespace Catalogging;

/// <summary>
/// Catalog executability tests covering listings that could not be run at all:
/// - a listing whose method declares optional parameters the catalog does not enumerate
/// - listings whose parameters are <c>decimal</c> rather than <c>double</c>
/// </summary>
/// <remarks>
/// The binding tests check that names resolve. These check that a listing can actually
/// be invoked, which is a separate failure mode: a name can be correct while the
/// argument list the executor assembles matches no overload, or carries the wrong CLR
/// type. Both classes produced listings that threw for every caller.
/// </remarks>
[TestClass]
public class CatalogExecutabilityTests : TestBase
{
    [TestMethod]
    public void ListingExecutesWhenMethodHasUndeclaredOptionalParameters()
    {
        // ICHIMOKU's Buffer method takes senkouOffset and chikouOffset, which the
        // listing does not declare. The executor must supply their defaults rather
        // than fail to find a matching overload.
        IndicatorListing listing = Catalog.Get("ICHIMOKU", Style.Buffer);
        listing.Should().NotBeNull();

        IReadOnlyList<IchimokuResult> results = listing.Execute<IchimokuResult>(Bars);

        results.Should().NotBeNullOrEmpty();
        results.Should().HaveCount(Bars.Count);
    }

    [TestMethod]
    public void ListingExecutesWhenParametersAreDecimal()
    {
        IndicatorListing renko = Catalog.Get("RENKO", Style.Series);
        renko.Should().NotBeNull();
        renko.Parameters.Single(static p => p.ParameterName == "brickSize")
            .DataType.Should().Be("Decimal", "the bound method takes a decimal brick size");

        IReadOnlyList<RenkoResult> renkoResults = renko.Execute<RenkoResult>(Bars);
        renkoResults.Should().NotBeNullOrEmpty();

        IndicatorListing zigZag = Catalog.Get("ZIGZAG", Style.Series);
        zigZag.Should().NotBeNull();
        zigZag.Parameters.Single(static p => p.ParameterName == "percentChange")
            .DataType.Should().Be("Decimal", "the bound method takes a decimal percent change");

        IReadOnlyList<ZigZagResult> zigZagResults = zigZag.Execute<ZigZagResult>(Bars);
        zigZagResults.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void DecimalParameterRejectsDoubleOverride()
    {
        IndicatorListing renko = Catalog.Get("RENKO", Style.Series);

        // a double cannot bind to a decimal parameter; fail with a usable message
        // rather than at reflection-invoke time
        Action act = () => renko.WithParamValue("brickSize", 2.5);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*expects a decimal value*2.5m*");
    }

    [TestMethod]
    public void DecimalParameterAcceptsDecimalOverride()
    {
        IndicatorListing renko = Catalog.Get("RENKO", Style.Series);

        IReadOnlyList<RenkoResult> results = renko
            .WithParamValue("brickSize", 2.5m)
            .FromSource((IEnumerable<IBar>)Bars)
            .Execute<RenkoResult>();

        results.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Indicators that need a second input series, which the executor cannot supply.
    /// </summary>
    /// <remarks>
    /// <c>ListingExecutor</c> binds exactly one bars source, so a listing that also
    /// declares a source series always assembles one argument too many. That is a
    /// capability the executor does not have rather than a defect in these listings,
    /// and it is tracked separately. Excluded by name so the count below stays a real
    /// assertion instead of a moving target.
    /// </remarks>
    private static readonly HashSet<string> TwoSeriesIndicators
        = new(StringComparer.Ordinal) { "BETA", "CORR", "PRS" };

    [TestMethod]
    public void EverySeriesAndBufferListingExecutes()
    {
        MethodInfo executeDefinition = typeof(ListingExecutionBuilderExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(static m => m.Name == "Execute"
                             && m.IsGenericMethodDefinition
                             && m.GetParameters().Length == 2
                             && m.GetParameters()[0].ParameterType == typeof(IndicatorListing));

        List<IndicatorListing> executable = Catalog.Get()
            .Where(static l => l.Style is Style.Series or Style.Buffer && !TwoSeriesIndicators.Contains(l.Uiid))
            .ToList();

        List<string> failures = [];

        foreach (IndicatorListing listing in executable)
        {
            // resolve the result record from the listing's own metadata, so this
            // exercises ResultRecordType on the same path a consumer would use
            Type resultType = CatalogReflection.FindPublicType(listing.ResultRecordType);

            if (resultType is null)
            {
                failures.Add($"{CatalogReflection.Describe(listing)}: ResultRecordType "
                           + $"'{listing.ResultRecordType}' does not resolve to a public type");
                continue;
            }

            try
            {
                object results = executeDefinition
                    .MakeGenericMethod(resultType)
                    .Invoke(null, [listing, Bars]);

                if (results is null)
                {
                    failures.Add($"{CatalogReflection.Describe(listing)}: returned null");
                }
            }
            catch (TargetInvocationException ex)
            {
                failures.Add($"{CatalogReflection.Describe(listing)} ({listing.MethodName}): "
                           + $"{ex.InnerException?.Message ?? ex.Message}");
            }
        }

        string.Join(Environment.NewLine, failures).Should().BeEmpty(
            "every Series and Buffer listing must be executable from its own catalog metadata; "
          + "a listing that cannot run is metadata describing something a consumer cannot invoke");

        // guard the exclusion list itself: if the executor gains multi-series support,
        // this count changes and the exclusion should be revisited rather than kept
        executable.Should().HaveCount(
            Catalog.Get().Count(static l => l.Style is Style.Series or Style.Buffer)
          - TwoSeriesIndicators.Count,
            "exactly the known two-series indicators are excluded");
    }
}
