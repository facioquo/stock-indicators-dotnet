namespace Regression;

[TestClass, TestCategory("Regression")]
public class HmaRegressionTests : RegressionTestBase<HmaResult>
{
    public HmaRegressionTests() : base("hma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToHma().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToHmaList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToHmaHub(14).Results.IsExactly(Expected);
}
