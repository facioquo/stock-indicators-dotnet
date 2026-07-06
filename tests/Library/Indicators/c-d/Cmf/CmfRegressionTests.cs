namespace Regression;

[TestClass, TestCategory("Regression")]
public class CmfRegressionTests : RegressionTestBase<CmfResult>
{
    public CmfRegressionTests() : base("cmf.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToCmf(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToCmfList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToCmfHub(20).Results.IsExactly(Expected);
}
