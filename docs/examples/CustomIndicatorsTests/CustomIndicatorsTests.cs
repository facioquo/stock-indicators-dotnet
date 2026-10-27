using Custom.Stock.Indicators;
using FacioQuo.Stock.Indicators;

[assembly: DoNotParallelize]

namespace CustomIndicatorsTests
{

    [TestClass]
    public class CustomIndicatorTests
    {
        private static readonly IReadOnlyList<Bar> Bars =
        [
            new(new DateTime(2026, 1, 2), 90, 110, 80, 100, 1000),
        new(new DateTime(2026, 1, 30), 100, 120, 70, 90, 1200),
        new(new DateTime(2026, 2, 2), 95, 105, 85, 100, 900)
        ];

        [TestMethod]
        public void ToExtendedPivotPoints_CompletedWindow_ReturnsFiveLevels()
        {
            ExtendedPivotPointsResult result = Bars.ToExtendedPivotPoints()[2];

            Assert.AreEqual(213.33333333333334, result.R5!.Value, 1e-12);
            Assert.AreEqual(190d, result.R4!.Value, 1e-12);
            Assert.AreEqual(166.66666666666666, result.R3!.Value, 1e-12);
            Assert.AreEqual(143.33333333333331, result.R2!.Value, 1e-12);
            Assert.AreEqual(116.66666666666666, result.R1!.Value, 1e-12);
            Assert.AreEqual(93.333333333333329, result.PP!.Value, 1e-12);
            Assert.AreEqual(66.666666666666657, result.S1!.Value, 1e-12);
            Assert.AreEqual(43.333333333333329, result.S2!.Value, 1e-12);
            Assert.AreEqual(16.666666666666657, result.S3!.Value, 1e-12);
            Assert.AreEqual(-10d, result.S4!.Value, 1e-12);
            Assert.AreEqual(-36.666666666666657, result.S5!.Value, 1e-12);
        }

        [TestMethod]
        public void ToExtendedPivotPoints_FirstWindow_ReturnsNullLevels()
        {
            ExtendedPivotPointsResult result = Bars.ToExtendedPivotPoints()[0];

            Assert.IsNull(result.PP);
            Assert.IsNull(result.R5);
            Assert.IsNull(result.S5);
        }

        [TestMethod]
        public void ToAtrWma_ZeroAtr_ReturnsNullInsteadOfInfinity()
        {
            IReadOnlyList<Bar> flatBars =
            [
                new(new DateTime(2026, 1, 1), 10, 10, 10, 10, 0),
            new(new DateTime(2026, 1, 2), 10, 10, 10, 10, 0),
            new(new DateTime(2026, 1, 3), 10, 10, 10, 10, 0)
            ];

            AtrWmaResult result = flatBars.ToAtrWma(2)[2];

            Assert.IsNull(result.AtrWma);
        }
    }

}
