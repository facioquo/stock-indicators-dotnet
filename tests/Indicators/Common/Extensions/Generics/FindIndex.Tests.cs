namespace Utilities;

[TestClass]
public class FindIndexTests : TestBase
{
    [TestMethod]
    public void FindIndex_WithMatchingElement_ReturnsCorrectIndex()
    {
        // arrange
        IReadOnlyList<EmaResult> emaResults = Bars.ToEma(20);

        // act - find first element where Ema is not null
        int index = emaResults.FindIndex(static x => x.Ema != null);

        // assert
        index.Should().BeGreaterThanOrEqualTo(0);
        emaResults[index].Ema.Should().NotBeNull();

        // verify all elements before index have null Ema
        for (int i = 0; i < index; i++)
        {
            emaResults[i].Ema.Should().BeNull();
        }
    }

    [TestMethod]
    public void FindIndex_WithNoMatch_ReturnsNegativeOne()
    {
        // arrange
        IReadOnlyList<EmaResult> emaResults = Bars.ToEma(20);

        // act - find element that doesn't exist (value > 1000000)
        int index = emaResults.FindIndex(static x => x.Ema > 1000000);

        // assert
        index.Should().Be(-1);
    }

    [TestMethod]
    public void FindIndex_WithEmptyList_ReturnsNegativeOne()
    {
        // arrange
        IReadOnlyList<EmaResult> emptyResults = Array.Empty<EmaResult>();

        // act
        int index = emptyResults.FindIndex(static x => x.Ema != null);

        // assert
        index.Should().Be(-1);
    }

    [TestMethod]
    public void FindIndex_WithMatchingFirstElement_ReturnsZero()
    {
        // arrange
        IReadOnlyList<EmaResult> emaResults = Bars.ToEma(20);

        // act - find first element (always matches, since there's always a first element)
        int index = emaResults.FindIndex(static _ => true);

        // assert
        index.Should().Be(0);
    }
}
