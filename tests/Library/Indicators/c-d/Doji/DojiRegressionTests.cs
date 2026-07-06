namespace Regression;

[TestClass, TestCategory("Regression")]
public class DojiRegressionTests : RegressionTestBase<CandleResult>
{
    public DojiRegressionTests() : base("doji.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToDoji(0.1).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToDojiList(0.1).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToDojiHub(0.1).Results.IsExactly(Expected);
}
