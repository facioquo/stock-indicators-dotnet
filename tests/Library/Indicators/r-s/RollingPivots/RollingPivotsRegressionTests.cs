namespace Regression;

[TestClass, TestCategory("Regression")]
public class RollingPivotsRegressionTests : RegressionTestBase<RollingPivotsResult>
{
    public RollingPivotsRegressionTests() : base("rolling-pivots.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRollingPivots().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRollingPivotsList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRollingPivotsHub().Results.IsExactly(Expected);
}
