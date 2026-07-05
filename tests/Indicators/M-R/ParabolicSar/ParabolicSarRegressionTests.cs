namespace Regression;

[TestClass, TestCategory("Regression")]
public class ParabolicSarRegressionTests : RegressionTestBase<ParabolicSarResult>
{
    public ParabolicSarRegressionTests() : base("psar.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToParabolicSar(0.02, 0.2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToParabolicSarList(0.02, 0.2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToParabolicSarHub(0.02, 0.2).Results.IsExactly(Expected);
}
