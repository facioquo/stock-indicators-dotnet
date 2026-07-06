namespace Regression;

[TestClass, TestCategory("Regression")]
public class FractalRegressionTests : RegressionTestBase<FractalResult>
{
    public FractalRegressionTests() : base("fractal.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToFractal(2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToFractalList(2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToFractalHub(2).Results.IsExactly(Expected);
}
