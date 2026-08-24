using System.Reflection;

namespace Utilities;

/// <summary>
/// Tests for the two obsolete v3 tuple shims that accept an <c>smaPeriods</c> argument
/// they cannot honor.
/// </summary>
/// <remarks>
/// v2 returned a moving average alongside the indicator — <c>PrsResult.PrsSma</c> and
/// <c>RocResult.RocSma</c>. Neither property exists in v3, so the argument can never be
/// applied; the guidance is to chain <c>ToSma(..)</c> onto the replacement's results.
/// <para>
/// The bar-based overloads carrying that argument are <c>Obsolete(.., true)</c>, so
/// passing it is a compile error that names the replacement. The tuple overloads warn
/// about tuple removal and said nothing about the dropped argument, so the same request
/// compiled and was then discarded. These pin the corrected messages and the behavior
/// they describe.
/// </para>
/// </remarks>
[TestClass]
public class ObsoleteShimTupleSmaPeriodsTests : TestBase
{
    /// <summary>
    /// Resolves an obsolete shim overload by parameter type.
    /// </summary>
    /// <remarks>
    /// The containing <c>Indicator</c> class is itself <c>Obsolete(.., true)</c>, so
    /// <c>typeof(Indicator)</c> is a CS0619 error that <c>#pragma warning disable</c>
    /// cannot suppress. Resolving the type by name is the only way to reach it.
    /// Overloads are matched on parameter <em>type</em> rather than name, because
    /// renaming a parameter is non-breaking for positional callers and should not fail
    /// these tests.
    /// </remarks>
    private static MethodInfo TupleOverload(string name)
    {
        Type indicator = typeof(Catalog).Assembly
            .GetType("FacioQuo.Stock.Indicators.Indicator");

        indicator.Should().NotBeNull(
            "the obsolete shim class must exist for these shims to be reachable at all");

        MethodInfo method = indicator
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == name
                               && m.GetParameters()[0].ParameterType
                                  == typeof(IEnumerable<(DateTime, double)>)
                               && m.GetParameters().Any(p => p.ParameterType == typeof(int)
                                                          || p.ParameterType == typeof(int?)));

        method.Should().NotBeNull($"'{name}' must have a tuple-receiver overload");

        return method;
    }

    [TestMethod]
    public void PrsTupleShimMessageStatesSmaPeriodsIsIgnored()
        => AssertMessageDisclosesIgnoredArgument("GetPrs", "PrsSma");

    [TestMethod]
    public void RocTupleShimMessageStatesSmaPeriodsIsIgnored()
        => AssertMessageDisclosesIgnoredArgument("GetRoc", "RocSma");

    private static void AssertMessageDisclosesIgnoredArgument(string method, string absentResult)
    {
        ObsoleteAttribute obsolete = TupleOverload(method)
            .GetCustomAttribute<ObsoleteAttribute>();

        obsolete.Should().NotBeNull();

        // asserted as independent tokens rather than one phrase, so rewording the
        // message for clarity does not fail the build
        obsolete.Message.Should().Contain("smaPeriods");
        obsolete.Message.Should().Contain("ignored");
        obsolete.Message.Should().Contain(absentResult);
        obsolete.Message.Should().Contain("ToSma");
    }

    [TestMethod]
    public void PrsTupleShimIgnoresSmaPeriodsWithoutAffectingResults()
    {
        IEnumerable<(DateTime d, double v)> evalTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

        IEnumerable<(DateTime d, double v)> baseTuples
            = OtherBars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618 // exercising the obsolete shim is the point
        List<PrsResult> withSma = evalTuples.GetPrs(baseTuples, 20, 5).ToList();
        List<PrsResult> withoutSma = evalTuples.GetPrs(baseTuples, 20).ToList();
#pragma warning restore CS0618

        // the argument changes nothing, which is what the message now says
        withSma.Select(static r => r.Prs).Should().Equal(withoutSma.Select(static r => r.Prs));
        withSma.Select(static r => r.PrsPercent).Should().Equal(withoutSma.Select(static r => r.PrsPercent));
    }

    [TestMethod]
    public void RocTupleShimIgnoresSmaPeriodsWithoutAffectingResults()
    {
        IEnumerable<(DateTime d, double v)> priceTuples
            = Bars.Select(static b => (b.Timestamp, (double)b.Close));

#pragma warning disable CS0618
        List<RocResult> withSma = priceTuples.GetRoc(20, 5).ToList();
        List<RocResult> withoutSma = priceTuples.GetRoc(20).ToList();
#pragma warning restore CS0618

        withSma.Select(static r => r.Roc).Should().Equal(withoutSma.Select(static r => r.Roc));
        withSma.Select(static r => r.Momentum).Should().Equal(withoutSma.Select(static r => r.Momentum));
    }

    [TestMethod]
    public void ResultRecordsHaveNoSmaProperty()
    {
        // the reason the argument cannot be honored: there is nowhere to put the value.
        // If either property is ever restored, these shims should apply the argument
        // rather than document ignoring it.
        typeof(PrsResult).GetProperty("PrsSma").Should().BeNull();
        typeof(RocResult).GetProperty("RocSma").Should().BeNull();
    }
}
