using System.Globalization;
using System.Text;
using static System.Net.WebUtility;

// One renderer for the rotation chart and the snapshot page, shared by the live page and headless runs.
static class Report
{
    private const int Size = 520;
    private const int Pad = 44;

    private static readonly Dictionary<string, string> Colors = new()
    {
        ["Leading"] = "#26a69a",
        ["Weakening"] = "#f5b942",
        ["Lagging"] = "#ef5350",
        ["Improving"] = "#7aa2f7"
    };

    private static string N(double v) => v.ToString("F1", CultureInfo.InvariantCulture);

    public static string Quadrant(DeskSnapshot s)
    {
        List<Regime> rotating = s.Regimes.Where(r => r.Tail.Count > 0).ToList();
        IEnumerable<RotationPoint> points = rotating.SelectMany(r => r.Tail);
        double dx = Math.Max(2, points.Select(p => Math.Abs(p.Ratio - 100)).DefaultIfEmpty().Max() * 1.15);
        double dy = Math.Max(2, points.Select(p => Math.Abs(p.Momentum - 100)).DefaultIfEmpty().Max() * 1.15);
        double half = (Size / 2.0) - Pad;
        double X(double ratio) => (Size / 2.0) + ((ratio - 100) / dx * half);
        double Y(double momentum) => (Size / 2.0) - ((momentum - 100) / dy * half);

        StringBuilder svg = new();
        svg.Append(CultureInfo.InvariantCulture,
            $"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {Size} {Size}" role="img" aria-label="Relative rotation against {HtmlEncode(s.Benchmark)}" font-family="system-ui, sans-serif" font-size="12">""");
        svg.Append(CultureInfo.InvariantCulture, $"""<rect width="{Size}" height="{Size}" fill="#0f1115"/>""");

        int mid = Size / 2;
        (string name, int x, int y, string anchor)[] corners =
        [
            ("Improving", Pad, Pad - 14, "start"), ("Leading", Size - Pad, Pad - 14, "end"),
            ("Lagging", Pad, Size - Pad + 24, "start"), ("Weakening", Size - Pad, Size - Pad + 24, "end")
        ];
        foreach ((string name, int x, int y, string anchor) in corners)
        {
            svg.Append(CultureInfo.InvariantCulture,
                $"""<text x="{x}" y="{y}" text-anchor="{anchor}" fill="{Colors[name]}" font-weight="600">{name}</text>""");
        }
        svg.Append(CultureInfo.InvariantCulture,
            $"""<rect x="{Pad}" y="{Pad}" width="{Size - (2 * Pad)}" height="{Size - (2 * Pad)}" fill="none" stroke="#262a33"/>""");
        svg.Append(CultureInfo.InvariantCulture,
            $"""<line x1="{Pad}" y1="{mid}" x2="{Size - Pad}" y2="{mid}" stroke="#3a404d"/><line x1="{mid}" y1="{Pad}" x2="{mid}" y2="{Size - Pad}" stroke="#3a404d"/>""");
        svg.Append(CultureInfo.InvariantCulture,
            $"""<text x="{Size - Pad}" y="{mid - 6}" text-anchor="end" fill="#8a90a0">RS-ratio →</text><text x="{mid + 6}" y="{Pad + 14}" fill="#8a90a0">RS-momentum ↑</text>""");

        // nudge labels apart where heads crowd together
        Dictionary<string, double> labelY = [];
        List<(double X, double Y)> placed = [];
        foreach (Regime r in rotating.OrderBy(r => Y(r.Tail[^1].Momentum)))
        {
            double x = X(r.Tail[^1].Ratio) + 8, y = Y(r.Tail[^1].Momentum) + 4;
            while (placed.Exists(p => Math.Abs(p.X - x) < 44 && Math.Abs(p.Y - y) < 13)) { y += 13; }
            placed.Add((x, y));
            labelY[r.Symbol] = y;
        }

        foreach (Regime r in rotating)
        {
            string color = Colors[r.Quadrant!];
            string path = string.Join(' ', r.Tail.Select(p => $"{N(X(p.Ratio))},{N(Y(p.Momentum))}"));
            svg.Append(CultureInfo.InvariantCulture,
                $"""<polyline points="{path}" fill="none" stroke="{color}" stroke-opacity="0.4" stroke-width="1.5" stroke-linejoin="round"/>""");
            foreach (RotationPoint p in r.Tail.SkipLast(1))
            {
                svg.Append(CultureInfo.InvariantCulture, $"""<circle cx="{N(X(p.Ratio))}" cy="{N(Y(p.Momentum))}" r="2.5" fill="{color}" fill-opacity="0.6"/>""");
            }
            RotationPoint head = r.Tail[^1];
            string label = HtmlEncode(r.Symbol.Split('/')[0]);
            svg.Append(CultureInfo.InvariantCulture,
                $"""<circle cx="{N(X(head.Ratio))}" cy="{N(Y(head.Momentum))}" r="5" fill="{color}"><title>{HtmlEncode(r.Symbol)}: RS-ratio {N(head.Ratio)}, RS-momentum {N(head.Momentum)}</title></circle>""");
            svg.Append(CultureInfo.InvariantCulture,
                $"""<text x="{N(X(head.Ratio) + 8)}" y="{N(labelY[r.Symbol])}" fill="#e6e6e6">{label}</text>""");
        }

        svg.Append(CultureInfo.InvariantCulture,
            $"""<text x="{Pad}" y="{Size - 8}" fill="#8a90a0">vs {HtmlEncode(s.Benchmark)} · daily · tails: last 4 weeks, one point a week</text></svg>""");
        return svg.ToString();
    }

    public static string Html(DeskSnapshot s)
    {
        string Opt(double? v, Func<double, string> f) => v is double d ? f(d) : "–";
        StringBuilder rows = new();
        foreach (Regime r in s.Regimes)
        {
            rows.Append(CultureInfo.InvariantCulture,
                $"<tr><th>{HtmlEncode(r.Symbol)}</th><td>{Fmt.Price(r.Price)}</td><td>{Opt(r.DayChange, Fmt.Pct)}</td><td>{HtmlEncode(r.Label)}</td>"
                + $"<td>{Opt(r.VsSma200, Fmt.Pct)}</td><td>{Opt(r.Adx, v => v.ToString("F0", CultureInfo.InvariantCulture))}</td>"
                + $"<td>{Opt(r.VolPercentile, Analysis.Ordinal)}</td><td>{Opt(r.Drawdown, Fmt.Pct)}</td><td>{HtmlEncode(r.Quadrant ?? (r.Symbol == s.Benchmark ? "benchmark" : "–"))}</td></tr>");
        }
        string Notes(bool live) => string.Concat(s.Insights.Where(i => i.Live == live).Select(i =>
            $"<li><time>{i.Time.ToString("HH:mm:ss", CultureInfo.InvariantCulture)}Z</time> {HtmlEncode(i.Text)}</li>"));
        string live = Notes(live: true);

        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>Market desk · {{s.AsOf.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)}}Z</title>
              <style>
                body { margin: 0 auto; max-width: 1100px; padding: 24px 16px; font: 14px system-ui, sans-serif; background: #0f1115; color: #e6e6e6; }
                h1 { font-size: 20px; margin: 0 0 4px; } .meta, time, footer { color: #8a90a0; }
                .grid { display: grid; grid-template-columns: minmax(0, 520px) minmax(0, 1fr); gap: 24px; margin-top: 16px; }
                svg { width: 100%; height: auto; border-radius: 8px; }
                ul { list-style: none; padding: 0; margin: 0; } li { padding: 8px 0; border-top: 1px solid #262a33; }
                .wrap { overflow-x: auto; margin-top: 24px; } table { border-collapse: collapse; width: 100%; }
                th, td { text-align: left; padding: 6px 10px; border-bottom: 1px solid #262a33; white-space: nowrap; }
                footer { margin-top: 24px; }
                @media (max-width: 860px) { .grid { grid-template-columns: 1fr; } }
              </style>
            </head>
            <body>
              <h1>Market desk</h1>
              <div class="meta">As of {{s.AsOf.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}} UTC · {{HtmlEncode(s.Provider)}} · {{s.Regimes.Count}} symbols vs {{HtmlEncode(s.Benchmark)}} · {{s.DailyBars:N0}} daily and {{s.MinuteBars:N0}} one-minute bars · today's daily bar is still forming</div>
              <div class="grid">
                {{Quadrant(s)}}
                <section><h2>Briefing</h2><ul>{{Notes(live: false)}}</ul>{{(live.Length > 0 ? $"<h2>Live</h2><ul>{live}</ul>" : "")}}</section>
              </div>
              <div class="wrap"><table>
                <thead><tr><th>Symbol</th><th>Price</th><th>Today</th><th>Daily regime</th><th>vs SMA200</th><th>ADX</th><th>Vol. pctile</th><th>From 1y high</th><th>Rotation</th></tr></thead>
                <tbody>{{rows}}</tbody>
              </table></div>
              <footer>Calculated with FacioQuo.Stock.Indicators {{HtmlEncode(s.PackageVersion)}}: SMA(50), SMA(200), ADX(14), ATR(14), EMA(5) and ROC(10) of relative strength on daily bars; EMA(21)/EMA(55) signals on one-minute bars. Descriptive analysis, not investment advice.</footer>
            </body>
            </html>
            """;
    }
}
