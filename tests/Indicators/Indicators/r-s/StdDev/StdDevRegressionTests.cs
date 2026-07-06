namespace Regression;

[TestClass, TestCategory("Regression")]
public class StdDevRegressionTests : RegressionTestBase<StdDevResult>
{
    public StdDevRegressionTests() : base("stdev.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToStdDev().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToStdDevList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToStdDevHub(14).Results.IsExactly(Expected);
}
