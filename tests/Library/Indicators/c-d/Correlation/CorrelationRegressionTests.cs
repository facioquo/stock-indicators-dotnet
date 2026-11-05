namespace Regression;

[TestClass, TestCategory("Regression")]
public class CorrelationRegressionTests : RegressionTestBase<CorrResult>
{
    public CorrelationRegressionTests() : base("corr.standard.json") { }

    private const int n = 20;

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.ToCorrelation(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToCorrelationList(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub sourceA = new();
        BarHub sourceB = new();
        CorrelationHub observer = sourceA.ToCorrelationHub(sourceB, n);

        sourceA.Add(OtherBars);
        sourceB.Add(Bars);

        observer.Results.IsExactly(Expected);
    }
}
