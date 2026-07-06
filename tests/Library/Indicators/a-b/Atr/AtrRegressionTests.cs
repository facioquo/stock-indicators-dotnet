namespace Regression;

[TestClass, TestCategory("Regression")]
public class AtrRegressionTests : RegressionTestBase<AtrResult>
{
    public AtrRegressionTests() : base("atr.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToAtr(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToAtrList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToAtrHub(14).Results.IsExactly(Expected);
}
