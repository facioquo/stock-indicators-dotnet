namespace Regression;

[TestClass, TestCategory("Regression")]
public class HeikinAshiRegressionTests : RegressionTestBase<HeikinAshiResult>
{
    public HeikinAshiRegressionTests() : base("heikinashi.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToHeikinAshi().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToHeikinAshiList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToHeikinAshiHub().Results.IsExactly(Expected);
}
