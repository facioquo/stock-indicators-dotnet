namespace Regression;

[TestClass, TestCategory("Regression")]
public class CciRegressionTests : RegressionTestBase<CciResult>
{
    public CciRegressionTests() : base("cci.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToCci(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => new CciList(20) { Bars }.IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToCciHub(20).Results.IsExactly(Expected);
}
