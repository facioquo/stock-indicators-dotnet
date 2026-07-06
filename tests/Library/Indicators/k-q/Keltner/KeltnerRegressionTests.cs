namespace Regression;

[TestClass, TestCategory("Regression")]
public class KeltnerRegressionTests : RegressionTestBase<KeltnerResult>
{
    public KeltnerRegressionTests() : base("keltner.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToKeltner(20, 2, 10).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToKeltnerList(20, 2, 10).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToKeltnerHub(20, 2, 10).Results.IsExactly(Expected);
}
