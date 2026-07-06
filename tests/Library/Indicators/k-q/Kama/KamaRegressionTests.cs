namespace Regression;

[TestClass, TestCategory("Regression")]
public class KamaRegressionTests : RegressionTestBase<KamaResult>
{
    public KamaRegressionTests() : base("kama.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToKama(10, 2, 30).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToKamaList(10, 2, 30).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToKamaHub(10, 2, 30).Results.IsExactly(Expected);
}
