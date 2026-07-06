namespace Regression;

[TestClass, TestCategory("Regression")]
public class DemaRegressionTests : RegressionTestBase<DemaResult>
{
    public DemaRegressionTests() : base("dema.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToDema().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToDemaList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToDemaHub(14).Results.IsExactly(Expected);
}
