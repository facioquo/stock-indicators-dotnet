using System.Text.Json;

namespace Catalogging;

/// <summary>
/// Catalog execution tests validating dynamic invocation by ID and JSON config:
/// - ExecuteById for RSI/EMA/SMA (with/without parameters) matching direct calls
/// - JSON-based execution (valid, minimal, invalid inputs)
/// - Error handling for invalid id/style, null bars, bad parameter types
/// - Defaults usage when required parameters are omitted
/// </summary>
[TestClass]
public class CatalogExecutionTests : TestBase
{
    [TestMethod]
    public void ExecuteByIdRsiDefault()
    {
        // Arrange
        const string id = "RSI";
        const Style style = Style.Series;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>(id, style);

        // Assert
        sut.Should().NotBeNullOrEmpty();

        IReadOnlyList<RsiResult> directResults = bars.ToRsi();
        sut.Should().HaveCount(directResults.Count);

        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteByIdRsiWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 10 } };

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>("RSI", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> directResults = bars.ToRsi(10);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteByIdEmaWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 20 } };

        // Act
        IReadOnlyList<EmaResult> sut = bars.ExecuteById<EmaResult>("EMA", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<EmaResult> directResults = bars.ToEma(20);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Ema.Should().Be(directResults[i].Ema);
        }
    }

    [TestMethod]
    public void ExecuteByIdSmaWithParameters()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = new() { { "lookbackPeriods", 10 } };

        // Act
        IReadOnlyList<SmaResult> sut = bars.ExecuteById<SmaResult>("SMA", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<SmaResult> directResults = bars.ToSma(10);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Sma.Should().Be(directResults[i].Sma);
        }
    }

    [TestMethod]
    public void ExecuteByIdInvalidInputs()
    {
        // invalid id
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action a1 = () => bars.ExecuteById<RsiResult>("INVALID_INDICATOR", Style.Series);
        a1.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");

        // null bars
        IEnumerable<IBar> nullBars = null!;
        Action a2 = () => nullBars.ExecuteById<RsiResult>("RSI", Style.Series);
        a2.Should().Throw<ArgumentNullException>().WithMessage("*bars*");

        // empty id
        Action a3 = () => bars.ExecuteById<RsiResult>(string.Empty, Style.Series);
        a3.Should().Throw<ArgumentException>().WithMessage("*ID cannot be null or empty*");

        // invalid style
        Action a4 = () => bars.ExecuteById<object>("RSI", (Style)999);
        a4.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");

        // mismatched parameter type
        Dictionary<string, object> badParams = new() { { "lookbackPeriods", "invalid_string" } };
        Action a5 = () => bars.ExecuteById<RsiResult>("RSI", Style.Series, badParams);
        a5.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ExecuteByIdMissingRequiredParametersUsesDefaults()
    {
        // Arrange
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Dictionary<string, object> parameters = [];

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteById<RsiResult>("RSI", Style.Series, parameters);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> defaultResults = bars.ToRsi();
        sut.Should().HaveCount(defaultResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(defaultResults[i].Timestamp);
            sut[i].Rsi.Should().Be(defaultResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteFromJsonRsiValid()
    {
        // Arrange
        IndicatorConfig config = new() {
            Id = "RSI",
            Style = Style.Series,
            Parameters = new Dictionary<string, object> { { "lookbackPeriods", 14 } }
        };
        string json = JsonSerializer.Serialize(config);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        // Act
        IReadOnlyList<RsiResult> sut = bars.ExecuteFromJson<RsiResult>(json);

        // Assert
        sut.Should().NotBeNullOrEmpty();
        IReadOnlyList<RsiResult> directResults = bars.ToRsi(14);
        sut.Should().HaveCount(directResults.Count);
        for (int i = 0; i < sut.Count; i++)
        {
            sut[i].Timestamp.Should().Be(directResults[i].Timestamp);
            sut[i].Rsi.Should().Be(directResults[i].Rsi);
        }
    }

    [TestMethod]
    public void ExecuteFromJsonMinimalConfigWorks()
    {
        IndicatorConfig config = new() { Id = "EMA", Style = Style.Series };
        string json = JsonSerializer.Serialize(config);
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        IReadOnlyList<EmaResult> sut = bars.ExecuteFromJson<EmaResult>(json);
        sut.Should().NotBeNullOrEmpty();
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidInputs()
    {
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();

        Action a1 = () => bars.ExecuteFromJson<object>("{ invalid json }");
        a1.Should().Throw<ArgumentException>().WithMessage("*Invalid JSON configuration*");

        const string nullJson = null!;
        Action a2 = () => bars.ExecuteFromJson<object>(nullJson);
        a2.Should().Throw<ArgumentNullException>().WithMessage("*json*");

        Action a3 = () => bars.ExecuteFromJson<object>(string.Empty);
        a3.Should().Throw<ArgumentException>().WithMessage("*JSON configuration cannot be null or empty*");

        IEnumerable<IBar> nullBars = null!;
        IndicatorConfig cfg = new() { Id = "RSI", Style = Style.Series };
        string json = JsonSerializer.Serialize(cfg);
        Action a4 = () => nullBars.ExecuteFromJson<object>(json);
        a4.Should().Throw<ArgumentNullException>().WithMessage("*bars*");
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidParameterTypesThrows()
    {
        const string json = /*lang=json,strict*/ """
        {
          "Id": "RSI",
          "Style": 0,
          "Parameters": {
            "lookbackPeriods": "not_a_number"
          }
        }
        """;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action act = () => bars.ExecuteFromJson<RsiResult>(json);
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void ExecuteFromJsonInvalidIdThrows()
    {
        const string json = /*lang=json,strict*/ """
        {
          "Id": "NONEXISTENT",
          "Style": 0
        }
        """;
        IReadOnlyList<Bar> bars = Bars.Take(50).ToList();
        Action act = () => bars.ExecuteFromJson<RsiResult>(json);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not found in catalog*");
    }
}
