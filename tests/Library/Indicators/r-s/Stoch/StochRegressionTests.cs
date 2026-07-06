namespace Regression;

[TestClass, TestCategory("Regression")]
public class StochRegressionTests : RegressionTestBase<StochResult>
{
    public StochRegressionTests() : base("stoch.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToStoch(14, 3, 3).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToStochList(14, 3, 3).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToStochHub(14, 3, 3).Results.IsExactly(Expected);
}
