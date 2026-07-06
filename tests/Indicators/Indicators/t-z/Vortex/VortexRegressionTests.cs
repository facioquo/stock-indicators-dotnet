namespace Regression;

[TestClass, TestCategory("Regression")]
public class VortexRegressionTests : RegressionTestBase<VortexResult>
{
    public VortexRegressionTests() : base("vortex.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToVortex(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToVortexList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToVortexHub(14).Results.IsExactly(Expected);
}
