namespace Regression;

[TestClass, TestCategory("Regression")]
public class PrsRegressionTests : RegressionTestBase<PrsResult>
{
    public PrsRegressionTests() : base("prs.standard.json") { }

    private const int n = 20;

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => OtherBars.ToPrs(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => OtherBars.ToPrsList(Bars, n).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub eval = new();
        BarHub baseHub = new();
        PrsHub observer = eval.ToPrsHub(baseHub, n);

        eval.Add(OtherBars);
        baseHub.Add(Bars);

        observer.Results.IsExactly(Expected);
    }
}
