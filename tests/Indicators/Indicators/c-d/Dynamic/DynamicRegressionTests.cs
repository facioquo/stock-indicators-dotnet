namespace Regression;

[TestClass, TestCategory("Regression")]
public class DynamicRegressionTests : RegressionTestBase<DynamicResult>
{
    public DynamicRegressionTests() : base("dynamic.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToDynamic(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToDynamicList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToDynamicHub(14, 0.6).Results.IsExactly(Expected);
}
