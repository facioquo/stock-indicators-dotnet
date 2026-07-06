namespace Regression;

[TestClass, TestCategory("Regression")]
public class MarubozuRegressionTests : RegressionTestBase<CandleResult>
{
    public MarubozuRegressionTests() : base("marubozu.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToMarubozu().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToMarubozuList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToMarubozuHub().Results.IsExactly(Expected);
}
