namespace FacioQuo.Stock.Indicators;

public static partial class IndyMath
{
    /// <summary>
    /// PLUS Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing PlusCommonListing =
        new CatalogListingBuilder()
            .WithName("Plus Amount")
            .WithId("PLUS")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("amount", "Amount", isRequired: true, defaultValue: 1.0, minimum: -1000000.0, maximum: 1000000.0)
            .AddResult(nameof(MathResult.Result), "Plus Amount", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// PLUS Series Listing
    /// </summary>
    internal static readonly IndicatorListing PlusSeriesListing =
        new CatalogListingBuilder(PlusCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Plus")
            .Build();

    /// <summary>
    /// PLUS Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing PlusBufferListing =
        new CatalogListingBuilder(PlusCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToPlusList")
            .Build();

    /// <summary>
    /// PLUS Stream Listing
    /// </summary>
    internal static readonly IndicatorListing PlusStreamListing =
        new CatalogListingBuilder(PlusCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToPlusHub")
            .Build();

    /// <summary>
    /// MINUS Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing MinusCommonListing =
        new CatalogListingBuilder()
            .WithName("Minus Amount")
            .WithId("MINUS")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("amount", "Amount", isRequired: true, defaultValue: 1.0, minimum: -1000000.0, maximum: 1000000.0)
            .AddResult(nameof(MathResult.Result), "Minus Amount", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// MINUS Series Listing
    /// </summary>
    internal static readonly IndicatorListing MinusSeriesListing =
        new CatalogListingBuilder(MinusCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Minus")
            .Build();

    /// <summary>
    /// MINUS Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing MinusBufferListing =
        new CatalogListingBuilder(MinusCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToMinusList")
            .Build();

    /// <summary>
    /// MINUS Stream Listing
    /// </summary>
    internal static readonly IndicatorListing MinusStreamListing =
        new CatalogListingBuilder(MinusCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToMinusHub")
            .Build();

    /// <summary>
    /// TIMES Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing TimesCommonListing =
        new CatalogListingBuilder()
            .WithName("Times Factor")
            .WithId("TIMES")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("factor", "Factor", isRequired: true, defaultValue: 2.0, minimum: -1000000.0, maximum: 1000000.0)
            .AddResult(nameof(MathResult.Result), "Times Factor", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// TIMES Series Listing
    /// </summary>
    internal static readonly IndicatorListing TimesSeriesListing =
        new CatalogListingBuilder(TimesCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Times")
            .Build();

    /// <summary>
    /// TIMES Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing TimesBufferListing =
        new CatalogListingBuilder(TimesCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToTimesList")
            .Build();

    /// <summary>
    /// TIMES Stream Listing
    /// </summary>
    internal static readonly IndicatorListing TimesStreamListing =
        new CatalogListingBuilder(TimesCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToTimesHub")
            .Build();

    /// <summary>
    /// DIVIDED-BY Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing DividedByCommonListing =
        new CatalogListingBuilder()
            .WithName("Divided By Divisor")
            .WithId("DIVIDED-BY")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("divisor", "Divisor", isRequired: true, defaultValue: 2.0, minimum: -1000000.0, maximum: 1000000.0)
            .AddResult(nameof(MathResult.Result), "Divided By Divisor", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// DIVIDED-BY Series Listing
    /// </summary>
    internal static readonly IndicatorListing DividedBySeriesListing =
        new CatalogListingBuilder(DividedByCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "DividedBy")
            .Build();

    /// <summary>
    /// DIVIDED-BY Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing DividedByBufferListing =
        new CatalogListingBuilder(DividedByCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToDividedByList")
            .Build();

    /// <summary>
    /// DIVIDED-BY Stream Listing
    /// </summary>
    internal static readonly IndicatorListing DividedByStreamListing =
        new CatalogListingBuilder(DividedByCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToDividedByHub")
            .Build();

    /// <summary>
    /// PLUS-PERCENT Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing PlusPercentCommonListing =
        new CatalogListingBuilder()
            .WithName("Plus Percent")
            .WithId("PLUS-PERCENT")
            .WithCategory(Category.PriceTransform)
            .AddParameter<double>("percentOffset", "Percent Offset", isRequired: true, defaultValue: 2.5, minimum: -100.0, maximum: 100.0)
            .AddResult(nameof(MathResult.Result), "Plus Percent", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// PLUS-PERCENT Series Listing
    /// </summary>
    internal static readonly IndicatorListing PlusPercentSeriesListing =
        new CatalogListingBuilder(PlusPercentCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "PlusPercent")
            .Build();

    /// <summary>
    /// PLUS-PERCENT Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing PlusPercentBufferListing =
        new CatalogListingBuilder(PlusPercentCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToPlusPercentList")
            .Build();

    /// <summary>
    /// PLUS-PERCENT Stream Listing
    /// </summary>
    internal static readonly IndicatorListing PlusPercentStreamListing =
        new CatalogListingBuilder(PlusPercentCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToPlusPercentHub")
            .Build();

    /// <summary>
    /// PLUS-SERIES Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing PlusOtherCommonListing =
        new CatalogListingBuilder()
            .WithName("Plus Series")
            .WithId("PLUS-SERIES")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Plus Series", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// PLUS-SERIES Series Listing
    /// </summary>
    internal static readonly IndicatorListing PlusOtherSeriesListing =
        new CatalogListingBuilder(PlusOtherCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Plus")
            .Build();

    /// <summary>
    /// MINUS-SERIES Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing MinusOtherCommonListing =
        new CatalogListingBuilder()
            .WithName("Minus Series")
            .WithId("MINUS-SERIES")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Minus Series", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// MINUS-SERIES Series Listing
    /// </summary>
    internal static readonly IndicatorListing MinusOtherSeriesListing =
        new CatalogListingBuilder(MinusOtherCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Minus")
            .Build();

    /// <summary>
    /// TIMES-SERIES Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing TimesOtherCommonListing =
        new CatalogListingBuilder()
            .WithName("Times Series")
            .WithId("TIMES-SERIES")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Times Series", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// TIMES-SERIES Series Listing
    /// </summary>
    internal static readonly IndicatorListing TimesOtherSeriesListing =
        new CatalogListingBuilder(TimesOtherCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "Times")
            .Build();

    /// <summary>
    /// DIVIDED-BY-SERIES Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing DividedByOtherCommonListing =
        new CatalogListingBuilder()
            .WithName("Divided By Series")
            .WithId("DIVIDED-BY-SERIES")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Divided By Series", "IndyMath", ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// DIVIDED-BY-SERIES Series Listing
    /// </summary>
    internal static readonly IndicatorListing DividedByOtherSeriesListing =
        new CatalogListingBuilder(DividedByOtherCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "DividedBy")
            .Build();

    /// <summary>
    /// MAX-WITH Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing MaxWithCommonListing =
        new CatalogListingBuilder()
            .WithName("Max With Series")
            .WithId("MAX-WITH")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Max With Series", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// MAX-WITH Series Listing
    /// </summary>
    internal static readonly IndicatorListing MaxWithSeriesListing =
        new CatalogListingBuilder(MaxWithCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "MaxWith")
            .Build();

    /// <summary>
    /// MIN-WITH Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing MinWithCommonListing =
        new CatalogListingBuilder()
            .WithName("Min With Series")
            .WithId("MIN-WITH")
            .WithCategory(Category.PriceTransform)
            .AddSeriesParameter("source", "Source")
            .AddSeriesParameter("other", "Other")
            .AddResult(nameof(MathResult.Result), "Min With Series", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// MIN-WITH Series Listing
    /// </summary>
    internal static readonly IndicatorListing MinWithSeriesListing =
        new CatalogListingBuilder(MinWithCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "MinWith")
            .Build();

    /// <summary>
    /// ROLLING-MAX Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMaxCommonListing =
        new CatalogListingBuilder()
            .WithName("Rolling Max")
            .WithId("ROLLING-MAX")
            .WithCategory(Category.PriceTransform)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult(nameof(MathResult.Result), "Rolling Max", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// ROLLING-MAX Series Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMaxSeriesListing =
        new CatalogListingBuilder(RollingMaxCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "RollingMax")
            .Build();

    /// <summary>
    /// ROLLING-MAX Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMaxBufferListing =
        new CatalogListingBuilder(RollingMaxCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToRollingMaxList")
            .Build();

    /// <summary>
    /// ROLLING-MAX Stream Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMaxStreamListing =
        new CatalogListingBuilder(RollingMaxCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToRollingMaxHub")
            .Build();

    /// <summary>
    /// ROLLING-MIN Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMinCommonListing =
        new CatalogListingBuilder()
            .WithName("Rolling Min")
            .WithId("ROLLING-MIN")
            .WithCategory(Category.PriceTransform)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult(nameof(MathResult.Result), "Rolling Min", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// ROLLING-MIN Series Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMinSeriesListing =
        new CatalogListingBuilder(RollingMinCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "RollingMin")
            .Build();

    /// <summary>
    /// ROLLING-MIN Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMinBufferListing =
        new CatalogListingBuilder(RollingMinCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToRollingMinList")
            .Build();

    /// <summary>
    /// ROLLING-MIN Stream Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMinStreamListing =
        new CatalogListingBuilder(RollingMinCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToRollingMinHub")
            .Build();

    /// <summary>
    /// ROLLING-MEDIAN Common Base Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMedianCommonListing =
        new CatalogListingBuilder()
            .WithName("Rolling Median")
            .WithId("ROLLING-MEDIAN")
            .WithCategory(Category.PriceTransform)
            .AddParameter<int>("lookbackPeriods", "Lookback Periods", isRequired: true, defaultValue: 20, minimum: 1, maximum: 250)
            .AddResult(nameof(MathResult.Result), "Rolling Median", IndicatorResult.PricePane, ResultType.Default, isReusable: true)
            .Build();

    /// <summary>
    /// ROLLING-MEDIAN Series Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMedianSeriesListing =
        new CatalogListingBuilder(RollingMedianCommonListing)
            .WithStyle(Style.Series)
            .WithMethodName(typeof(IndyMath), "RollingMedian")
            .Build();

    /// <summary>
    /// ROLLING-MEDIAN Buffer Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMedianBufferListing =
        new CatalogListingBuilder(RollingMedianCommonListing)
            .WithStyle(Style.Buffer)
            .WithMethodName(typeof(IndyMath), "ToRollingMedianList")
            .Build();

    /// <summary>
    /// ROLLING-MEDIAN Stream Listing
    /// </summary>
    internal static readonly IndicatorListing RollingMedianStreamListing =
        new CatalogListingBuilder(RollingMedianCommonListing)
            .WithStyle(Style.Stream)
            .WithMethodName(typeof(IndyMath), "ToRollingMedianHub")
            .Build();
}
