namespace Regression;

[TestClass, TestCategory("Regression")]
public class ObvRegressionTests : RegressionTestBase<ObvResult>
{
    public ObvRegressionTests() : base("obv.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToObv().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToObvList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToObvHub().Results.IsExactly(Expected);
}
