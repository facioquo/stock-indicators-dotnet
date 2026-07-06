namespace Regression;

[TestClass, TestCategory("Regression")]
public class PivotPointsRegressionTests : RegressionTestBase<PivotPointsResult>
{
    public PivotPointsRegressionTests() : base("pivot-points.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToPivotPoints().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToPivotPointsList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToPivotPointsHub().Results.IsExactly(Expected);
}
