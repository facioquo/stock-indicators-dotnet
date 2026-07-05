namespace Regression;

[TestClass, TestCategory("Regression")]
public class AtrStopRegressionTests : RegressionTestBase<AtrStopResult>
{
    public AtrStopRegressionTests() : base("atr-stop.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToAtrStop(21, 3).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToAtrStopList(21, 3).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToAtrStopHub(21, 3).Results.IsExactly(Expected);
}
