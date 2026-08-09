using System.Globalization;

namespace Utilities;

[TestClass]
public class StringOutputs : TestBase
{
    [TestMethod]
    public void ToConsoleBarType()
    {
        DateTime timestamp = DateTime.TryParse(
            "2017-02-03", CultureInfo.InvariantCulture, out DateTime d) ? d : default;

        Bar bar = new(timestamp, 216.1579m, 216.875m, 215.84m, 216.67m, 98765432832);

        string sut = bar.ToConsole();
        string val = bar.ToStringOut();

        sut.Should().Be(val);
    }

    [TestMethod]
    public void ToConsoleBarList()
    {
        string sut = Bars.ToConsole();
        string val = Bars.ToStringOut();
        int length = sut.Split(Environment.NewLine).Length;

        sut.Should().Be(val);
        length.Should().Be(505); // 2 headers + 502 data rows + 1 eof line
    }

    [TestMethod]
    public void ToStringOutBarType()
    {
        DateTime timestamp = DateTime.TryParse(
            "2017-02-03", CultureInfo.InvariantCulture, out DateTime d) ? d : default;

        Bar bar = new(timestamp, 216.1m, 216.875m, 215.84m, 216.67m, 85273832);

        string sut = bar.ToStringOut();
        Console.WriteLine(sut);

        // note description has max of 30 "-" characters
        string expected = """
            Property   Type                    Value  Description
            ------------------------------------------------------------------------
            Timestamp  DateTime  2017-02-03T00:00:00  Close date/time of the aggregate
            Open       Decimal                 216.1  Aggregate bar's first tick price
            High       Decimal               216.875  Aggregate bar's highest tick price
            Low        Decimal                215.84  Aggregate bar's lowest tick price
            Close      Decimal                216.67  Aggregate bar's last tick price
            Volume     Decimal              85273832  Aggregate bar's tick volume
            """.WithDefaultLineEndings();

        sut.Should().Be(expected);
    }

