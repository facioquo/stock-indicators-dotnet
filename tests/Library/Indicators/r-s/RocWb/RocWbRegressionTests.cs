namespace Regression;

[TestClass, TestCategory("Regression")]
public class RocWbRegressionTests : RegressionTestBase<RocWbResult>
{
    public RocWbRegressionTests() : base("roc-wb.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRocWb().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRocWbList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToRocWbHub().Results.IsExactly(Expected);
}
