namespace Regression;

[TestClass, TestCategory("Regression")]
public class SmmaRegressionTests : RegressionTestBase<SmmaResult>
{
    public SmmaRegressionTests() : base("smma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToSmma(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToSmmaList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToSmmaHub(20).Results.IsExactly(Expected);
}
