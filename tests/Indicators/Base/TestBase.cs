using System.Globalization;
using TestData = Test.Data.Data;

namespace Test.Base;

/// <summary>
/// base for all tests
/// </summary>
public abstract class TestBase
{
    internal static readonly CultureInfo invariantCulture = CultureInfo.InvariantCulture;

    internal static readonly IReadOnlyList<Bar> Bars = TestData.GetDefault();
    internal static readonly IReadOnlyList<Bar> Intraday = TestData.GetIntraday();
    internal static readonly IReadOnlyList<Bar> OtherBars = TestData.GetCompare();
    internal static readonly IReadOnlyList<Bar> BadBars = TestData.GetBad();
    internal static readonly IReadOnlyList<Bar> BigBars = TestData.GetTooBig();
    internal static readonly IReadOnlyList<Bar> LongishBars = TestData.GetLongish();
    internal static readonly IReadOnlyList<Bar> LongestBars = TestData.GetLongest();
    internal static readonly IReadOnlyList<Bar> MismatchBars = TestData.GetMismatch();
    internal static readonly IReadOnlyList<Bar> Nobars = [];
    internal static readonly IReadOnlyList<Bar> Onebar = TestData.GetDefault(1);
    internal static readonly IReadOnlyList<Bar> RandomBars = TestData.GetRandom(1000);
    internal static readonly IReadOnlyList<Bar> ZeroesBars = TestData.GetZeros();

    internal const int barsCount = 502;

    protected static readonly DateTime EvalDate
        = DateTime.ParseExact("12/31/2018", "MM/dd/yyyy", invariantCulture);
}
