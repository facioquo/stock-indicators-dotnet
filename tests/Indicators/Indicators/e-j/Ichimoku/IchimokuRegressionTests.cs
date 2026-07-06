namespace Regression;

[TestClass, TestCategory("Regression")]
public class IchimokuRegressionTests : RegressionTestBase<IchimokuResult>
{
    public IchimokuRegressionTests() : base("ichimoku.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToIchimoku(9, 26, 52).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToIchimokuList(9, 26, 52).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToIchimokuHub(9, 26, 52).Results.IsExactly(Expected);
}
