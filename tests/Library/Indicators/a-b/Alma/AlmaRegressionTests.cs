namespace Regression;

[TestClass, TestCategory("Regression")]
public class AlmaRegressionTests : RegressionTestBase<AlmaResult>
{
    public AlmaRegressionTests() : base("alma.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToAlma(9, 0.85, 6).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToAlmaList(9, 0.85, 6).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToAlmaHub(9, 0.85, 6).Results.IsExactly(Expected);
}
