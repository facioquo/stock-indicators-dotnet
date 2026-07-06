namespace Regression;

[TestClass, TestCategory("Regression")]
public class EmaRegressionTests : RegressionTestBase<EmaResult>
{
    public EmaRegressionTests() : base("ema.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToEma(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToEmaList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToEmaHub(20).Results.IsExactly(Expected);
}
