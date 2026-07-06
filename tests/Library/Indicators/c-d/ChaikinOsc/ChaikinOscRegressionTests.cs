namespace Regression;

[TestClass, TestCategory("Regression")]
public class ChaikinOscRegressionTests : RegressionTestBase<ChaikinOscResult>
{
    public ChaikinOscRegressionTests() : base("chaikin-osc.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToChaikinOsc(3, 10).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToChaikinOscList(3, 10).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToChaikinOscHub(3, 10).Results.IsExactly(Expected);
}
