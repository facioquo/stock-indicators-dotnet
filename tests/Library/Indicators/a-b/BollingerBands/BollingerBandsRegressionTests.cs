namespace Regression;

[TestClass, TestCategory("Regression")]
public class BollingerBandsRegressionTests : RegressionTestBase<BollingerBandsResult>
{
    public BollingerBandsRegressionTests() : base("bb.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToBollingerBands(20, 2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToBollingerBandsList(20, 2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToBollingerBandsHub(20, 2).Results.IsExactly(Expected);
}
