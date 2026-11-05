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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
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
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
}

[TestClass, TestCategory("Regression")]
public class IndyMathPlusSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathPlusSeriesRegressionTests() : base("plus-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Plus(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}

[TestClass, TestCategory("Regression")]
public class IndyMathMinusSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMinusSeriesRegressionTests() : base("minus-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Minus(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}

[TestClass, TestCategory("Regression")]
public class IndyMathTimesSeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathTimesSeriesRegressionTests() : base("times-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.Times(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}

[TestClass, TestCategory("Regression")]
public class IndyMathDividedBySeriesRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathDividedBySeriesRegressionTests() : base("divided-by-series.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.DividedBy(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}

[TestClass, TestCategory("Regression")]
public class IndyMathMaxWithRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMaxWithRegressionTests() : base("max-with.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.MaxWith(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}

[TestClass, TestCategory("Regression")]
public class IndyMathMinWithRegressionTests : RegressionTestBase<MathResult>
{
    public IndyMathMinWithRegressionTests() : base("min-with.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.MinWith(Bars).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Series only: two-series input has no incremental form");
}
