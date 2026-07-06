namespace Regression;

[TestClass, TestCategory("Regression")]
public class MamaRegressionTests : RegressionTestBase<MamaResult>
{
    public MamaRegressionTests() : base("mama.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToMama(0.5, 0.05).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToMamaList(0.5, 0.05).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToMamaHub(0.5, 0.05).Results.IsExactly(Expected);
}
