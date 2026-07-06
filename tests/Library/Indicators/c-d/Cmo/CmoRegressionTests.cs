namespace Regression;

[TestClass, TestCategory("Regression")]
public class CmoRegressionTests : RegressionTestBase<CmoResult>
{
    public CmoRegressionTests() : base("cmo.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToCmo(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToCmoList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToCmoHub(14).Results.IsExactly(Expected);
}
