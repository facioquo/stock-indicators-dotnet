# Playbook: add indicators to an existing app

Read [start.md](https://dotnet.stockindicators.dev/agents/start.md) first; its ground rules apply here.

## Finish line

The developer's app calculates the indicators they asked for from its own price data, in the style that fits how that data arrives, with a test that pins the results.

## Fit the library to their code

- **Their bar type.** Implement `IBar` on their existing class rather than copying into `Bar`: `Timestamp`, `Open`, `High`, `Low`, `Close`, `Volume`, and `Value` (usually the close, as a `double`). See [getting started](https://dotnet.stockindicators.dev/guide/getting-started.md#using-custom-bar-classes).
- **The style**, chosen by how bars arrive ([comparison](https://dotnet.stockindicators.dev/guide/styles.md)):

  | Data arrives | Style |
  | ------------ | ----- |
  | As a complete history, recalculated on demand | Series: `bars.ToMacd()` |
  | One closed bar at a time, always in order | Buffer list: `new MacdList()` then `.Add(bar)` |
  | From a live feed that can revise, repeat, or reorder bars, or feeds several indicators at once | Stream hub: `barHub.ToMacdHub()` |

- **Warmup.** Make sure the history they load covers each indicator's warmup, and decide with them whether to drop the warmup rows (`.RemoveWarmupPeriods()`) or keep the nulls.

## Prove it

Add a test that loads a fixed slice of their real data, calculates with Series style, and asserts the last few values. If the app uses Buffer or Stream style, assert that it matches Series on the same bars; the three styles are designed to agree exactly.
