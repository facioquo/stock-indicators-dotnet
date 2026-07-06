namespace Regression;

[TestClass, TestCategory("Regression")]
public class BetaRegressionTests : RegressionTestBase<BetaResult>
{
    public BetaRegressionTests() : base("beta.standard.json") { }

    private const int n = 50;

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.ToBeta(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
}
