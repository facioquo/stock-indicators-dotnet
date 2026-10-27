using System.Text.Json;
using FacioQuo.Stock.Indicators;

// See ConsoleApp first.  This is more advanced.

/************************************************************

 This is a basic 20-year backtest-style analysis of
 Stochastic RSI.  It will buy-to-open (BTO) one share
 when the Stoch RSI (%K) is below 20 and crosses over the
 Signal (%D). The reverse Sell-to-Close (STC) and
 Sell-To-Open (STO) occurs when the Stoch RSI is above 80 and
 crosses below the Signal.

 As a result, there will always be one open LONG or SHORT
 position that is opened and closed at signal crossover
 points in the overbought and oversold regions of the indicator.

 ************************************************************/

// Fetch historical price bars from data provider.
// We're mocking with a simple JSON file import
string json = File.ReadAllText("quotes.data.json");

IReadOnlyCollection<Bar> importedBars = JsonSerializer
    .Deserialize<IReadOnlyCollection<Bar>>(json)
    ?? throw new InvalidDataException("The price-bar file does not contain a JSON collection.");

IReadOnlyList<Bar> bars = importedBars
    .ToSortedList();

// Calculate Stochastic RSI
List<StochRsiResult> resultsList =
    bars.ToStochRsi(14, 14, 3)
        .ToList();

// initialize
decimal trdPrice = 0;
decimal trdQty = 0;
decimal rlzGain = 0;

Console.WriteLine("   Date         Close  StRSI Signal  Cross    Net Gains");
Console.WriteLine("-------------------------------------------------------");

// roll through source values
for (int i = 1; i < bars.Count; i++)
{
    Bar b = bars[i];

    StochRsiResult e = resultsList[i]; // evaluation period
    StochRsiResult l = resultsList[i - 1]; // last (prior) period
    string cross = string.Empty;

    // unrealized gain on open trade
    decimal trdGain = trdQty * (b.Close - trdPrice);

    // check for LONG event
    // condition: Stoch RSI was <= 20 and Stoch RSI crosses over Signal
    if (l.StochRsi <= 20
     && l.StochRsi < l.Signal
     && e.StochRsi >= e.Signal
     && trdQty != 1)
    {
        // emulates BTC + BTO
        rlzGain += trdGain;
        trdQty = 1;
        trdPrice = b.Close;
        cross = "LONG";
    }

    // check for SHORT event
    // condition: Stoch RSI was >= 80 and Stoch RSI crosses under Signal
    else if (l.StochRsi >= 80
     && l.StochRsi > l.Signal
     && e.StochRsi <= e.Signal
     && trdQty != -1)
    {
        // emulates STC + STO
        rlzGain += trdGain;
        trdQty = -1;
        trdPrice = b.Close;
        cross = "SHORT";
    }

    if (cross != string.Empty)
    {
        Console.WriteLine(
            $"{b.Timestamp,10:yyyy-MM-dd} " +
            $"{b.Close,10:c2}" +
            $"{e.StochRsi,7:N1}" +
            $"{e.Signal,7:N1}" +
            $"{cross,7}" +
            $"{rlzGain + trdGain,13:c2}");
    }
}
