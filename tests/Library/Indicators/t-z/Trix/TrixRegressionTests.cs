namespace Regression;

[TestClass, TestCategory("Regression")]
public class TrixRegressionTests : RegressionTestBase<TrixResult>
{
    public TrixRegressionTests() : base("trix.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToTrix().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTrixList(14).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTrixHub(14).Results.IsExactly(Expected);
}
