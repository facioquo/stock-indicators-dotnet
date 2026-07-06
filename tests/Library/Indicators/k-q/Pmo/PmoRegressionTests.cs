namespace Regression;

[TestClass, TestCategory("Regression")]
public class PmoRegressionTests : RegressionTestBase<PmoResult>
{
    public PmoRegressionTests() : base("pmo.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToPmo(35, 20, 10).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToPmoList(35, 20, 10).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToPmoHub(35, 20, 10).Results.IsExactly(Expected);
}
