#nullable enable
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Catalogging;

/// <summary>
/// Catalog shape snapshot test: renders every listing's parameters and results to a
/// deterministic text form and diffs it against a committed snapshot, so a listing
/// that gains or loses a parameter or result fails even though each field still
/// passes the per-field non-empty checks in <see cref="CatalogMetadataTests"/>.
/// </summary>
[TestClass]
public class CatalogShapeTests : TestBase
{
    private const string SnapshotRelativePath = "TestData/catalog/shape.snapshot.txt";
    private const string UpdateEnvironmentVariable = "UPDATE_CATALOG_SHAPE";

    [TestMethod]
    public void CatalogShapeMatchesSnapshot()
    {
        string actual = Render(Catalog.Get());

        // Regeneration writes the source file, not the bin copy, and never reports a pass.
        if (Environment.GetEnvironmentVariable(UpdateEnvironmentVariable) == "1")
        {
            File.WriteAllText(GetSourceSnapshotPath(), actual);
            Assert.Inconclusive($"Catalog shape snapshot regenerated; rerun without {UpdateEnvironmentVariable} to verify it.");
        }

        string snapshotPath = Path.Combine(AppContext.BaseDirectory, SnapshotRelativePath);

        File.Exists(snapshotPath).Should().BeTrue(
            $"the committed catalog shape snapshot should exist at '{snapshotPath}'; " +
            $"generate it by running the test once with {UpdateEnvironmentVariable}=1 set");

        string expected = File.ReadAllText(snapshotPath).Replace("\r\n", "\n", StringComparison.Ordinal);

        if (!string.Equals(expected, actual, StringComparison.Ordinal))
        {
            Assert.Fail(DescribeMismatch(expected, actual));
        }
    }

    /// <summary>
    /// Renders the catalog to a stable plain-text shape: one block per listing,
    /// ordered by UIID then style, naming its parameters and results but omitting
    /// prose (descriptions, display names, URLs) so wording edits do not churn it.
    /// </summary>
    private static string Render(IReadOnlyList<IndicatorListing> listings)
    {
        StringBuilder sb = new();

        foreach (IndicatorListing listing in listings
            .OrderBy(static l => l.Uiid, StringComparer.Ordinal)
            .ThenBy(static l => l.Style))
        {
            sb.Append("### ").Append(listing.Uiid)
                .Append(" / ").Append(listing.Style)
                .Append('\n')
                .Append("Method: ").Append(listing.MethodName ?? "(none)")
                .Append('\n')
                .Append("Parameters:\n");

            if (listing.Parameters is { Count: > 0 } parameters)
            {
                foreach (IndicatorParam param in parameters)
                {
                    sb.Append("  - ").Append(param.ParameterName)
                        .Append(" : ").Append(param.DataType)
                        .Append(" : ").Append(param.IsRequired ? "required" : "optional")
                        .Append('\n');
                }
            }
            else
            {
                sb.Append("  (none)\n");
            }

            sb.Append("Results:\n");
            foreach (IndicatorResult result in listing.Results)
            {
                sb.Append("  - ").Append(result.DataName)
                    .Append(" : ").Append(result.DataType)
                    .Append(" : reusable=").Append(result.IsReusable ? "true" : "false")
                    .Append('\n');
            }

            sb.Append('\n');
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a failure message naming which listing blocks were added, removed, or
    /// changed, plus the regeneration command, instead of a bare "strings differ".
    /// </summary>
    private static string DescribeMismatch(string expected, string actual)
    {
        Dictionary<string, string> expectedBlocks = SplitBlocks(expected);
        Dictionary<string, string> actualBlocks = SplitBlocks(actual);

        string[] added = [.. actualBlocks.Keys.Except(expectedBlocks.Keys, StringComparer.Ordinal).OrderBy(static k => k, StringComparer.Ordinal)];
        string[] removed = [.. expectedBlocks.Keys.Except(actualBlocks.Keys, StringComparer.Ordinal).OrderBy(static k => k, StringComparer.Ordinal)];
        string[] changed = [.. expectedBlocks.Keys
            .Intersect(actualBlocks.Keys, StringComparer.Ordinal)
            .Where(k => !string.Equals(expectedBlocks[k], actualBlocks[k], StringComparison.Ordinal))
            .OrderBy(static k => k, StringComparer.Ordinal)];

        StringBuilder sb = new();
        sb.AppendLine("Catalog shape snapshot mismatch.");

        if (added.Length > 0)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Added listings: {string.Join(", ", added)}");
        }

        if (removed.Length > 0)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Removed listings: {string.Join(", ", removed)}");
        }

        foreach (string key in changed)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Changed listing [{key}]:")
                .AppendLine("--- expected ---")
                .AppendLine(expectedBlocks[key])
                .AppendLine("--- actual ---")
                .AppendLine(actualBlocks[key]);
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"Regenerate the snapshot by running this test once with {UpdateEnvironmentVariable}=1 set.");

        return sb.ToString();
    }

    /// <summary>
    /// Splits a rendering into one entry per listing block, keyed by its
    /// <c>### UIID / Style</c> header line.
    /// </summary>
    private static Dictionary<string, string> SplitBlocks(string rendered)
    {
        Dictionary<string, string> blocks = new(StringComparer.Ordinal);

        foreach (string block in rendered.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            string header = block[..block.IndexOf('\n', StringComparison.Ordinal)];
            blocks[header] = block;
        }

        return blocks;
    }

    /// <summary>
    /// Locates the committed snapshot file next to this source file, for the
    /// regeneration path, so it writes the repo copy rather than the bin-relative one.
    /// </summary>
    private static string GetSourceSnapshotPath([CallerFilePath] string testFilePath = "")
    {
        string testDirectory = Path.GetDirectoryName(testFilePath)!; // .../tests/Library/Common/Catalog
        string libraryTestsRoot = Path.GetFullPath(Path.Combine(testDirectory, "..", ".."));
        return Path.Combine(libraryTestsRoot, SnapshotRelativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
