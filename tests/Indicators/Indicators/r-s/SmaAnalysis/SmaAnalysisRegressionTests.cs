namespace Regression;

[TestClass, TestCategory("Regression")]
public class SmaAnalysisRegressionTests : RegressionTestBase<SmaAnalysisResult>
{
    public SmaAnalysisRegressionTests() : base("sma-analysis.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToSmaAnalysis(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToSmaAnalysisList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToSmaAnalysisHub(20).Results.IsExactly(Expected);
}
