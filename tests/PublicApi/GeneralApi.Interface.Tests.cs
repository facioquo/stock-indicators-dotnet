namespace GeneralApi;

// PUBLIC API (INTERFACES)

[TestClass, TestCategory("Integration")]
public class UserInterface
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();
    private static readonly IReadOnlyList<Bar> barsBad = Data.GetBad();

    [TestMethod]
    public void BarValidation()
    {
        IReadOnlyList<Bar> clean = bars;

        clean.Validate();
        clean.ToSma(6);
        clean.ToEma(5);

        IReadOnlyList<Bar> reverse = bars
            .OrderByDescending(x => x.Timestamp)
            .ToList();

        // has duplicates
        InvalidBarsException dx
            = Assert.ThrowsExactly<InvalidBarsException>(
                () => barsBad.Validate());

        dx.Message.Should().Contain("Duplicate date found");

        // out of order
        InvalidBarsException sx
            = Assert.ThrowsExactly<InvalidBarsException>(
                () => reverse.Validate());

        sx.Message.Should().Contain("Bars are out of sequence");
    }

    [TestMethod]
    public void BarValidationReturn()
    {
        IReadOnlyList<Bar> h = bars.Validate();

        Bar f = h[0];
        Console.WriteLine($"Bar:{f}");
    }
}
