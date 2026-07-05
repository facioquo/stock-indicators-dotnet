namespace Utilities;

// invalid bars exceptions

public partial class Bars : TestBase
{
    [TestMethod]
    public void BadHistory()
        => FluentActions
            .Invoking(static () => throw new InvalidBarsException())
            .Should()
            .ThrowExactly<InvalidBarsException>();

    [TestMethod]
    public void BadHistoryWithMessage()
        => FluentActions
            .Invoking(static () => throw new InvalidBarsException("This is a bars exception."))
            .Should()
            .ThrowExactly<InvalidBarsException>();

    [TestMethod]
    public void BadHistoryWithInner()
        => Assert.ThrowsExactly<InvalidBarsException>(
            static () => throw new InvalidBarsException("This has an inner Exception.", new ArgumentException()));
}
