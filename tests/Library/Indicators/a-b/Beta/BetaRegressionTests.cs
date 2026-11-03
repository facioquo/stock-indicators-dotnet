namespace Regression;

[TestClass, TestCategory("Regression")]
public class BetaRegressionTests : RegressionTestBase<BetaResult>
{
    public BetaRegressionTests() : base("beta.standard.json") { }

    private const int n = 50;

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.ToBeta(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToBetaList(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub eval = new();
        BarHub mrkt = new();
        BetaHub observer = eval.ToBetaHub(mrkt, n);

        eval.Add(OtherBars);
        mrkt.Add(Bars);

        observer.Results.IsExactly(Expected);
    }
}