    [TestMethod]
    public void ToStringOutMostTypes()
    {
        AllTypes allTypes = new();
        string sut = allTypes.ToStringOut();
        Console.WriteLine(sut);

        string expected = """
            Property                Type                                          Value  Description
            -----------------------------------------------------------------------------------------------------------
            Timestamp               DateTime                       2023-01-01 14:30:00Z  A 'DateTime' type with time (UTC)
            DateTimeProperty        DateTime                        2023-01-01T09:30:00  A 'DateTime' type with time
            DateProperty            DateOnly                                 2023-01-01  A 'DateOnly' type without time.
            DateTimeOffsetProperty  DateTimeOffset    2023-01-01T09:30:00.0000000-05:00  A 'DateTimeOffset' type with time and offset.
            TimeSpanProperty        TimeSpan                                   01:02:03  A 'TimeSpan' type
            ByteProperty            Byte                                            255  A 'Byte' type
            ShortProperty           Int16                                         32767  A 'Int16' short integer type
            IntProperty             Int32                                   -2147483648  A 'Int32' integer type
            LongProperty            Int64                           9223372036854775803  'get' the 'Int64' long integer type
            FloatProperty           Single                                   -125.25143  A get of 'Single' floating point type
            DoubleProperty          Double                            5.251426433759354  A 'Double' floating point type
            DecimalProperty         Decimal              7922815.2514264337593543950335  A 'Decimal' type
            CharProperty            Char                                              A  A 'Char' type
            BoolProperty            Boolean                                        True  A 'Boolean' type
            NoXmlProperty           Boolean                                       False  
            StringProperty          String          The lazy dog jumped over the sly...  A 'String' type
            """.WithDefaultLineEndings();

        sut.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthBarStandard()
    {
        /* based on what we know about the test data precision */

        string output = Bars.ToStringOut(limitQty: 12);
        Console.WriteLine(output);

        string expected = """
            i   Timestamp     Open    High     Low   Close    Volume
            --  ----------  ------  ------  ------  ------  --------
            0   2017-01-03  212.61  213.35  211.52  212.80  96708880
            1   2017-01-04  213.16  214.22  213.15  214.06  83348752
            2   2017-01-05  213.77  214.06  213.02  213.89  82961968
            3   2017-01-06  214.02  215.17  213.42  214.66  75744152
            4   2017-01-09  214.38  214.53  213.91  213.95  49684316
            5   2017-01-10  213.97  214.89  213.52  213.95  67500792
            6   2017-01-11  213.86  214.55  213.13  214.55  79014928
            7   2017-01-12  213.99  214.22  212.53  214.02  76329760
            8   2017-01-13  214.21  214.84  214.17  214.51  66385084
            9   2017-01-17  213.81  214.25  213.33  213.75  64821664
            10  2017-01-18  214.02  214.27  213.42  214.22  57997156
            11  2017-01-19  214.31  214.46  212.96  213.43  70503512

            """.WithDefaultLineEndings();

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthBarWithArgs()
    {
        Dictionary<string, string> args = new()
        {
            { "decimal", "F4" },
            { "Close", "F3" },
            { "Volume", "F0" }
        };

        string output = Bars.Take(12).ToStringOut(args);
        Console.WriteLine(output);

        string expected = """
            i   Timestamp       Open      High       Low    Close    Volume
            --  ----------  --------  --------  --------  -------  --------
            0   2017-01-03  212.6100  213.3500  211.5200  212.800  96708880
            1   2017-01-04  213.1600  214.2200  213.1500  214.060  83348752
            2   2017-01-05  213.7700  214.0600  213.0200  213.890  82961968
            3   2017-01-06  214.0200  215.1700  213.4200  214.660  75744152
            4   2017-01-09  214.3800  214.5300  213.9100  213.950  49684316
            5   2017-01-10  213.9700  214.8900  213.5200  213.950  67500792
            6   2017-01-11  213.8600  214.5500  213.1300  214.550  79014928
            7   2017-01-12  213.9900  214.2200  212.5300  214.020  76329760
            8   2017-01-13  214.2100  214.8400  214.1700  214.510  66385084
            9   2017-01-17  213.8100  214.2500  213.3300  213.750  64821664
            10  2017-01-18  214.0200  214.2700  213.4200  214.220  57997156
            11  2017-01-19  214.3100  214.4600  212.9600  213.430  70503512

            """.WithDefaultLineEndings();

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthBarIntraday()
    {
        string output = Intraday.ToStringOut(limitQty: 12);
        Console.WriteLine(output);

        string expected = """
            i   Timestamp            Open     High      Low   Close  Volume
            --  ----------------  -------  -------  -------  ------  ------
            0   2020-12-15 09:30  367.400  367.620  367.360  367.46  407870
            1   2020-12-15 09:31  367.480  367.480  367.190  367.19  173406
            2   2020-12-15 09:32  367.190  367.400  367.020  367.35  149240
            3   2020-12-15 09:33  367.345  367.640  367.345  367.59  197941
            4   2020-12-15 09:34  367.590  367.610  367.320  367.43  147919
            5   2020-12-15 09:35  367.430  367.650  367.260  367.34  170552
            6   2020-12-15 09:36  367.350  367.560  367.150  367.53  200528
            7   2020-12-15 09:37  367.535  367.720  367.340  367.47  117417
            8   2020-12-15 09:38  367.480  367.480  367.190  367.42  127936
            9   2020-12-15 09:39  367.440  367.600  367.300  367.57  150339
            10  2020-12-15 09:40  367.580  367.775  367.560  367.61  136414
            11  2020-12-15 09:41  367.610  367.640  367.450  367.60   98185

            """.WithDefaultLineEndings();

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthBarMinutes()
    {
        List<Bar> bars = [];
        for (int i = 0; i < 20; i++)
        {
            DateTime timestamp = new DateTime(2023, 1, 1, 9, 30, 0).AddMinutes(i);
            bars.Add(new Bar(timestamp, 100 + i, 105 + i, 95 + i, 102 + i, 1000 + i));
        }

        string expected = """
            i   Timestamp         Open  High  Low  Close  Volume
            --  ----------------  ----  ----  ---  -----  ------
            0   2023-01-01 09:30   100   105   95    102    1000
            1   2023-01-01 09:31   101   106   96    103    1001
            2   2023-01-01 09:32   102   107   97    104    1002
            3   2023-01-01 09:33   103   108   98    105    1003
            4   2023-01-01 09:34   104   109   99    106    1004
            5   2023-01-01 09:35   105   110  100    107    1005
            6   2023-01-01 09:36   106   111  101    108    1006
            7   2023-01-01 09:37   107   112  102    109    1007
            8   2023-01-01 09:38   108   113  103    110    1008
            9   2023-01-01 09:39   109   114  104    111    1009
            10  2023-01-01 09:40   110   115  105    112    1010
            11  2023-01-01 09:41   111   116  106    113    1011
            12  2023-01-01 09:42   112   117  107    114    1012
            13  2023-01-01 09:43   113   118  108    115    1013
            14  2023-01-01 09:44   114   119  109    116    1014
            15  2023-01-01 09:45   115   120  110    117    1015
            16  2023-01-01 09:46   116   121  111    118    1016
            17  2023-01-01 09:47   117   122  112    119    1017
            18  2023-01-01 09:48   118   123  113    120    1018
            19  2023-01-01 09:49   119   124  114    121    1019

            """.WithDefaultLineEndings();

        string output = bars.ToStringOut();
        Console.WriteLine(output);

        string[] lines = output.Split(Environment.NewLine);
        lines.Length.Should().Be(23); // 2 headers + 20 data rows + 1 eof line

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthBarSeconds()
    {
        List<Bar> bars = [];
        for (int i = 0; i < 20; i++)
        {
            DateTime timestamp = new DateTime(2023, 1, 1, 9, 30, 0).AddSeconds(i);
            bars.Add(new Bar(timestamp, 100 + i, 105 + i, 95 + i, 102 + i, 1000 + i));
        }

        string expected = """
            i   Timestamp            Open  High  Low  Close  Volume
            --  -------------------  ----  ----  ---  -----  ------
            0   2023-01-01 09:30:00   100   105   95    102    1000
            1   2023-01-01 09:30:01   101   106   96    103    1001
            2   2023-01-01 09:30:02   102   107   97    104    1002
            3   2023-01-01 09:30:03   103   108   98    105    1003
            4   2023-01-01 09:30:04   104   109   99    106    1004
            5   2023-01-01 09:30:05   105   110  100    107    1005
            6   2023-01-01 09:30:06   106   111  101    108    1006
            7   2023-01-01 09:30:07   107   112  102    109    1007
            8   2023-01-01 09:30:08   108   113  103    110    1008
            9   2023-01-01 09:30:09   109   114  104    111    1009
            10  2023-01-01 09:30:10   110   115  105    112    1010
            11  2023-01-01 09:30:11   111   116  106    113    1011
            12  2023-01-01 09:30:12   112   117  107    114    1012
            13  2023-01-01 09:30:13   113   118  108    115    1013
            14  2023-01-01 09:30:14   114   119  109    116    1014
            15  2023-01-01 09:30:15   115   120  110    117    1015
            16  2023-01-01 09:30:16   116   121  111    118    1016
            17  2023-01-01 09:30:17   117   122  112    119    1017
            18  2023-01-01 09:30:18   118   123  113    120    1018
            19  2023-01-01 09:30:19   119   124  114    121    1019

            """.WithDefaultLineEndings();

        string output = bars.ToStringOut();
        Console.WriteLine(output);

        string[] lines = output.Split(Environment.NewLine);
        lines.Length.Should().Be(23); // 2 headers + 20 data rows + 1 eof line

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthResultEma()
    {
        IReadOnlyList<EmaResult> ema = Bars.ToEma(14);
        string output = ema.ToStringOut(startIndex: ema.Count - 21, endIndex: ema.Count - 1);
        Console.WriteLine(output);

        // TODO: fix after adding index range

        string expected = """
            i    Timestamp          Ema
            ---  ----------  ----------
            481  2018-11-29  264.114847
            482  2018-11-30  264.760868
            483  2018-12-03  265.795419
            484  2018-12-04  265.514696
            485  2018-12-06  265.218070
            486  2018-12-07  264.144994
            487  2018-12-10  263.280328
            488  2018-12-11  262.538951
            489  2018-12-12  262.068424
            490  2018-12-13  261.649968
            491  2018-12-14  260.649972
            492  2018-12-17  259.117976
            493  2018-12-18  257.754246
            494  2018-12-19  256.075013
            495  2018-12-20  254.087678
            496  2018-12-21  251.706654
            497  2018-12-24  248.811100
            498  2018-12-26  247.850954
            499  2018-12-27  247.265493
            500  2018-12-28  246.716761
            501  2018-12-31  246.525193

            """.WithDefaultLineEndings();

        output.Should().Be(expected);
    }

    [TestMethod]
    public void ToFixedWidthResultHtTrendline()
    {
        string output = Bars.ToHtTrendline().ToStringOut(startIndex: 90, endIndex: 110);
        Console.WriteLine(output);

        // TODO: fix after adding index range

        string expected = """
            i    Timestamp   DcPeriods   Trendline  SmoothPrice
            ---  ----------  ---------  ----------  -----------
            90   2017-05-12         18  225.587904   226.912000
            91   2017-05-15         19  225.755992   227.174500
            92   2017-05-16         19  225.969113   227.481500
            93   2017-05-17         19  226.155297   226.608000
            94   2017-05-18         20  226.224826   225.659000
            95   2017-05-19         21  226.246929   225.548000
            96   2017-05-22         22  226.251725   226.017000
            97   2017-05-23         22  226.340184   226.802000
            98   2017-05-24         22  226.487975   227.505000
            99   2017-05-25         22  226.646455   228.305000
            100  2017-05-26         23  226.790405   228.846000
            101  2017-05-30         24  226.905861   229.084500
            102  2017-05-31         25  226.999587   229.089000
            103  2017-06-01         26  227.098513   229.479000
            104  2017-06-02         26  227.227763   230.233000
            105  2017-06-05         25  227.413835   230.913000
            106  2017-06-06         24  227.634324   231.168500
            107  2017-06-07         23  227.889454   231.138500
            108  2017-06-08         22  228.143057   231.170500
            109  2017-06-09         21  228.386085   231.095000
            110  2017-06-12         21  228.603337   230.852000

            """.WithDefaultLineEndings();

        output.Should().Be(expected);
    }
}

/// <summary>
/// A test class implementing <see cref="ISeries"/> containing properties of various data types.
/// </summary>
public class AllTypes : ISeries
{
    /// <summary>
    /// A <see cref="DateTime"/> type with time (UTC)
    /// </summary>
    public DateTime Timestamp { get; } = new DateTime(2023, 1, 1, 14, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// A <see cref="DateTime"/> type with time
    /// </summary>
    public DateTime DateTimeProperty { get; } = new DateTime(2023, 1, 1, 9, 30, 0);

    /// <summary>
    /// A <see cref="DateOnly"/> type without time.
    /// </summary>
    public DateOnly DateProperty { get; } = new DateOnly(2023, 1, 1);

    /// <summary>
    /// A <see cref="DateTimeOffset"/> type with time and offset.
    /// </summary>
    public DateTimeOffset DateTimeOffsetProperty { get; } = new DateTimeOffset(2023, 1, 1, 9, 30, 0, TimeSpan.FromHours(-5));

    /// <summary>
    /// A <see cref="TimeSpan"/> type
    /// </summary>
    public TimeSpan TimeSpanProperty { get; } = new TimeSpan(1, 2, 3);

    /// <summary>
    /// A <see cref="byte"/> type
    /// </summary>
    public byte ByteProperty { get; } = 255;

    /// <summary>
    /// A <see cref="short"/> short integer type
    /// </summary>
    public short ShortProperty { get; } = 32767;

    /// <summary>
    /// A <see cref="int"/> integer type
    /// </summary>
    public int IntProperty { get; } = -2147483648;

    /// <summary>
    /// <see langword="get"/> the <see cref="long"/> long integer type
    /// </summary>
    public long LongProperty { get; } = 9223372036854775803L;

    /// <summary>
    /// A <c>get</c> of <see cref="float"/> floating point type
    /// </summary>
    public float FloatProperty { get; } = -125.25143f;

    /// <summary>
    /// A <see cref="double"/> floating point type
    /// </summary>
    public double DoubleProperty { get; } = 5.251426433759354d;

    /// <summary>
    /// A <see cref="decimal"/> type
    /// </summary>
    public decimal DecimalProperty { get; } = 7922815.2514264337593543950335m;

    /// <summary>
    /// A <see cref="char"/> type
    /// </summary>
    public char CharProperty { get; } = 'A';

    /// <summary>
    /// A <see cref="bool"/> type
    /// </summary>
    public bool BoolProperty { get; } = true;

    public bool NoXmlProperty { get; }  // false

    /// <summary>
    /// A <see cref="string"/> type
    /// </summary>
    public string StringProperty { get; } = "The lazy dog jumped over the sly brown fox.";
}
