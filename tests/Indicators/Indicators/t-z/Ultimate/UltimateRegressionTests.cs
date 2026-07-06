namespace Regression;

[TestClass, TestCategory("Regression")]
public class UltimateRegressionTests : RegressionTestBase<UltimateResult>
{
    public UltimateRegressionTests() : base("uo.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToUltimate(7, 14, 28).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToUltimateList(7, 14, 28).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToUltimateHub(7, 14, 28).Results.IsExactly(Expected);
}
