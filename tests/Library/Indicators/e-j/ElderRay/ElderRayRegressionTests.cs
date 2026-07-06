namespace Regression;

[TestClass, TestCategory("Regression")]
public class ElderRayRegressionTests : RegressionTestBase<ElderRayResult>
{
    public ElderRayRegressionTests() : base("elder-ray.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToElderRay(13).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToElderRayList(13).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToElderRayHub(13).Results.IsExactly(Expected);
}
