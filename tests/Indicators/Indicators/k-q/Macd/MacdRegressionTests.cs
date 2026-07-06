namespace Regression;

[TestClass, TestCategory("Regression")]
public class MacdRegressionTests : RegressionTestBase<MacdResult>
{
    public MacdRegressionTests() : base("macd.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToMacd(12, 26, 9).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToMacdList(12, 26, 9).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToMacdHub(12, 26, 9).Results.IsExactly(Expected);
}
