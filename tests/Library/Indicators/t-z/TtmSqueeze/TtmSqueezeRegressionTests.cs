namespace Regression;

[TestClass, TestCategory("Regression")]
public class TtmSqueezeRegressionTests : RegressionTestBase<TtmSqueezeResult>
{
    public TtmSqueezeRegressionTests() : base("ttm-squeeze.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToTtmSqueeze(20, 2, 1.5).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTtmSqueezeList(20, 2, 1.5).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTtmSqueezeHub(20, 2, 1.5).Results.IsExactly(Expected);
}
