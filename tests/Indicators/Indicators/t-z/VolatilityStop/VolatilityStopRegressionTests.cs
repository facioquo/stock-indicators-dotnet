namespace Regression;

[TestClass, TestCategory("Regression")]
public class VolatilityStopRegressionTests : RegressionTestBase<VolatilityStopResult>
{
    public VolatilityStopRegressionTests() : base("vol-stop.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToVolatilityStop().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToVolatilityStopList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToVolatilityStopHub().Results.IsExactly(Expected);
}
