namespace Regression;

[TestClass, TestCategory("Regression")]
public class WilliamsRRegressionTests : RegressionTestBase<WilliamsResult>
{
    public WilliamsRRegressionTests() : base("willr.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToWilliamsR(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToWilliamsRList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToWilliamsRHub(14).Results.IsExactly(Expected);
}
