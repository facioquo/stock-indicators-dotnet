namespace Regression;

[TestClass, TestCategory("Regression")]
public class TemaRegressionTests : RegressionTestBase<TemaResult>
{
    public TemaRegressionTests() : base("tema.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToTema(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToTemaList(20).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToTemaHub(20).Results.IsExactly(Expected);
}
