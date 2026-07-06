namespace Regression;

[TestClass, TestCategory("Regression")]
public class AwesomeRegressionTests : RegressionTestBase<AwesomeResult>
{
    public AwesomeRegressionTests() : base("awesome.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToAwesome(5, 34).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToAwesomeList(5, 34).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToAwesomeHub(5, 34).Results.IsExactly(Expected);
}
