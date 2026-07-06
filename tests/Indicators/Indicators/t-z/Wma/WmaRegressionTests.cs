namespace Regression;

[TestClass, TestCategory("Regression")]
public class WmaRegressionTests : RegressionTestBase<WmaResult>
{
    public WmaRegressionTests() : base("wma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToWma(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToWmaList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToWmaHub(14).Results.IsExactly(Expected);
}
