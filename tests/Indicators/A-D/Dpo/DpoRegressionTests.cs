namespace Regression;

[TestClass, TestCategory("Regression")]
public class DpoRegressionTests : RegressionTestBase<DpoResult>
{
    public DpoRegressionTests() : base("dpo.standard.json") { }

    private const int lookbackPeriods = 14;

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly()
        => Bars.ToDpo(lookbackPeriods).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly()
        => Bars.ToDpoList(lookbackPeriods).IsExactly(Expected);


    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
        => Bars.ToDpoHub(lookbackPeriods).Results.IsExactly(Expected);
}
