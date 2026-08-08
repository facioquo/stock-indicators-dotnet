namespace Utilities;

[TestClass]
[TestCategory("Utilities")]
public class DeMaths : TestBase
{
    [TestMethod]
    public void Atan_SignedZero_PreservesSign()
    {
        double positive = DeMath.Atan(0.0);
        double negative = DeMath.Atan(-0.0);

        double.IsNegative(positive).Should().BeFalse();
        double.IsNegative(negative).Should().BeTrue();
        positive.Should().Be(0.0);
        negative.Should().Be(-0.0);
    }

    [TestMethod]
    public void Atan2_SignedZeroInputs_MatchesMathAtan2()
    {
        // negative-zero x belongs to the negative half-plane
        DeMath.Atan2(0.0, -0.0).Should().Be(Math.Atan2(0.0, -0.0));   // +pi
        DeMath.Atan2(-0.0, -0.0).Should().Be(Math.Atan2(-0.0, -0.0)); // -pi

        // positive-zero x preserves the sign of y
        double posY = DeMath.Atan2(0.0, 0.0);
        double negY = DeMath.Atan2(-0.0, 0.0);
        double.IsNegative(posY).Should().BeFalse();
        double.IsNegative(negY).Should().BeTrue();

        // y = -0.0 with positive x preserves the sign of y
        double.IsNegative(DeMath.Atan2(-0.0, 1.0)).Should().BeTrue();
    }

    [TestMethod]
    public void Exp_AtOverflowBoundary_ReturnsFiniteValue()
    {
        // ln(double.MaxValue): exp is finite exactly at
        // this value and overflows just above it; the
        // literal is the correctly-rounded exp(boundary),
        // deterministic across platforms unlike Math.Exp
        const double boundary = 709.782712893383973096d;

        DeMath.Exp(boundary).Should().Be(1.7976931348622732E+308);
        double.IsFinite(DeMath.Exp(boundary)).Should().BeTrue();

        double aboveBoundary = Math.BitIncrement(boundary);
        DeMath.Exp(aboveBoundary).Should().Be(double.PositiveInfinity);
    }
}
