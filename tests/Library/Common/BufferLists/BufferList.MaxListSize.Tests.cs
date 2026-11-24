using System.Collections;
using System.Reflection;

namespace BufferLists;

/// <summary>
/// Runs every buffer list in the catalog with a <c>MaxListSize</c> smaller than its warmup
/// and requires the retained results to equal the newest Series results exactly.
/// </summary>
/// <remarks>
/// Per-indicator pruning tests use a <c>MaxListSize</c> of 100 or more, which hides lists
/// that derive warmup state from their own pruned results (#2443). Lists run at catalog
/// defaults; a series parameter is filled with the default bars. Results are compared by
/// public property values, since a record's equality also sees its non-public state.
/// </remarks>
[TestClass]
public class BufferListMaxListSize : TestBase
{
    private static readonly int[] Sizes = [1, 2, 3, 5, 10, 30];

    [TestMethod]
    public void MaxListSize_BelowWarmup_MatchesSeriesTail()
    {
        List<string> failures = [];

        foreach (IndicatorListing buffer in Catalog.Get(Style.Buffer))
        {
            IndicatorListing series = Catalog.Get(buffer.Uiid, Style.Series);
            List<object> expected = [.. ListingExecutor.Execute<object>(Bars, series, SeriesArgs(series))];

            foreach (int size in Sizes)
            {
                List<object> actual = RunPruned(buffer, size);
                List<object> tail = expected.Skip(expected.Count - actual.Count).ToList();

                if (actual.Count != Math.Min(size, expected.Count))
                {
                    failures.Add($"{buffer.Uiid} MaxListSize={size}: kept {actual.Count} results");
                    continue;
                }

                int first = Enumerable.Range(0, actual.Count)
                    .FirstOrDefault(i => !PublicValues(tail[i]).SequenceEqual(PublicValues(actual[i])), -1);

                if (first >= 0)
                {
                    failures.Add($"{buffer.Uiid} MaxListSize={size}: first differs at [{first}] {Difference(tail[first], actual[first])}");
                }
            }
        }

        if (failures.Count > 0)
        {
            Assert.Fail($"Pruned buffer lists disagree with Series:\n{string.Join("\n", failures)}");
        }
    }

    /// <summary>
    /// Builds the list from the catalog, clears it, sets <paramref name="maxListSize"/>,
    /// then adds the default bars again so pruning happens during the adds.
    /// </summary>
    private static List<object> RunPruned(IndicatorListing listing, int maxListSize)
    {
        object list = ListingExecutor.Execute<object>(Bars, listing, SeriesArgs(listing));
        Type type = list.GetType();

        type.GetMethod("Clear", Type.EmptyTypes).Invoke(list, null);
        type.GetProperty("MaxListSize").SetValue(list, maxListSize);

        MethodInfo addBars = FindAdd(type, typeof(IReadOnlyList<IBar>));
        MethodInfo addChain = FindAdd(type, typeof(IReadOnlyList<IReusable>));
        MethodInfo addPair = type.GetMethod("Add", [typeof(IReusable), typeof(IReusable)]);

        if (addPair is not null)
        {
            foreach (Bar bar in Bars)
            {
                addPair.Invoke(list, [bar, bar]);
            }
        }
        else if (addBars is not null)
        {
            addBars.Invoke(list, [Bars]);
        }
        else
        {
            addChain.Invoke(list, [Bars.Cast<IReusable>().ToList()]);
        }

        return [.. ((IEnumerable)list).Cast<object>()];
    }

    private static MethodInfo FindAdd(Type type, Type parameterType)
        => type.GetMethod("Add", [parameterType]);

    private static Dictionary<string, object> SeriesArgs(IndicatorListing listing)
    {
        Dictionary<string, object> args = [with(StringComparer.Ordinal)];

        foreach (IndicatorParam param in (listing.Parameters ?? [])
            .Where(static p => p.DataType == IndicatorParam.SeriesDataType))
        {
            args[param.ParameterName] = Bars.Cast<IReusable>().ToList();
        }

        return args;
    }

    private static IEnumerable<object> PublicValues(object result)
        => PublicProperties(result).Select(p => p.GetValue(result));

    private static IOrderedEnumerable<PropertyInfo> PublicProperties(object result)
        => result.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(static p => p.Name, StringComparer.Ordinal);

    private static string Difference(object expected, object actual)
        => string.Join("; ", PublicProperties(expected)
            .Where(p => !Equals(p.GetValue(expected), p.GetValue(actual)))
            .Select(p => $"{p.Name}: Series {p.GetValue(expected)}, got {p.GetValue(actual)}"));
}
