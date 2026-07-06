namespace Regression;

[TestClass, TestCategory("Regression")]
public class DonchianRegressionTests : RegressionTestBase<DonchianResult>
{
    public DonchianRegressionTests() : base("donchian.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToDonchian(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToDonchianList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToDonchianHub(20).Results.IsExactly(Expected);
}
