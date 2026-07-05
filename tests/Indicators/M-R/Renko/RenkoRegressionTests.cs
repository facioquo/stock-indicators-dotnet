namespace Regression;

[TestClass, TestCategory("Regression")]
public class RenkoRegressionTests : RegressionTestBase<RenkoResult>
{
    public RenkoRegressionTests() : base("renko.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRenko(1.0m).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToRenkoList(1.0m).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToRenkoHub(1.0m).Results.IsExactly(Expected);
}
