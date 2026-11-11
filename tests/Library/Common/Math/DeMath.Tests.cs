using System.Globalization;

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

    [TestMethod]
    public void ToDecimal_Values_RoundTo15SignificantDigits()
    {
        (double value, string expected)[] cases =
        [
            (210.059, "210.059"),
            (218.9497482878433, "218.949748287843"),
            (-0.1, "-0.1"),
            (1.23, "1.23"),
            (1e20, "100000000000000000000"),
            (123456789012345678.0, "123456789012346000"),
            (1e-28, "0.0000000000000000000000000001"),
            (1e-29, "0"),
            (0.0, "0")
        ];

        foreach ((double value, string expected) in cases)
        {
            DeMath.ToDecimal(value).ToString(CultureInfo.InvariantCulture)
                .Should().Be(expected, $"converting {value:R}");
        }
    }

    [TestMethod]
    public void ToDecimal_NullOrOutOfRange_ReturnsNullOrThrows()
    {
        DeMath.ToDecimal((double?)null).Should().BeNull();

        FluentActions.Invoking(() => DeMath.ToDecimal(double.NaN))
            .Should().Throw<OverflowException>();
        FluentActions.Invoking(() => DeMath.ToDecimal(1e30))
            .Should().Throw<OverflowException>();
    }

#if !NET11_0_OR_GREATER
    [TestMethod]
    public void ToDecimal_BeforeNet11_MatchesBuiltInConversion()
    {
        Random random = new(42);

        for (int i = 0; i < 100_000; i++)
        {
            double value = (random.NextDouble() - 0.5) * Math.Pow(10, random.Next(-30, 29));
            decimal expected = (decimal)value;
            decimal actual = DeMath.ToDecimal(value);

            actual.Should().Be(expected);
            actual.Scale.Should().Be(expected.Scale);
        }
    }
#endif
}
