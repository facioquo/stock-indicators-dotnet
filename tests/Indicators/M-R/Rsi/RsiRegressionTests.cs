namespace Regression;

[TestClass, TestCategory("Regression")]
public class RsiRegressionTests : RegressionTestBase<RsiResult>
{
    public RsiRegressionTests() : base("rsi.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRsi(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRsiList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRsiHub(14).Results.IsExactly(Expected);
}
