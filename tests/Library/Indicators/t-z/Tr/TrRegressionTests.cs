namespace Regression;

[TestClass, TestCategory("Regression")]
public class TrRegressionTests : RegressionTestBase<TrResult>
{
    public TrRegressionTests() : base("tr.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToTr().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTrList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTrHub().Results.IsExactly(Expected);
}
