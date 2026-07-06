namespace Regression;

[TestClass, TestCategory("Regression")]
public class RocRegressionTests : RegressionTestBase<RocResult>
{
    public RocRegressionTests() : base("roc.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRoc().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRocList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRocHub(14).Results.IsExactly(Expected);
}
