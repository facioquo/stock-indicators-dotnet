using System.Text.Json;

namespace Catalogging;

/// <summary>
/// Catalog export tests for producing JSON and Markdown outputs:
/// - JSON format validation and file save
/// - Markdown checklist/table variants and file save
/// - behavior with empty/whitespace file paths (no write)
/// </summary>
[TestClass]
public class CatalogExportTests : TestBase
{
    [TestMethod]
    public void CatalogToJson()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string catalogJson = indicatorCatalog.ToJson();
        Console.WriteLine(catalogJson);

        catalogJson.Should().NotBeNullOrEmpty();
        catalogJson.Should().StartWith("[");
        catalogJson.Should().EndWith("]");
        catalogJson.Should().Contain("\"uiid\":");
        catalogJson.Should().Contain("\"name\":");
        catalogJson.Should().Contain("\"style\":");
        catalogJson.Should().Contain("\"category\":");
        catalogJson.Should().Contain("\"results\":");
        catalogJson.Should().Contain("\"legendTemplate\":");

        using JsonDocument doc = JsonDocument.Parse(catalogJson);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
    }

    [TestMethod]
    public void CatalogToMarkdownChecklist()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string catalogMarkdown = indicatorCatalog.ToMarkdownChecklist();
        Console.WriteLine(catalogMarkdown);

        catalogMarkdown.Should().NotBeNullOrEmpty();
        catalogMarkdown.Should().Contain("- [ ]");

        int lineCount = catalogMarkdown.Split('\n').Length;
        lineCount.Should().BeGreaterThan(50);
        catalogMarkdown.Should().NotContain("|");
    }

    [TestMethod]
    public void CatalogToMarkdownTable()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string catalogMarkdown = indicatorCatalog.ToMarkdownTable();
        Console.WriteLine(catalogMarkdown);

        catalogMarkdown.Should().NotBeNullOrEmpty();
        catalogMarkdown.Should().Contain("| ID | Name | Series | Buffer | Stream |");
        catalogMarkdown.Should().Contain("|---|---|:---:|:---:|:---:|");
        catalogMarkdown.Should().Contain("✓");
        catalogMarkdown.Should().Contain("RSI");
        catalogMarkdown.Should().Contain("EMA");
        catalogMarkdown.Should().Contain("SMA");
        catalogMarkdown.Should().NotContain("- [ ]");
    }

    [TestMethod]
    public void CatalogToJsonWithFilePathSavesToFile()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string tempFilePath = Path.GetTempFileName();
        try
        {
            string catalogJson = indicatorCatalog.ToJson(tempFilePath);
            catalogJson.Should().NotBeNullOrEmpty();
            File.Exists(tempFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(tempFilePath);
            fileContent.Should().Be(catalogJson);
            fileContent.Should().StartWith("[");
            fileContent.Should().EndWith("]");
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [TestMethod]
    public void CatalogToMarkdownWithFilePathSavesToFile()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string tempFilePath = Path.GetTempFileName();
        try
        {
            string catalogMarkdown = indicatorCatalog.ToMarkdownChecklist(tempFilePath);
            catalogMarkdown.Should().NotBeNullOrEmpty();
            File.Exists(tempFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(tempFilePath);
            fileContent.Should().Be(catalogMarkdown);
            fileContent.Should().Contain("- [ ]");
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [TestMethod]
    public void CatalogToMarkdownTableWithFilePathSavesToFile()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string tempFilePath = Path.GetTempFileName();
        try
        {
            string catalogMarkdown = indicatorCatalog.ToMarkdownTable(tempFilePath);
            catalogMarkdown.Should().NotBeNullOrEmpty();
            File.Exists(tempFilePath).Should().BeTrue();
            string fileContent = File.ReadAllText(tempFilePath);
            fileContent.Should().Be(catalogMarkdown);
            fileContent.Should().Contain("| ID | Name | Series | Buffer | Stream |");
            fileContent.Should().Contain("✓");
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [TestMethod]
    public void CatalogToJsonWithEmptyFilePathDoesNotSaveToFile()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string catalogJson = indicatorCatalog.ToJson(" ");
        catalogJson.Should().NotBeNullOrEmpty();
        catalogJson.Should().StartWith("[");
        catalogJson.Should().EndWith("]");
    }

    [TestMethod]
    public void CatalogToMarkdownChecklistWithEmptyFilePathDoesNotSaveToFile()
    {
        IReadOnlyList<IndicatorListing> indicatorCatalog = Catalog.Get();
        string catalogMarkdown = indicatorCatalog.ToMarkdownChecklist(" ");
        catalogMarkdown.Should().NotBeNullOrEmpty();
        catalogMarkdown.Should().Contain("- [ ]");
    }
}
