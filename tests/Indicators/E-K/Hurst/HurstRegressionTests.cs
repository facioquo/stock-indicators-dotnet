namespace Regression;

[TestClass, TestCategory("Regression")]
public class HurstRegressionTests : RegressionTestBase<HurstResult>
{
    public HurstRegressionTests() : base("hurst.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToHurst().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToHurstList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToHurstHub().Results.IsExactly(Expected);
}
