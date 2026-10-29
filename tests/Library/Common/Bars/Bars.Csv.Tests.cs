using System.Net;

namespace Utilities;

// bars from CSV

public partial class Bars : TestBase
{
    public TestContext TestContext { get; set; }

    private static readonly string[] csvRows = [
        "date,open,high,low,close,volume",
        "2017-01-04,213.16,214.22,213.15,214.06,83348752",
        "2017-01-03,212.61,213.35,211.52,212.8,96708880"
    ];

    private static readonly Bar[] csvBars = [
        new(new DateTime(2017, 1, 3), 212.61m, 213.35m, 211.52m, 212.8m, 96708880m),
        new(new DateTime(2017, 1, 4), 213.16m, 214.22m, 213.15m, 214.06m, 83348752m)
    ];

    [TestMethod]
    public void FromCsv()
    {
        IReadOnlyList<Bar> bars = Bar.FromCsv(Path.Combine("TestData", "quotes", "default.csv"));

        bars.Should().HaveCount(502);
        bars.Should().Equal(Bars);
    }

    [TestMethod]
    public void FromCsvWithByteOrderMarkAndUsDates()
    {
        IReadOnlyList<Bar> bars = Bar.FromCsv(Path.Combine("TestData", "quotes", "msft.csv"));

        bars.Should().Equal(Data.BarsFromCsv("msft.csv"));
        bars[0].Should().Be(new Bar(new DateTime(1990, 1, 2), 0.378m, 0.385m, 0.373m, 0.385m, 0m));
    }

    [TestMethod]
    public async Task FromCsvAsync()
    {
        Uri uri = new("https://example.com/bars.csv");
        using StubHandler handler = new(string.Join("\r\n", csvRows) + "\r\n");
        using HttpClient client = new(handler, disposeHandler: false);

        IReadOnlyList<Bar> bars = await Bar
            .FromCsvAsync(uri, client, TestContext.CancellationToken)
            .ConfigureAwait(true);

        handler.RequestUri.Should().Be(uri);
        bars.Should().Equal(csvBars);
    }

    [TestMethod]
    public void FromCsvWithoutHeader()
    {
        IReadOnlyList<Bar> bars = Bar.ParseCsv(csvRows.Skip(1));

        bars.Should().Equal(csvBars);
    }

    [TestMethod]
    public void FromCsvSkipsBlankLines()
    {
        IReadOnlyList<Bar> bars = Bar.ParseCsv(["", csvRows[0], "  ", csvRows[1], "", csvRows[2], ""]);

        bars.Should().Equal(csvBars);
    }

    [TestMethod]
    public void FromCsvWithTimestampHeader()
    {
        IReadOnlyList<Bar> bars = Bar.ParseCsv(["Timestamp,Open,High,Low,Close,Volume", csvRows[1]]);

        bars.Should().Equal(csvBars[1]);
    }

    [TestMethod]
    public void FromCsvWithUtcAndOffsetTimestamps()
    {
        IReadOnlyList<Bar> bars = Bar.ParseCsv([
            "2017-01-03T00:00:00Z,212.61,213.35,211.52,212.8,96708880",
            "2017-01-04T09:30:00+05:00,213.16,214.22,213.15,214.06,83348752",
            "2017-01-05,214.75,216.05,213.81,215.62,83348752"
        ]);

        // DateTime equality ignores Kind, so check it separately
        bars[0].Timestamp.Should().Be(new DateTime(2017, 1, 3, 0, 0, 0, DateTimeKind.Utc));
        bars[0].Timestamp.Kind.Should().Be(DateTimeKind.Utc);
        bars[1].Timestamp.Should().Be(new DateTime(2017, 1, 4, 4, 30, 0, DateTimeKind.Utc));
        bars[1].Timestamp.Kind.Should().Be(DateTimeKind.Utc);
        bars[2].Timestamp.Should().Be(new DateTime(2017, 1, 5));
        bars[2].Timestamp.Kind.Should().Be(DateTimeKind.Unspecified);
    }

    [TestMethod]
    public void FromCsvWithHeaderOutOfOrder()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv(["date,open,low,high,close,volume", csvRows[1]]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage("CSV line 1 was read as a header*not in date, open, high, low, close, volume order.");

    [TestMethod]
    public void FromCsvWithWrongHeaderDateColumn()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv(["time,open,high,low,close,volume", csvRows[1]]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage("CSV line 1 was read as a header because 'time' is not a date*");

    [TestMethod]
    public void FromCsvWithMissingHeaderColumn()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv(["date,open,high,low,close", csvRows[1]]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage("CSV line 1 has 5 columns*");

    [TestMethod]
    public void FromCsvWithBadDateOnFirstRow()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv(["2017-13-45,212.61,213.35,211.52,212.8,96708880", csvRows[1]]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage(
                "CSV line 1 was read as a header because '2017-13-45' is not a date, "
              + "but '2017-13-45,212.61,213.35,211.52,212.8,96708880' is not in date, open, high, low, close, volume order.");

    [TestMethod]
    public void FromCsvWithMissingColumn()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv([csvRows[0], csvRows[1], "2017-01-05,214.75,216.05,213.81,215.62"]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage("CSV line 3 has 5 columns*");

    [TestMethod]
    [DataRow("open")]
    [DataRow("high")]
    [DataRow("low")]
    [DataRow("close")]
    [DataRow("volume")]
    public void FromCsvWithBadValue(string name)
    {
        string[] fields = csvRows[1].Split(',');
        fields[Array.IndexOf(csvRows[0].Split(','), name)] = "n/a";

        FluentActions
            .Invoking(() => Bar.ParseCsv([csvRows[0], string.Join(',', fields)]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage($"CSV line 2 has an invalid {name} value 'n/a'.");
    }

    [TestMethod]
    public void FromCsvWithBadDate()
        => FluentActions
            .Invoking(static () => Bar.ParseCsv([csvRows[0], csvRows[1], "Jan 99th,214.75,216.05,213.81,215.62,83348752"]))
            .Should()
            .ThrowExactly<FormatException>()
            .WithMessage("CSV line 3 has an invalid date 'Jan 99th'.");

    [TestMethod]
    public async Task FromCsvWithNullArguments()
    {
        FluentActions
            .Invoking(static () => Bar.FromCsv(null))
            .Should()
            .ThrowExactly<ArgumentNullException>();

        FluentActions
            .Invoking(static () => Bar.FromCsv(string.Empty))
            .Should()
            .ThrowExactly<ArgumentException>();

        await FluentActions
            .Awaiting(() => Bar.FromCsvAsync(null, cancellationToken: TestContext.CancellationToken))
            .Should()
            .ThrowExactlyAsync<ArgumentNullException>()
            .ConfigureAwait(true);
    }

    private sealed class StubHandler(string content) : HttpMessageHandler
    {
        internal Uri RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(content)
            });
        }
    }
}
