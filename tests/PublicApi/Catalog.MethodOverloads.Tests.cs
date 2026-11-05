using System.Reflection;

namespace Behavioral;

/// <summary>
/// Reads and invokes a listing's indicator method the way a catalog-driven consumer
/// does, without scanning the library assembly.
/// </summary>
[TestClass]
public class CatalogMethodOverloadsTests
{
    [TestMethod]
    public void EveryListingExposesItsMethodOverloads()
    {
        foreach (IndicatorListing listing in Catalog.Get())
        {
            IReadOnlyList<MethodInfo> overloads = listing.GetMethodOverloads();

            overloads.Should().NotBeEmpty($"{listing.Uiid} ({listing.Style}) names {listing.MethodName}");
            overloads.Should().OnlyContain(m => m.Name == listing.MethodName && m.IsStatic && m.IsPublic);
        }
    }

    [TestMethod]
    public void OverloadInvokesLikeTheDirectCall()
    {
        IReadOnlyList<Bar> bars = Data.GetDefault();
        IndicatorListing listing = Catalog.Get("EMA", Style.Series)!;

        MethodInfo method = listing.GetMethodOverloads()
            .Single(static m => m.GetParameters()
                .Select(static p => p.ParameterType)
                .SequenceEqual([typeof(IReadOnlyList<IReusable>), typeof(int)]));

        object result = method.Invoke(null, [bars, 20]);

        result.Should().BeEquivalentTo(bars.ToEma(20));
    }

    [TestMethod]
    public void OperatorSharedAcrossStylesResolvesToTheListingStyle()
    {
        // IndyMath's Minus names both a Series method and a stream hub method
        IReadOnlyList<MethodInfo> series = Catalog.Get("MINUS-SERIES", Style.Series)!.GetMethodOverloads();
        IReadOnlyList<MethodInfo> stream = Catalog.Get("MINUS-SERIES", Style.Stream)!.GetMethodOverloads();

        series.Should().NotBeEmpty()
            .And.OnlyContain(static m => m.ReturnType == typeof(IReadOnlyList<MathResult>));
        stream.Should().NotBeEmpty()
            .And.OnlyContain(static m => typeof(IStreamObservable<MathResult>).IsAssignableFrom(m.ReturnType));
    }

    [TestMethod]
    public void OverloadsCannotBeModifiedThroughTheResult()
    {
        IReadOnlyList<MethodInfo> overloads = Catalog.Get("EMA", Style.Series)!.GetMethodOverloads();

        overloads.Should().NotBeAssignableTo<MethodInfo[]>();
        overloads.Should().BeAssignableTo<ICollection<MethodInfo>>()
            .Which.IsReadOnly.Should().BeTrue();
    }

    [TestMethod]
    public void ListingNamingNoMethodHasNoOverloads()
    {
        IndicatorListing listing = Catalog.Get("EMA", Style.Series)! with { MethodName = "NoSuchMethod" };

        listing.GetMethodOverloads().Should().BeEmpty();
    }

    [TestMethod]
    public void ListingBuiltWithOnlyMethodNameResolvesThroughTheCatalog()
    {
        IndicatorListing catalogListing = Catalog.Get("EMA", Style.Series)!;
        IndicatorListing external = new() {
            Uiid = "MY-EMA",
            Name = "My EMA",
            Category = catalogListing.Category,
            Results = catalogListing.Results,
            LegendTemplate = "My EMA",
            MethodName = "ToEma"
        };

        external.GetMethodOverloads().Should().Equal(catalogListing.GetMethodOverloads());
    }

    [TestMethod]
    public void ListingWithoutMethodHasNoOverloads()
    {
        IndicatorListing listing = Catalog.Get("EMA", Style.Series)! with { MethodName = null };

        listing.GetMethodOverloads().Should().BeEmpty();
    }
}
