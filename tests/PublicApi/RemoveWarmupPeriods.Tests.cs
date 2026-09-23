namespace Behavioral;

/// <summary>
/// Calls <c>RemoveWarmupPeriods()</c> the way a package consumer does: this project has no
/// internals access, so an overload the indicator pages document but the library does not
/// expose fails to compile here.
/// </summary>
[TestClass]
public class RemoveWarmupPeriodsTests
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();

    [TestMethod]
    public void WithoutConvergence_RemovesLeadingNulls()
    {
        IReadOnlyList<SmaResult> sut = bars
            .ToSma(20)
            .RemoveWarmupPeriods();

        sut.Should().HaveCount(502 - 19);
        sut[0].Sma.Should().NotBeNull();
    }

    [TestMethod]
    public void WithConvergence_UsesIndicatorOverload()
    {
        IReadOnlyList<EmaResult> sut = bars
            .ToEma(20)
            .RemoveWarmupPeriods();

        sut.Should().HaveCount(502 - (20 + 100));
    }

    [TestMethod]
    public void InGenericContext_RemovesLeadingNulls()
    {
        IReadOnlyList<SmaResult> sut = Trim(bars.ToSma(20));

        sut.Should().HaveCount(502 - 19);

        static IReadOnlyList<T> Trim<T>(IReadOnlyList<T> results)
            where T : IReusable
            => results.RemoveWarmupPeriods();
    }

    [TestMethod]
    public void WithoutConvergence_RemovesLeadingNulls_ForFormerOverloads()
    {
        IReadOnlyList<AtrStopResult> atrStop = bars.ToAtrStop();
        AssertLeadingNullsRemoved(atrStop, atrStop.RemoveWarmupPeriods(), static x => x.AtrStop is not null);

        IReadOnlyList<PivotPointsResult> pivotPoints = bars.ToPivotPoints(BarInterval.Week);
        AssertLeadingNullsRemoved(pivotPoints, pivotPoints.RemoveWarmupPeriods(), static x => x.PP is not null);

        IReadOnlyList<RollingPivotsResult> rollingPivots = bars.ToRollingPivots(11, 9);
        AssertLeadingNullsRemoved(rollingPivots, rollingPivots.RemoveWarmupPeriods(), static x => x.PP is not null);

        IReadOnlyList<SuperTrendResult> superTrend = bars.ToSuperTrend();
        AssertLeadingNullsRemoved(superTrend, superTrend.RemoveWarmupPeriods(), static x => x.SuperTrend is not null);

        static void AssertLeadingNullsRemoved<T>(
            IReadOnlyList<T> results, IReadOnlyList<T> sut, Func<T, bool> hasValue)
        {
            int leadingNulls = results.TakeWhile(x => !hasValue(x)).Count();

            leadingNulls.Should().BePositive();
            sut.Should().HaveCount(results.Count - leadingNulls);
            hasValue(sut[0]).Should().BeTrue();
        }
    }

    [TestMethod]
    public void AllWarmup_ReturnsEmpty()
    {
        IReadOnlyList<SmaResult> sut = Data.GetDefault(10)
            .ToSma(20)
            .RemoveWarmupPeriods();

        sut.Should().BeEmpty();
    }
}
