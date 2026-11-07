using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Utilities;

[TestClass]
public class IndicatorJsonContextTests : TestBase
{
    private static readonly JsonSerializerOptions ReflectionOptions = new() {
        TypeInfoResolver = new DefaultJsonTypeInfoResolver()
    };

    [TestMethod]
    public void EverySeriesResultListHasGeneratedMetadata()
    {
        List<Type> listTypes = Catalog.Get(Style.Series)
            .SelectMany(static listing => listing.GetMethodOverloads())
            .Select(static method => method.ReturnType)
            .Where(static type => type != typeof(IReadOnlyList<IBar>))
            .Distinct()
            .ToList();

        listTypes.Should().NotBeEmpty();

        foreach (Type listType in listTypes)
        {
            IndicatorJsonContext.Default.GetTypeInfo(listType)
                .Should().NotBeNull($"{listType} is returned by a Series method");
        }
    }

    [TestMethod]
    public void SerializesLikeReflection()
    {
        AssertSameJson(Bars.ToEma(20));
        AssertSameJson(Bars.ToMacd());
        AssertSameJson(Bars.ToZigZag());
        AssertSameJson(Bars.ToCandles());
        AssertSameJson(Bars.ToMarubozu());
        AssertSameJson(Bars.ToEma(12).Minus(Bars.ToEma(26)));
        AssertSameJson(Bars.Use(CandlePart.HL2));
#pragma warning disable CS0618 // the obsolete UI alias is part of the wire shape
        AssertSameJson(Bars.ToUlcerIndex());
#pragma warning restore CS0618
    }

    [TestMethod]
    public void OmitsReusableValue()
    {
        string json = JsonSerializer.Serialize(
            Bars.ToEma(20), IndicatorJsonContext.Default.IReadOnlyListEmaResult);

        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement last = doc.RootElement[doc.RootElement.GetArrayLength() - 1];

        last.TryGetProperty(nameof(EmaResult.Ema), out _).Should().BeTrue();
        last.TryGetProperty(nameof(EmaResult.Value), out _).Should().BeFalse();
    }

    [TestMethod]
    public void TakesCallerOptions()
    {
        IndicatorJsonContext context = new(new JsonSerializerOptions(JsonSerializerDefaults.Web));

        string json = JsonSerializer.Serialize(Bars.ToEma(20), context.IReadOnlyListEmaResult);

        json.Should().StartWith("[{\"timestamp\":");
    }

    [TestMethod]
    public void RoundTripsResults()
    {
        IReadOnlyList<MacdResult> expected = Bars.ToMacd();

        string json = JsonSerializer.Serialize(expected, IndicatorJsonContext.Default.IReadOnlyListMacdResult);
        IReadOnlyList<MacdResult> actual
            = JsonSerializer.Deserialize(json, IndicatorJsonContext.Default.IReadOnlyListMacdResult)!;

        actual.Should().Equal(expected);
    }

    private static void AssertSameJson<T>(IReadOnlyList<T> results)
    {
        string generated = JsonSerializer.Serialize(results, typeof(IReadOnlyList<T>), IndicatorJsonContext.Default);
        string reflected = JsonSerializer.Serialize(results, ReflectionOptions);

        generated.Should().Be(reflected, $"{typeof(T).Name} should serialize the same either way");
    }
}
