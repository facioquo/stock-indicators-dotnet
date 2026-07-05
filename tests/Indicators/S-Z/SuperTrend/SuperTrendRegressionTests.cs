namespace Regression;

[TestClass, TestCategory("Regression")]
public class SuperTrendRegressionTests : RegressionTestBase<SuperTrendResult>
{
    public SuperTrendRegressionTests() : base("supertrend.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToSuperTrend(10, 3).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToSuperTrendList(10, 3).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToSuperTrendHub(10, 3).Results.IsExactly(Expected);
}
