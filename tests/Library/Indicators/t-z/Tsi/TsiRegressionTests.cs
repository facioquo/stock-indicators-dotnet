namespace Regression;

[TestClass, TestCategory("Regression")]
public class TsiRegressionTests : RegressionTestBase<TsiResult>
{
    public TsiRegressionTests() : base("tsi.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToTsi(25, 13, 7).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTsiList(25, 13, 7).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTsiHub(25, 13, 7).Results.IsExactly(Expected);
}
