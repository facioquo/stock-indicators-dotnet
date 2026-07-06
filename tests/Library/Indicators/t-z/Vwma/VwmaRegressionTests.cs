namespace Regression;

[TestClass, TestCategory("Regression")]
public class VwmaRegressionTests : RegressionTestBase<VwmaResult>
{
    public VwmaRegressionTests() : base("vwma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToVwma(14).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToVwmaList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToVwmaHub(14).Results.IsExactly(Expected);
}
