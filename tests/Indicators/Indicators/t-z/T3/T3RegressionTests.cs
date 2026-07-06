namespace Regression;

[TestClass, TestCategory("Regression")]
public class T3RegressionTests : RegressionTestBase<T3Result>
{
    public T3RegressionTests() : base("t3.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToT3(5, 0.7).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToT3List(5, 0.7).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToT3Hub(5, 0.7).Results.IsExactly(Expected);
}
