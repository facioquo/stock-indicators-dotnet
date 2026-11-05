namespace Regression;

[TestClass, TestCategory("Regression")]
public class IndyMathPlusRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathPlusRegressionTests() : base("plus.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.Plus(1).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToPlusList(1).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToPlusHub(1).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathMinusRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMinusRegressionTests() : base("minus.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.Minus(1).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToMinusList(1).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToMinusHub(1).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathTimesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathTimesRegressionTests() : base("times.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.Times(2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTimesList(2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTimesHub(2).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathDividedByRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathDividedByRegressionTests() : base("divided-by.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.DividedBy(2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToDividedByList(2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToDividedByHub(2).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathPlusPercentRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathPlusPercentRegressionTests() : base("plus-percent.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.PlusPercent(2.5).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToPlusPercentList(2.5).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToPlusPercentHub(2.5).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathRollingMaxRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathRollingMaxRegressionTests() : base("rolling-max.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.RollingMax(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRollingMaxList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRollingMaxHub(20).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathRollingMinRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathRollingMinRegressionTests() : base("rolling-min.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.RollingMin(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRollingMinList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRollingMinHub(20).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathRollingMedianRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathRollingMedianRegressionTests() : base("rolling-median.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.RollingMedian(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRollingMedianList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRollingMedianHub(20).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathPlusSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathPlusSeriesRegressionTests() : base("plus-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Plus(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToPlusList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToPlusHub(BarHub).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathMinusSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMinusSeriesRegressionTests() : base("minus-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Minus(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToMinusList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToMinusHub(BarHub).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathTimesSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathTimesSeriesRegressionTests() : base("times-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Times(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToTimesList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToTimesHub(BarHub).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathDividedBySeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathDividedBySeriesRegressionTests() : base("divided-by-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.DividedBy(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToDividedByList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToDividedByHub(BarHub).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathMaxWithRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMaxWithRegressionTests() : base("max-with.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.MaxWith(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToMaxWithList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToMaxWithHub(BarHub).Results.IsExactly(Expected);
}

[TestClass, TestCategory("Regression")]
public class IndyMathMinWithRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMinWithRegressionTests() : base("min-with.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.MinWith(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToMinWithList(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => IndyMathStreamSources.OtherBarHub.ToMinWithHub(BarHub).Results.IsExactly(Expected);
}

/// <summary>
/// Stream source for the second series of the two-series regression tests.
/// </summary>
internal static class IndyMathStreamSources
{
    internal static BarHub OtherBarHub { get; } = CreateOtherBarHub();

    private static BarHub CreateOtherBarHub()
    {
        BarHub hub = new();
        hub.Add(TestBase.OtherBars);
        return hub;
    }
}
