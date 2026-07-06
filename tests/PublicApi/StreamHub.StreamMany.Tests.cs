using Test.Tools;

namespace StressLoading;

[TestClass, TestCategory("Integration")]
public class StreamLoadTests
{
    private static readonly IReadOnlyList<Bar> bars = Data.GetDefault();

    [TestMethod]
    public void StreamMany() // from bar provider
    {
        /******************************************************
         * Attaches many stream observers to one Bar provider
         * for a full sprectrum stream collective.
         *
         * Currently, it does not include any [direct] chains.
         *
         * This test covers most of the unusual test cases, like:
         *
         *  - out of order bars (late arrivals)
         *  - duplicates, but not to an overflow situation
         *
         ******************************************************/

        int length = bars.Count;

        // setup bar hub
        BarHub barHub = new();

        // initialize observers
        AdlHub adlHub = barHub.ToAdlHub();
        AlligatorHub alligatorHub = barHub.ToAlligatorHub();
        AtrHub atrHub = barHub.ToAtrHub();
        AtrStopHub atrStopHub = barHub.ToAtrStopHub();
        EmaHub emaHub = barHub.ToEmaHub(20);
        BarPartHub barPartHub = barHub.ToBarPartHub(CandlePart.OHL3);
        SmaHub smaHub = barHub.ToSmaHub(20);
        TrHub trHub = barHub.ToTrHub();

        // emulate adding bars to hub
        for (int i = 0; i < length; i++)
        {
            // skip one (add later)
            if (i == 80)
            {
                continue;
            }

            Bar q = bars[i];
            barHub.Add(q);

            // resend duplicate bars
            if (i is > 100 and < 105)
            {
                barHub.Add(q);
            }
        }

        // late arrival
        barHub.Add(bars[80]);

        // end all observations
        barHub.EndTransmission();

        // get static equivalents for comparison
        IReadOnlyList<AdlResult> staticAdl = bars.ToAdl();
        IReadOnlyList<AtrResult> staticAtr = bars.ToAtr();
        IReadOnlyList<AtrStopResult> staticAtrStop = bars.ToAtrStop();
        IReadOnlyList<AlligatorResult> staticAlligator = bars.ToAlligator();
        IReadOnlyList<EmaResult> staticEma = bars.ToEma(20);
        IReadOnlyList<TimeValue> staticBarPart = bars.Use(CandlePart.OHL3);
        IReadOnlyList<SmaResult> staticSma = bars.ToSma(20);
        IReadOnlyList<TrResult> staticTr = bars.ToTr();

        // final results should persist in scope
        IReadOnlyList<AdlResult> streamAdl = adlHub.Results;
        IReadOnlyList<AtrResult> streamAtr = atrHub.Results;
        IReadOnlyList<AtrStopResult> streamAtrStop = atrStopHub.Results;
        IReadOnlyList<AlligatorResult> streamAlligator = alligatorHub.Results;
        IReadOnlyList<EmaResult> streamEma = emaHub.Results;
        IReadOnlyList<TimeValue> streamBarPart = barPartHub.Results;
        IReadOnlyList<SmaResult> streamSma = smaHub.Results;
        IReadOnlyList<TrResult> streamTr = trHub.Results;

        // assert, should be correct length
        length.Should().Be(502);
        streamAdl.Should().HaveCount(length);
        streamAlligator.Should().HaveCount(length);
        streamAtr.Should().HaveCount(length);
        streamAtrStop.Should().HaveCount(length);
        streamEma.Should().HaveCount(length);
        streamBarPart.Should().HaveCount(length);
        streamSma.Should().HaveCount(length);
        streamTr.Should().HaveCount(length);

        // assert, should equal static series
        streamAdl.IsExactly(staticAdl);
        streamAtr.IsExactly(staticAtr);
        streamAtrStop.IsExactly(staticAtrStop);
        streamAlligator.IsExactly(staticAlligator);
        streamEma.IsExactly(staticEma);
        streamBarPart.IsExactly(staticBarPart);
        streamSma.IsExactly(staticSma);
        streamTr.IsExactly(staticTr);
    }
}
