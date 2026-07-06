namespace Regression;

[TestClass, TestCategory("Regression")]
public class ConnorsRsiRegressionTests : RegressionTestBase<ConnorsRsiResult>
{
    public ConnorsRsiRegressionTests() : base("crsi.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToConnorsRsi(3, 2, 100).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToConnorsRsiList(3, 2, 100).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToConnorsRsiHub(3, 2, 100).Results.IsExactly(Expected);
}
