namespace Regression;

[TestClass, TestCategory("Regression")]
public class EpmaRegressionTests : RegressionTestBase<EpmaResult>
{
    public EpmaRegressionTests() : base("epma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToEpma().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToEpmaList(10).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToEpmaHub(10).Results.IsExactly(Expected);
}
