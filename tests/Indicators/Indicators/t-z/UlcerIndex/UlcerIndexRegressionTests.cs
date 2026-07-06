namespace Regression;

[TestClass, TestCategory("Regression")]
public class UlcerIndexRegressionTests : RegressionTestBase<UlcerIndexResult>
{
    public UlcerIndexRegressionTests() : base("ulcer.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToUlcerIndex(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToUlcerIndexList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToUlcerIndexHub(14).Results.IsExactly(Expected);
}
