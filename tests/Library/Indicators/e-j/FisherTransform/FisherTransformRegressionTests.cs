namespace Regression;

[TestClass, TestCategory("Regression")]
public class FisherTransformRegressionTests : RegressionTestBase<FisherTransformResult>
{
    public FisherTransformRegressionTests() : base("fisher.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToFisherTransform(10).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToFisherTransformList(10).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToFisherTransformHub(10).Results.IsExactly(Expected);
}
