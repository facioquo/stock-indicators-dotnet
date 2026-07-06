# Streaming Indicators implementation plan

This document tracks remaining work and architectural direction for the v3 streaming indicators implementation.

**Status (2026-07-01).** The focused quality pass shipped across §B Guidance alignment (G001–G008), §C Pre-v3.0 cleanup (T230/T232–T234; T203 deferred on CI SDK), §D Test coverage hardening (TC001–TC006), §E Architecture documentation (DOC-ARCH-1 through DOC-ARCH-7; ADR 0001 in `docs/decisions/` codifies the dual-track model), §F PV001 (Slope BufferList O(1) verification), §G Documentation (D009/D010/D011), and §I (T216 ConnorsRsi doc + T217 CMO test). The architecture is unchanged. **RG001 baseline refresh is now complete (including both `*-report-full.json` and `*-report-github.md` baseline artifacts), RG002/RG003 are complete, and RG004 shipped; remaining v3.0 work is §K release mechanics plus deferred T203.** A pre-release confidence review (overall-review-v3 swarm, 2026-05-29) confirmed the rollback/replay engine is ship-quality — bit-exact across deep chains, aggregators, and long-run pruning, with 1,072 streaming tests green — and surfaced surface-level correctness, documentation, and tooling defects (tracked in §E), now **cleared**: shipped fixes, maintainer-deferred SR001, and two framework-scale items reclassified to v3.1 (ARCH-V31-11 prune-stable rollback, SR008e BufferList enforcement). All shipped quality-pass items are recorded in the appendix at the end of this document with their PR references; the body below carries only what is still open.

**Update (2026-06-30) — RG001 resolved: YES (shipped).** Performance baselines were refreshed from current BenchmarkDotNet runs, with both machine-readable and human-readable artifacts committed in `tools/performance/baselines/` (`Performance.*-report-full.json` and `Performance.*-report-github.md`).

**Coverage (verified 2026-05-24 via `CatalogShouldHaveExactStyleCounts` at `tests/Library/_common/Catalog/Catalog.Metrics.Tests.cs:21`):**

- Series listings: 85 (84 indicators + `BarPart`)
- BufferList listings: 79 (78 indicators + `BarPart`)
- StreamHub listings: 79 (78 indicators + `BarPart`)
- Streaming docs coverage: 79 of 79
- Non-streamable (Series only): Beta, Correlation, Prs, RenkoAtr, StdDevChannels, ZigZag

**Related plans**: [File Reorganization](file-reorg.plan.md) (deferred to v3.1). The Branching Strategy Migration plan has been retired — its migration completed (RG003 / K007, via PR #1014).

**Related guidance** (cross-reference for contributors and AI agents):
`AGENTS.md` (root), `src/AGENTS.md`, `tests/AGENTS.md`, `docs/AGENTS.md`, `.agents/skills/indicator-stream/SKILL.md`, `.agents/skills/indicator-buffer/SKILL.md`, `.agents/skills/indicator-catalog/SKILL.md`, `.agents/skills/performance-testing/SKILL.md`. Guidance docs are in alignment with the shipped v3 surface as of the §B closeout.

---

## Recommendation — v3.0 stable pending release-gate and §K release-mechanics work (§E hardening shipped)

The quality pass landed across §B/§C/§D/§E/§F/§G — guidance docs aligned, test coverage hardened, architecture decisions formalized in `docs/decisions/0001-dual-track-bufferlist-streamhub.md` (ADR 0001), and documentation gaps closed. No architectural change was required. The Architect verdict (no blockers, four v3.1 refactors queued) and Tester verdict (parity strong; rigor gaps addressed by TC001 + TC002 + TC003) stand.

What remains before tagging v3.0.0 is **operational, not implementation work**: execute the remaining §K go-live launch checklist (package publish, DNS cutover, doc-site deploy). RG001 baseline refresh, RG002 community feedback, RG003 branching migration, and RG004 Quote→Bar are complete. The branching-strategy migration (RG003 / K007) — the irreversible cut-over inside §K — has already been executed via PR #1014.

The 2026-05-29 confidence review (6 static finders + 4 dynamic exercisers — offline SSE emulator, BenchmarkDotNet, deep-chain and lifecycle stress harnesses — with adversarial per-finding verification) returned a **ship-as-preview / hold-stable** verdict: continue the `preview.4` line, hold the K013 stable tag until §E clears. **§E has now cleared (PRs #2052–#2056)** — the engine needed no redesign and the pass closed out the silent-drop correction (SR002), the BufferList out-of-order contract (SR008, documented + tested), the prune+rollback characterization (SR003, documented; framework fix → ARCH-V31-11), observer-callback isolation (SR004/SR005, code-fixed), the doc/tooling fixes (SR011–SR016, SR018, SR021), and the offline emulator (SR021). SR001 (before-head silent drop) was reviewed and **deferred** by the maintainer. So the K013 stable hold is released from the §E side; remaining gates are §A (release-gate runtime) and §K (release mechanics).

### What still has to happen before tagging v3.0.0

| Bucket | Items | Estimated effort |
| ------ | ----- | ---------------- |
| Release gates | ~~RG001 baseline refresh~~ ✅ done (2026-06-30), ~~RG002 community feedback~~ ✅ done, ~~RG003 branching migration~~ ✅ done — PR #1014, ~~RG004 Quote→Bar decision~~ ✅ resolved — shipped in v3.0 | done |
| ~~Pre-stable hardening~~ ✅ **shipped** | §E below — overall-review-v3 swarm (SR002/003/004/005/006/008/011–016/018/021) shipped in PRs #2052–#2056; SR001 maintainer-deferred; framework fixes → v3.1 (ARCH-V31-11, SR008e) | done |
| Release mechanics | §K below — go-live launch checklist (FacioQuo rebrand, repo transfer, DNS cutover, branching migration via K007 / RG003) | 6–10 hours active + NuGet/DNS propagation |
| Residual cleanup | ~~T231 baseline-folder delete~~ ✅ done, T203 preview-features flag (deferred on CI SDK) | 30 min when unblocked |

### v3.1+ direction — what's worth tackling next

After v3.0 ships, the highest-leverage streaming work is (in priority order):

1. **`Reinitialize()` abstraction (T205).** Replaces replay-on-`OnAdd` with subclass-controlled fast-path bulk init via Series static methods. TODO is already at `src/_common/StreamHub/StreamHub.cs:230`.
2. **Retire `BaseProvider<T>` (Architect F1, Stercorator F1).** Introduce `StreamSource<T>` as a root base separate from `StreamHub<TIn,TOut>`; collapse the workaround. Non-breaking internal refactor — too risky to land in a release-hardening window, perfect for v3.1.
3. **`RollbackState(int)` contract clarification and audit (Architect F2).** XML-document precise index semantics, audit the 55 overrides against the formalized contract. Lightweight v3.0 doc work + v3.1 broader audit.
4. **Private cache lock + routed mutation methods (Architect F3).** Replace `lock (Cache)` with a private monitor, expose `AppendResult`/`ReplaceAt`/`TruncateFrom`. Non-breaking.
5. **`JoinHub<TLeft,TRight,TOut>` primitive (Architect F7, Researcher F8).** Re-enable Beta/Correlation/Prs streaming via an explicit zip-by-timestamp semantic (Rx `Zip`/`CombineLatest` analog). PairsProvider revert (PR #1821) attempted this without a designed primitive.
6. **Rx and `IAsyncEnumerable<T>` adapters (Researcher F1, F2).** `ToObservable()` and `ConsumeAsync(IAsyncEnumerable<TIn>, CancellationToken)` extensions so hubs interoperate with the rest of .NET's streaming ecosystem.
7. **Shared "increment kernels" expansion (Inspector F1, ARCH-V31-7).** Seven indicators ship kernels today (`Ema`, `Sma`, `Tr`, `Atr`, `Adl`, `Obv`, `Dynamic` — see `src/*/*/*.Utilities.cs`); adoption is 39 of 158 streaming files (~25%), with the BufferList side carrying most of the duplication tax (10/79 vs 29/79 on StreamHub). Sub-items 7a–7f below carry PR-sized scope ordered by ROI.

Medium-priority enhancements (composite naming E010, MaEnvelopes remaining MA types T214, BufferList configuration E009, ADX DMI properties E007–E008, ISeries.UnixDate T226) can land independently in v3.1 minor releases. (T215 Hurst Anis-Lloyd already shipped in PR #2007.)

---

## v3.0 Work Remaining

### A. Release gates — non-implementation

- [x] **RG001 — Refresh performance baselines** (completed 2026-06-30). Baselines now reflect post-fix runs; refresh procedure copies both `Performance.*-report-full.json` and `Performance.*-report-github.md` from `BenchmarkDotNet.Artifacts/results/` to `tools/performance/baselines/`.

- [x] **RG002 — Get and incorporate final community feedback** (ongoing). Time-boxed by maintainer.

- [x] **RG003 — Execute branching strategy migration** (10–16 hours). Completed via PR #1014 (the merge vehicle); `v3` was promoted to `main`. The standalone branching-strategy plan has been retired now that the migration shipped.

- [x] **RG004 — `Quote → Bar` rename for v3.0 — RESOLVED: YES, shipped** (PR #1933, bundled in #2102).
  - **Context**: PR #1014 comment 1 (2024-07-01) proposed renaming `Quote` → `Bar` (with `Tick` reserved for individual bid/ask trades) and introducing `IBar` with `[Obsolete] IQuote` as a base. v3.0 was the only realistic window because it's a breaking rename; doing it later would have meant a v4.0.
  - **Trade-off (historical)**: YES → cleaner nomenclature aligned with industry (Pine Script "bar", TA-Lib OHLCV "bar") and a clean `Tick`/`Bar` distinction now that `TradeTickHub` exists (PR #1875). NO → less churn for consumers already migrating from v2; would have kept `IQuote` as the canonical bar interface; deferred cleanup to a potential v4.x.
  - **Outcome (YES)**: the rename shipped in v3.0 and `docs/migration/v3.md` was refreshed. **Warning-level** `[Obsolete]` aliases in `Obsolete.V3.Other.cs` cover `Quote`/`IQuote`/`PeriodSize`/`IReusableResult`/`BasicData` so v2 code keeps compiling and running during a deprecation window; `PeriodSize` callers are served by `Aggregate(PeriodSize)`/`GetPivotPoints(PeriodSize)` forwarding overloads. ARCH-V31-9 is closed as done in v3.0.

### B. Pre-v3.0 cleanup pass

- [ ] **T203 — Remove `EnablePreviewFeatures=true` from project configuration** (30 min, **deferred — blocked on CI SDK**).
  - **Evidence**: `src/Indicators.csproj:11` enables preview features for the `field` keyword used at `src/_common/BufferLists/BufferList.cs:54,56`.
  - **Deferral note (2026-05-25)**: PR #2026 attempted the removal and was closed after CI failure. Locally the build is green (SDK `10.0.300` treats `field` as GA), but the SDK that GitHub Actions `setup-dotnet` pulls under `dotnet-version: "10.x"` + `dotnet-quality: "ga"` still flags `field` as preview (`CS8652: The feature 'field keyword' is currently in Preview and *unsupported*`). The flag is therefore still load-bearing for the SDK the CI runners select today.
  - **Re-attempt trigger**: ship this item when `setup-dotnet@v4` resolves `10.x` + `ga` to an SDK whose Roslyn ships `field` as GA. Quick re-verification: bump a no-op csproj change in a throwaway PR, watch the `quick check` job — if it passes without `EnablePreviewFeatures=true`, T203 is ready.
  - **Alternative path if waiting is unacceptable**: pin a specific SDK via `global.json` to a version known to have `field` GA, then drop the preview flag. Has the side effect of pinning all CI to one SDK rev; ask before introducing.

- [x] **T231 — Delete `tools/performance/baselines/before-fixes/`** (completed 2026-07-01).
  - **Resolution**: deleted; historical snapshots remain available through git history/tags when needed.

### C. Infrastructure — deferred but listed for context

- [x] **File reorganization** — [#1810](https://github.com/facioquo/stock-indicators-dotnet/issues/1810). See [file-reorg.plan.md](file-reorg.plan.md). Non-breaking portion implemented (directories, ~500 file renames, test class standardization); breaking type renames deferred to v4 sub-issues #2137–#2140.

### D. Release mechanics — go-live launch checklist

**Source**: incorporated 2026-05-24 from private project board item 58144081 ("v3 go-live launch checklist") and PR #1014 comment 3 (DNS/NuGet/deployer URL pointers). Tasks here are release plumbing executed by the maintainer; they run in parallel with §A release gates RG001/RG002/RG003. (The §A RG004 Quote→Bar decision is resolved — shipped in v3.0.) Total active effort ~6–10 hours plus async waits for NuGet package indexing and DNS TTL.

> **Cross-cutting decision (already made)**: v3 ships under a new package identity `FacioQuo.Stock.Indicators` with the repo transferring to a FacioQuo org as `stock-indicators-dotnet`. K-items below assume that transition; if the rebrand is cancelled or postponed, items K001–K006 and K009–K011 collapse to a single "tag and release on existing `FacioQuo.Stock.Indicators` package" item.

#### Package and naming

- [ ] **K001 — Reserve and publish `FacioQuo.Stock.Indicators` package** (1 hour active + NuGet indexing). Cut the v3.0 stable as the first listed version on this package ID; do not publish previews here (previews remain on `Skender.Stock.Indicators` for continuity per K003).
- [x] **K002 — Add "previously known as Skender.Stock.Indicators" banner** (30 min). Visible in README header, repo summary/About, NuGet package description, and the first paragraph of release notes for v3.0.0. Keep the v2 download-count badge alongside the new badge for the deprecation window.
- [x] **K003 — Unlist all `3.0.0-preview.*` versions on `Skender.Stock.Indicators`** (30 min). After K001 publishes, mark `3.0.0-preview.1014.15`, `3.0.0-preview.1014.12`, `3.0.0-preview.1014.25`, `3.0.0-preview.2`, etc. as unlisted on NuGet.org with a description note pointing to `FacioQuo.Stock.Indicators 3.0.0`.
- [ ] **K004 — Release final `Skender.Stock.Indicators` v2.x patch with "we moved" notice** (1–2 hours). Stays in draft until cut-over. Release note: "We moved to `FacioQuo.Stock.Indicators` — install that package for v3+. This v2 line remains on `v2` branch for netstandard2.x compatibility maintenance only (per RG005)."

#### Source repo and project transfer

- [x] **K005 — Transfer `DaveSkender/Stock.Indicators` → `facioquo/stock-indicators-dotnet`** (1 hour + GitHub's auto-forwarding propagation). Confirms GitHub's HTTP redirect from old to new URLs; verify a few links from external blog posts continue resolving. Audit and prune obsolete contributors during transfer. **Companion repos already transferred** per project board: `Stock.Indicators.Python` → `stock-indicators-python`, `Stock.Indicators.Python.QuickStart` → `stock-indicators-python-quickstart`, `Stock.Charts` → `stock-charts`. Re-verify before cut-over.
- [ ] **K006 — Transfer or reconnect Stock.Indicators project boards** (30 min). The public Stock.Indicators project, if any, plus link the private `DaveSkender/projects/6` to the new repo location.

#### Branching and v2 maintenance

- [x] **K007 — Execute branching-strategy migration** — canonical entry is §A **RG003** (with effort estimate + PR #1014 reference). This was the irreversible cut-over; it shipped after the §A RG004 decision was finalized (YES). The standalone branching-strategy plan has since been retired.

#### Documentation and URL/DNS

- [x] **K009 — Lowercase all doc-site page URLs + Jekyll/VitePress redirects for old casing** (1–2 hours). Required because GitHub Pages canonical URLs and external inbound links may be case-sensitive depending on the docs platform. Audit `docs/` page paths; add redirect rules for any URL that changes case. Verify a sample of external blog post inbound links continue resolving.
- [x] **K010 — Cut `dotnet.stockindicators.dev` DNS to production doc site; remove `/v3` preview pointer** (15 min + DNS TTL). Update CloudFlare CNAME/A records; remove the temporary `dotnet.stockindicators.dev/v3` pointer added during preview phase per PR #1014 comment 3.
- [x] **K011 — Update NuGet release-notes URL and release-deployer URL references to new package name** (30 min). Search source/build pipelines for `FacioQuo.Stock.Indicators` references in release-note URLs, deployer config, badge URLs; replace with `FacioQuo.Stock.Indicators` equivalents. Per PR #1014 comment 3.
- [x] **K012 — Update in-repo GitHub URLs to lowercase canonical form** (30 min). Grep source/docs/markdown for `github.com/facioquo/stock-indicators-dotnet` references; replace with the new owner/repo (and lowercase) per K005. Jekyll/VitePress redirects (K009) handle inbound external links; this item handles outbound links from within the repo.

#### Cut-over and finalize

- [ ] **K013 — Tag `3.0.0` and publish stable release** (1 hour). Tag on the post-migration `main` branch. Publish GitHub Release with full release notes (consolidated from v3 preview release notes + post-preview changes). Push NuGet package to `FacioQuo.Stock.Indicators`. Close PR #1014; post the locked summary comment in Discussion #1018.
- [ ] **K014 — Deploy doc site with v3.0.0 link refs** (30 min + CDN propagation). VitePress build with updated URLs, NuGet badges pointing to new package, examples updated to install command for `FacioQuo.Stock.Indicators`.
  - Before deploy, run `bash docs/.vitepress/test-a11y.sh` and resolve any regressions. (Last open item carried over from the now-retired `documentation-site.plan.md`; the site IA work itself is complete.)
- [ ] **K015 — Update examples project to consume released `FacioQuo.Stock.Indicators` 3.0.0** (30 min). Bump package reference; verify build; push.
- [ ] **K016 — Cascade meta updates to Python and Charts repos** (1 hour). README badges, "previously known as" notices, link refs in companion repo READMEs and doc sites. Mostly mechanical given the python and charts repos already transferred.
- [ ] **K017 — `Skender.Stock.Indicators` long-term wind-down** (15 min set-up + ongoing). The library is **not** unlisted at v3 cut-over (preview-version unlisting is handled by K003). Instead, `Skender.Stock.Indicators` remains listed as a v2-only maintenance package for ~1 year following v3.0 release, accepting only security/compatibility patches per the retired branching-strategy plan's v2-branch role (RG005 + K008). After the ~12-month maintenance window, transition the listing to a more fully deprecated state (NuGet may not support full unlisting once installs have occurred; document the chosen mechanism — e.g., a final `[Deprecated]` package metadata update with a "please migrate to FacioQuo.Stock.Indicators" notice — in this checklist when reached).

### E. Pre-stable hardening — overall-review-v3 swarm review (2026-05-29)

**§E CLOSEOUT (2026-05-31).** All High items are resolved or maintainer-deferred, so the K013 stable hold no longer waits on §E. Shipped across **PRs #2052–#2056**: SR002 (correction fix), SR003 (characterized + documented; framework fix → ARCH-V31-11), SR008 (documented + tested; enforcement → SR008e), SR004/SR005 (observer-callback isolation code-fixed), SR006 (documented; code → SR006c), SR011–SR016/SR018/SR021 (doc + tooling). SR001 (before-head silent drop) reviewed and **deferred** by the maintainer. The non-blocking v3.1+ follow-ups are below.

Source: pre-release confidence review (6 static finders + 4 worktree dynamic exercisers — offline SSE emulator, BenchmarkDotNet, deep-chain + lifecycle stress harnesses — with adversarial per-finding verification that defaulted to *refuted*). Verdict: **ship as preview, hold the v3.0 stable tag (K013) until the High items below clear.** The rollback/replay engine passed empirically — bit-exact across 3-deep chains, scrambled multi-late arrivals, aggregator boundaries, the stdev family, and mid-stream rebuild; 1,072 streaming tests green; long-run memory bounded; `ResetFault()` recovery verified working. These are surface defects around that engine, not engine redesign. Non-blocking follow-ups are grouped under v3.1+ below; per-finding evidence (file:line + repro) lives in the review write-up / PR description, not here.

#### Correctness — stable blockers

- [ ] **SR001 — Standalone BarHub silently drops before-head bars** (High). `src/_common/Bars/Bar.StreamHub.cs:90` returns on `item.Timestamp < Cache[0].Timestamp` with no exception, notification, or status. Empirically confirmed silent data loss on out-of-order / two-feed-merge / backfill patterns. Surface the drop (status / `OnError` / dropped-bar callback) and document the before-head policy on `IStreamHub.Add`. *(Reviewed 2026-05-30 — deferred; **⚠️ REOPENED 2026-05-31 as a pending v3.0 decision** per the quality steer — silent data loss is the canonical "obvious problem." See §L.)*
- [x] **SR002 — Standalone BarHub silently drops same-timestamp value corrections below MinCacheSize** (High). `src/_common/Bars/Bar.StreamHub.cs:117`. Empirically confirmed divergence at index < MinCacheSize (streamed root vs fresh hub); the general rollback path accepts the identical correction (harness S16), so the guard is provably over-broad. Route the correction through the general rollback path or surface the rejection. Free correctness win. *(PR #2053 — removed the over-broad guard; corrections now route through the existing replacement+rebuild path; correct for caches ≥ warmup, the pruned small-cache case stays SR003)*
- [x] **SR003 — Prune + sub-horizon rollback re-seeds Wilder/cumulative indicators from truncated history** (High; decision). `src/_common/StreamHub/StreamHub.cs:179` passes a `ProviderCache` index to `RollbackState`; `src/a-d/Adx/Adx.StreamHub.cs:228,255,294` and `src/a-d/Atr/Atr.StreamHub.cs:50` re-seed Wilder smoothing with **absolute** warmup gates, so after a head-prune the seed is rebuilt from a truncated suffix → silently wrong (non-null) ADX/ATR/AtrStop values vs the Series oracle. Analytically confirmed (two skeptics), **not** reproduced — the 100k default cache masks it; reachable via the `stream.md:277` minimal-cache advice. Decide: enforce `MinCacheSize ≥ true warmup span`, reject rollbacks preceding `ProviderCache[0]`, gate warmup on a prune-stable processed-count (as Slope does via `OnProviderPrune`), or document "small cache + deep late arrival unsupported." Add a small-cache prune+rollback oracle test. *(PR #2055 — confirmed AND scope-corrected: this is a framework-level property of **every** stateful hub, not just Wilder. Reproduced across EMA, ATR, ADX, RSI, SMMA, AtrStop, OBV (`StreamHub.RollbackAfterPrune.Tests.cs`); the rollback engine is exact with an adequate cache (positive control passes). Maintainer decision 2026-05-30: ship stable with the limitation **documented** (`stream.md` rollback-after-prune warning) — recursive smoothers drift transiently and re-converge, cumulative hubs (no warmup) carry a permanent offset with no leading nulls, and warmup/lookback indicators (window like SMA + recursive smoothers) lose their leading results to null where the retained cache no longer spans a full lookback after a post-prune rebuild; real framework fix reclassified to **ARCH-V31-11**.)*
- [x] **SR008 — BufferList track has no out-of-order / duplicate / correction contract or test** (High). `src/s-z/Sma/Sma.BufferList.cs:38` and every sibling append blindly; `src/_common/BufferLists/IIncrementFromBar.cs:21` and `IIncrementFromChain.cs:28` document a chronological precondition that nothing enforces and no test pins. A BufferList fed from the WebSocket source `stream.md` encourages is silently wrong on any reordered/re-sent tick. Enforce/guard the precondition or document it prominently + add an out-of-order test. Largest proven-vs-shipped gap (completeness-critic #1 residual risk). *(PR #2054 — document path: prominent precondition on `BufferList<T>` + both increment interfaces + `buffer.md`; `BufferList.OrderingContract.Tests.cs` pins ordered-matches-series, out-of-order-appended-verbatim, duplicate-appends-not-corrects. Enforcement deferred to SR008e.)*

#### Documentation / tooling — cheap, clear before stable

- [x] **SR011 — `stream.md:112` rollback example calls non-existent `barHub.Remove(badBar)` → does not compile** (High). No `Remove(TIn)` exists on the streaming surface; use `RemoveRange(badBar.Timestamp, true)` or `RemoveAt`. Align the thread-safety bullets to name the real methods (`RemoveAt`/`RemoveRange`). *(PR #2052)*
- [x] **SR012 — `migration.md:311` flagship v2→v3 StreamHub example uses `ToSma`/`ToRsi`/`ToMacd` (missing `Hub` suffix) → does not compile** (High). *(PR #2052)*
- [x] **SR013 — `stream.md:277` "minimize cache size" advice steers users into `ArgumentOutOfRangeException`** (Medium). `MaxCacheSize` is inherited by chained hubs (`StreamHub.cs:37`) and `ValidateCacheSize` floors at warmup (RSI 2×, TEMA 3×, MAMA 50). Warn re inheritance + the warmup multiplier; recommend a safe floor rather than "the indicator minimum." *(PR #2052)*
- [x] **SR014 — `stream.md:241` WebSocket example calls `Add` from an async handler with no serialization, contradicting the page's own thread-safety contract** (Medium). Funnel `Add` through a single writer (lock / `SemaphoreSlim` / single-consumer `Channel<Bar>`), matching the thread-safe example shown earlier on the page. *(PR #2052)*
- [x] **SR015 — Fault model undocumented for users** (Medium). `IsFaulted`/`ResetFault`/`OverflowException`/`OverflowCount` appear nowhere in the guide; 100 byte-identical same-timestamp ticks fault the hub (`StreamHub.cs:567`), a real trade-print pattern. Document the fault + recovery model in `stream.md`; consider not faulting on external (non-cascade) identical ticks. (`ResetFault()`-only recovery is verified to resume correctly.) *(PR #2052; "don't fault on external identical ticks" remains a v3.1 robustness option)*
- [x] **SR016 — Docs claim empty `Results[^1]` throws `IndexOutOfRangeException`; actual is `ArgumentOutOfRangeException`** (Low). `stream.md:62` + two sibling pages (`migration.md:284`, `buffer.md:56`). *(PR #2052)*
- [x] **SR018 — No same-timestamp tail-correction (different-value) test for any stateful hub** (High; test). The exact path SR002 exploits; `tests/Library/_common/StreamHub/StreamHub.CacheMgmt.Tests.cs:284` only drives a stateless `BarPartHub`. Add a correction-vs-fresh-oracle test (EMA family, Stoch/StochRsi buffer state, AtrStop direction latch) to the per-indicator late-arrival suite. *(PR #2053 — `StreamHub.SameTimestampCorrection.Tests.cs`: Sma/Atr/AtrStop/Stoch sub-MinCacheSize correction vs fresh-hub oracle; fails before SR002 fix, passes after)*
- [x] **SR021 — Offline SSE emulator's `/bars/random` endpoint returns HTTP 500** (Medium; tooling). `tools/sse-server/Program.cs:64` builds `RandomGbm(bars: 0, …)` but `tests/Library/_testdata/TestData.Random.cs:40` throws on `bars <= 0`; the documented offline streaming-validation path (`dotnet run -- sse bar …`) delivers zero bars. Pass a positive `bars` count. *(PR #2052 — verified HTTP 200 with bars flowing)*

#### Robustness — document for v3.0, code-fix in v3.1

- [x] **SR004 — One throwing observer aborts notification to its siblings → silent desync** (Medium). `src/_common/StreamHub/StreamHub.Observable.cs:147` (and the three sibling notify loops) iterate `_observers.ToArray()` with no per-callback try/catch; `custom-observers.md:26` ("peer subscriber receives the same notifications") implies an isolation that does not exist. v3.0: document the no-throw observer contract. v3.1: isolate each callback (see SR004c/SR005c). *(Fully resolved in v3.0 — doc PR #2052, isolation fix PR #2056 (pulled forward from SR004c per maintainer): each `NotifyObserversOn*` callback is guarded and a thrower is routed to its own `OnError`; `custom-observers.md` rewritten to describe the robust behavior.)*
- [x] **SR005 — Faulting `OnError` rethrows inside `NotifyObserversOnError` → later siblings never notified** (Medium). `src/_common/StreamHub/StreamHub.Observer.cs:139` (`=> throw exception;`) inside the un-isolated error loop. v3.0: document; v3.1: isolate error fan-out. *(Fully resolved in v3.0 — doc PR #2052, fix PR #2056 (pulled forward from SR005c): the hub-as-observer `OnError` now cascades downstream via `NotifyObserversOnError` instead of rethrowing.)*
- [x] **SR006 — `Reinitialize()` rebuild→subscribe gap can drop high-frequency data** (Medium). `src/_common/StreamHub/StreamHub.cs:228`, flagged by the in-source TODO at `:233` (distinct from the `:230` TODO tracked by T205). v3.0: document that `Reinitialize` requires externally serialized `Add`. v3.1: subscribe-before/atomic-with rebuild, or catch-up rebuild after subscribe (see SR006c). *(v3.0 doc-half shipped — PR #2052; code-half remains SR006c)*

### L. v3.0 stable-readiness — streaming-debut quality (reclassified from v3.1, 2026-05-31)

The v3 streaming engine is the headline of this release after a long development cycle. Maintainer steer (2026-05-31): v3.0 must **not** ship hasty or with obvious problems in the new streaming features. The items below were filed under v3.1+ but belong in the stable debut and are reclassified here — the API/behavior items are cheap pre-stable and breaking after; the correctness items are real (silent) data-loss/race defects in the key features; the coverage items prove the rollback/concurrency engine before the tag. None of these need a maintainer decision; the three that do are highlighted at the end and are **not** scheduled.

#### API / behavior finalization (one-way door — land before the stable tag)

- [x] **ARCH-V31-12 — Guard mutation on subscribed (non-root) hubs + `Remove(IBar)`** (2–3 hours). Make `Add`/`RemoveAt`/`RemoveRange`/`Reinitialize` throw `InvalidOperationException` (or a documented no-op) on a non-root hub — detect via `BarHub._isStandalone` / real-`Provider`-vs-inert-`BaseProvider<T>` — so a leaf can't be desynced from its provider; add `Remove(IBar)` on the root hub (find-by-timestamp → `RemoveAt`/`RemoveRange`; no-op-or-throw if absent). Public-behavior change → tests + tighten `stream.md` from *documenting* the root-hub rule to *enforcing* it. (Approved as v3.1 in the #2052 review; reclassified to v3.0 because it's breaking.) *(PR #2059)*
- [ ] **SR017 — `IDisposable` / chain teardown on `StreamHub`** — **DEFERRED from v3.0 → [#2068](https://github.com/facioquo/stock-indicators-dotnet/issues/2068).** Built in PR #2060, then withdrawn: hubs hold no unmanaged resources, `EndTransmission`/`Unsubscribe` + GC proved sufficient (no observed need), and most of the feature's cost (CA2000 suppressions, the `MarkAsInternalHub` ownership flag, the re-entrancy guard) was self-induced by adding `IDisposable`. Adding `IDisposable` later is non-breaking, so deferral is reversible; revisit per #2068.
- [x] **ARCH-V31-3 — `Snapshot()` for an immutable cache copy** (2 hours). Keep `Results` as the live view; add `Snapshot()` returning `IReadOnlyList<TOut>` taken under `CacheLock`. Backs the thread-safety guidance shipped in #2052 ("copy or snapshot before handing `Results` to another thread"). *(PR #2061)*

#### Correctness — small real races in the key features

- [x] **SR007 — Guard `_observers` against Subscribe-during-stream races** (2–3 hours). `StreamHub.Observable.cs:40,51,95` mutate `_observers` unsynchronized vs the notify-loop enumeration; constructing a downstream hub concurrent with `Add` is a real race. Guard `_observers` (snapshot-on-mutate or lock). Pairs with TC-V31-1. *(PR #2069)*
- [x] **SR006c — Close the `Reinitialize` rebuild→subscribe gap** (part of T205 / `StreamHub.cs:233`). The gap can drop high-frequency data; subscribe-before/atomic-with rebuild, or re-run a catch-up rebuild after Subscribe returns. Data-loss → in scope for the debut. *(PR #2069)*

#### Test coverage — prove the rollback/concurrency engine before the tag

- [x] **TC-V31-1 — StreamHub concurrency test suite** (4–6 hours). Parallel `Add` from N threads, interleaved late arrivals, concurrent observer subscribe/dispose. The safety net validating the documented single-writer / thread-safety contract. *(PR #2070)*
- [x] **TC-V31-4 — Aggregator-hub rollback-equivalence** (2 hours). `BarAggregatorHub`/`TradeTickAggregatorHub` override `RollbackState` but aren't in the catalog, so TC001 doesn't exercise them; catalog-register or add two hand-built rollback-equivalence cases to `StreamHub.RollbackContract.Tests.cs`. *(PR #2071)*
- [x] **SR020 — Deep-chain rebuild value-equality** (2 hours). `StreamHub.BoundsChecking.Tests.cs:810` asserts count/timestamp only after a chained `RemoveAt`; extend to `IsExactly` vs a Series-equivalent chain. *(PR #2071)*
- [x] **TC-V31-8 — Chained-downstream late-arrival through an aggregator** (1–2 hours). `BarHub → AggregatorHub → EmaHub` (+ tick analog) late-arrival test; assert downstream `EmaHub.Results` bit-equality between late and fresh chains — catches a dropped-notification-per-replay regression. *(PR #2071)*
- [x] **TC-V31-7 — BufferList bounded-value parity** (1–2 hours). Add `Boundary_WithRandomBars_StaysWithinBounds` to each bounded `*.BufferList.Tests.cs` (RSI/Stoch/Aroon/MFI/Ultimate/ConnorsRsi/WilliamsR), matching the Series + StreamHub coverage. *(PR #2072)*

#### ⚠️ Pending maintainer decisions — highlighted, NOT in the next batch

- **RG004 → ARCH-V31-9 — `Quote` → `Bar` rename — ✅ RESOLVED: YES, shipped in v3.0** (PR #1933). Old names ship as warning-level `[Obsolete]` aliases; see the 2026-06-19 update at the top. (Detail in §A RG004 and ARCH-V31-9 below.)
- **SR001 — Before-head silent drop.** Deferred 2026-05-30; the 2026-05-31 quality steer reopens it — silent data loss on out-of-order / two-feed-merge / backfill is the canonical "obvious problem." Decide: surface the drop (status / `OnError` / dropped-bar callback) in v3.0, or hold the defer. (Detail in §E above.)
- **ARCH-V31-11 — Prune-stable rollback (the SR003 real fix).** Largest correctness gap in the rollback engine but a multi-day, dozens-of-hubs effort. Decide: pull into v3.0 (engine bit-exact under every cache config) or ship the documented edge-config limitation. (Detail in v3.1+ below.)

---

## v3.1+ Enhancements — Deferred Work

> **Reclassified to v3.0 (see §L, 2026-05-31):** ARCH-V31-3, ARCH-V31-12, SR006c, SR007, SR017, SR020, TC-V31-1, TC-V31-4, TC-V31-7, TC-V31-8 — now v3.0-scheduled in §L; the detailed entries below are retained for reference. **Pending a v3.0 decision (see §L):** ARCH-V31-11 (prune-stable rollback). (ARCH-V31-9 / RG004 Quote→Bar is now resolved — shipped in v3.0.) Everything else here is genuinely v3.1+.

### Framework architecture improvements (new — Architect findings)

- [ ] **ARCH-V31-1 — Retire `BaseProvider<T>`, introduce `StreamSource<T>`** (4–6 hours).
  - Source: Architect F1, Stercorator F1.
  - Internal refactor; no public API change. Resolves the self-flagged TODO at `src/_common/StreamHub/Providers/BaseProvider.cs`.

- [ ] **ARCH-V31-2 — Routed mutation methods on top of the private cache monitor** (2 hours, **scope narrowed**).
  - Source: Architect F3.
  - **Partial completion verified (2026-05-25)**: the private monitor `CacheLock` is already in place at `src/_common/StreamHub/StreamHub.cs:16`, and the `lock (CacheLock)` pattern (not `lock (Cache)`) is in use across the framework. The public-field-as-monitor anti-pattern is already eliminated.
  - **Remaining work**: route all cache mutations through dedicated `AppendResult`/`ReplaceAt`/`TruncateFrom` methods so subclasses cannot directly `Cache.Add(...)` / `Cache.RemoveAt(...)`. Today mutation still flows through `AppendCache` (`StreamHub.cs:324`) plus direct `Cache.Add/Remove` calls in subclass overrides.

- [ ] **ARCH-V31-3 — `Snapshot()` method for immutable cache copies** (2 hours).
  - Source: Architect F4.
  - Keep `Results` as the live view (with documented hazards); add `Snapshot()` returning `IReadOnlyList<TOut>` taken under the cache lock.

- [ ] **ARCH-V31-4 — `JoinHub<TLeft,TRight,TOut>` for multi-input streaming** (8–12 hours).
  - Source: Architect F7, Researcher F8.
  - Designed primitive for two-input indicators (Beta, Correlation, Prs). Configurable zip-by-timestamp vs combine-latest semantic. PairsProvider revert (PR #1821) failed because no primitive was designed first.

- [ ] **ARCH-V31-5 — `IObservable<T>` adapter for Rx interop** (3–4 hours).
  - Source: Researcher F1.
  - Add `ToObservable()` extension on each hub. Does not retrofit Rx as the base — the `OnRebuild` semantic is genuinely outside Rx's model. Just an adapter.

- [ ] **ARCH-V31-6 — `IAsyncEnumerable<T>` and `Channels` adapters** (3–4 hours).
  - Source: Researcher F2.
  - `StreamHubExtensions.ConsumeAsync(IAsyncEnumerable<TIn>, CancellationToken)` and `Results.ToAsyncEnumerable()`. Lets hubs interoperate with SignalR / gRPC server-streaming / Kafka consumers without boilerplate.

- [ ] **ARCH-V31-7 — Standardize shared "increment kernels" usage** (umbrella; ~20–30 hours across sub-items).
  - Source: Inspector F1, verified observation. Scoping research 2026-05-26.
  - Seven indicators ship kernels today (`Ema`, `Sma`, `Tr`, `Atr`, `Adl`, `Obv`, `Dynamic` — see `src/*/*/*.Utilities.cs`). Adoption is **39 of 158 streaming files (~25%)**: 29 of 79 StreamHubs vs only 10 of 79 BufferLists — the BufferList side carries the duplication tax. Target ≥80% adoption across both surfaces.
  - Tier 4 (kernel-incompatible) covers ~25–30 files: DSP/Hilbert state (`Mama`, `FisherTransform`, `HtTrendline`), adaptive K (`Kama`), brick/reversal (`ParabolicSar`, `Renko`, `VolatilityStop`), argmax/range (`Aroon`, `Donchian`, `Fractal`, `PivotPoints`, `RollingPivots`, `Pivots`, `Fcb`), pure OHLC transforms (`HeikinAshi`, `Doji`, `Marubozu`), session-bound (`Vwap`), multi-window offsets (`Ichimoku`). These are out of scope for kernel migration and should be excluded when measuring the ≥80% target (denominator becomes ~128 files).
  - **Design tensions to settle before the first migration PR lands** (so they aren't re-discovered per sub-item):
    - `Sma.Increment` today is StreamHub-shaped (`IReadOnlyList<IReusable>` + `endIndex`). BufferLists carry `Queue<double>`. Pragmatic resolution: add a `Queue<double>` overload; defer `ReadOnlySpan<double>` unification.
    - **Do not** introduce composite per-indicator kernels (`Macd.Increment`, `Tema.Increment`). The per-layer `Ema.Increment` chain is correct and reusable; composite kernels would re-encode duplication.
    - **Wilder smoothing ≠ EMA semantically** (K = `1/N` vs `2/(N+1)`). Keep a separate `Wilder.Increment` kernel rather than overloading `Ema.Increment`; document the distinction in xmldoc.
    - Cumulative kernels (`Adl`, `Obv`) return rich result objects. Acceptable for these but **do not generalize** — `Ema.Increment` returning `double` is the better blueprint.

- [ ] **ARCH-V31-7a — `Sma.Increment(Queue<double>)` overload + BufferList sweep** (4–6 hours).
  - Add a `Queue<double>` overload to `Sma.Utilities.cs` (formalize the existing `Average(Queue<double>)` extension as `Increment` to align naming with other kernels). Migrate ~12–15 BufferLists currently inlining `sum / count` over their queue: Tema, Trix, T3, MaEnvelopes, BollingerBands, StdDev, Cmf, Cci, Mfi, Smma SMA-seed, ConnorsRsi, others. Math algebraically identical; pin with `IsExactly` against series oracle.
  - Highest single-PR adoption swing on the underserved BufferList side (10/79 → ~24/79).

- [ ] **ARCH-V31-7b — Inline `_lastEma + K*(x - _lastEma)` → `Ema.Increment` sweep** (3–4 hours).
  - Replace inline EMA arithmetic in: `Tema.BufferList.cs:89-91`, `Trix.BufferList.cs:91-93`, `T3.BufferList` (6 layers), `Dema.BufferList` (verify already migrated), plus two missed StreamHub sites at `Pmo.StreamHub.cs:86,113`. Pure cleanup, no behavior change.

- [ ] **ARCH-V31-7c — Extract `Roc.Increment(prev, curr)` kernel + migrate call sites** (2–3 hours).
  - Add `Roc.Increment` to `src/m-r/Roc/Roc.Utilities.cs` returning percentage rate-of-change. Migrate Roc, RocWb, Pmo, Pvo, Macd inline ROC math (~6 sites).

- [ ] **ARCH-V31-7d — Extract `Wilder.Increment(lookback, last, new)` kernel** (6–8 hours).
  - New shared kernel for Wilder smoothing (K = `1/N`). Refactor `Atr.Increment` to delegate. Migrate `Rsi.StreamHub` and `Smma.StreamHub`/`BufferList`. Pin with `IsExactly` bit-equality across three indicator families — medium risk because Wilder K differs from EMA K.

- [ ] **ARCH-V31-7e — Extract `StdDev.Increment(Queue<double>)` kernel** (4–6 hours).
  - Running-variance kernel for `Queue<double>`. Migrate `StdDev.BufferList`, `BollingerBands.BufferList`, `Keltner.BufferList` stddev arms. Explicitly pin population-vs-sample variance choice via test.

- [ ] **ARCH-V31-7f — Extract `Wma.Increment` kernel (v3.2 candidate)** (4–6 hours).
  - Weighted-window kernel for Wma, Hma (composes WMAs), Alma. Lower priority than 7a–7e because adoption surface is smaller (~6 sites). Mark for v3.2 if v3.1 ships before this lands.

- [ ] **ARCH-V31-8 — Shared rolling-window primitives** (6–8 hours).
  - Source: Inspector F2.
  - `RollingMin`, `RollingMax`, `RollingSum`, `RegressionAccumulator` as shared utilities. Today indicators like Chandelier and Slope reinvent these per surface.

- [x] **ARCH-V31-9 — `Quote → Bar` rename — ✅ DONE in v3.0** (RG004 = YES; PR #1933, bundled in #2102). Superseded the conditional-on-NO framing below; the rename shipped in v3.0 rather than being deferred.
  - Source: PR #1014 comment 1 (2024-07-01). **Only lands here if §A RG004 defers the rename out of v3.0.**
  - If RG004 = YES, this entry is moved into v3.0 §B (or §A) as the implementation tracker, not v3.1+.
  - Scope: rename `Quote` → `Bar` and `IQuote` → `IBar`; `[Obsolete]` shims preserve `Quote`/`IQuote` for a deprecation window; refresh `docs/migration/v3.md`; reserve `TradeTick` exclusively for individual bid/ask trades (already aligned with `TradeTickHub` per PR #1875). v4.0 would be the next available window if this slips here.

- [ ] **ARCH-V31-10 — External hub cache storage** (8–12 hours). Tracks open [Issue #1884](https://github.com/facioquo/stock-indicators-dotnet/issues/1884).
  - Source: PR #1014 comment 5 (2024-07-01) + private project board entry.
  - Allow consumers to inject an external `List<TSeries>` cache for `StreamHub`/`BufferList` storage, enabling durable persistence and rehydration patterns (Redis, FusionCache, custom storage). Already prototyped via the `AbstractCache(List<TSeries> externalCache)` ctor stub in #1884; needs design + test scenarios for `IBar` and `IResult/IReusable` caches.

- [ ] **ARCH-V31-11 — Prune-stable rollback: framework state snapshot/restore** (large; the SR003 real fix — dedicated multi-PR effort, **do not bolt onto a hardening pass**). **⚠️ PENDING v3.0 DECISION (see §L)** — pull into v3.0 (engine bit-exact under every cache config) or ship the documented edge-config limitation.
  - Source: overall-review-v3 SR003, empirically confirmed and scope-corrected (PR #2055, `tests/Library/_common/StreamHub/StreamHub.RollbackAfterPrune.Tests.cs`).
  - **Problem.** Rebuild replays from an absolute `ProviderCache` index (base loop `StreamHub.cs:273-276`) and per-hub `RollbackState` re-derives state from `ProviderCache[0]` (e.g. `Adx.StreamHub.cs:228`). After a head-prune, `ProviderCache[0]` is no longer the true first bar, so recursive/cumulative seeds are rebuilt from a truncated suffix. Confirmed across **every** stateful hub — EMA and all derivatives (Macd/Tema/Dema/Trix/T3/…), the Wilder family (ATR/ADX/RSI/SMMA/AtrStop), and cumulative hubs (OBV/ADL) — not just the three originally named. Recursive smoothers drift transiently and re-converge; cumulative hubs (no warmup) carry a permanent offset with no leading nulls; and warmup/lookback indicators — window ones (SMA) plus recursive smoothers — lose their leading results to null where the retained cache no longer spans a full lookback (the Wilder hubs that re-derive their seed on every rollback — RSI/ADX/SMMA — additionally drift on *any* post-prune correction, not only deep ones). The rollback engine is **exact** when the cache retains the full history (positive control passes), so this is strictly a pruning interaction.
  - **Why it can't be a per-hub patch.** The boundary history a rebuild needs is gone by the time `OnPrune`/`PruneState` fires (the provider prunes its cache *before* notifying observers, `StreamHub.cs:432-434`). A correct fix must preserve each hub's recursive state at the prune boundary *before* the bars are lost.
  - **Design.** (a) Capture the pruned provider items at prune time — add a pre-prune hook, or pass the pruned items into a new `PruneState(DateTime, IReadOnlyList<TIn>)` overload, so a hub can fold them into a preserved boundary snapshot (do **not** change the public `IStreamObserver.OnPrune` signature). (b) Add `protected virtual SnapshotState()` / `RestoreState(...)` (or per-hub boundary fields) so `RollbackState` replays from the preserved boundary instead of `ProviderCache[0]`. (c) Adopt across all stateful hubs. (d) Pin with `IsExactly` oracle tests and **invert the divergence assertions in `StreamHub.RollbackAfterPrune.Tests.cs` to exact-equality** as the acceptance gate.
  - **Residual.** Corrections to bars already evicted below the retained head remain out of contract (handled by the SR001 before-head drop). Within-window and at-boundary corrections become exact once the boundary snapshot is preserved.
  - **Cost/risk.** Multi-day; touches the base prune/rollback machinery plus dozens of hubs; high regression surface against the full suite. Schedule as its own effort with a complete sweep.

- [ ] **ARCH-V31-12 — Guard mutation on subscribed (non-root) hubs** (2–3 hours).
  - Source: PR #2052 review (maintainer, 2026-05-31) — approved to spec as v3.1.
  - A hub already knows whether it is self-rooted (`BarHub._isStandalone`; non-root hubs hold a real `Provider`, not the inert `BaseProvider<T>`). Make the mutating surface — `Add`, `RemoveAt`, `RemoveRange`, `Reinitialize` — throw `InvalidOperationException` (or no-op under a documented contract) when called on a subscribed/chained hub, so a user cannot desync a leaf hub from its provider. Public-behavior change → land deliberately with tests; once enforced, tighten `stream.md` (which today only *documents* the "mutate the root hub" rule rather than enforcing it).
  - Companion (same root-only constraint): a convenience `Remove(IBar)` on the root `BarHub`/`TradeTickHub` (find-by-timestamp → `RemoveAt`/`RemoveRange`; no-op-or-throw if absent), from the §E SR011 doc discussion. Decide whether to ship the two together.

### Streaming confidence-review follow-ups (overall-review-v3, 2026-05-29)

Non-blocking items from the same swarm review; the stable-blocking subset is in §E. Cross-referenced to existing items where they overlap.

- [ ] **SR009 — Amortize `PruneCache` / `PruneList` O(MaxCacheSize)-per-tick shift** (4–6 hours). `src/_common/StreamHub/StreamHub.cs:432` and `src/_common/BufferLists/BufferList.cs:116` left-shift a `List<T>` on every tick once the cap is reached. Benchmark-confirmed *flat* (constant per tick, not growing with feed length), but ~800 KB memmove/tick at the 100k default. Amortize (batch-prune on overshoot) or back the results store with a ring buffer (generalize the existing `CircularDoubleBuffer` to `TOut`); one base-class change covers all 79 BufferLists. Overlaps ARCH-V31-2 cache-routing.
- [ ] **SR010 — Cache the observer-notify snapshot** (2 hours). `src/_common/StreamHub/StreamHub.Observable.cs:147` allocates `_observers.ToArray()` on every notified tick (measured 104 B/hop; 192 B/add in a 2-hop chain). Snapshot only on subscribe/unsubscribe.
- [x] **SR004c / SR005c — Isolate observer callbacks + error fan-out** (3–4 hours). Code half of §E SR004/SR005: wrap each `o.OnAdd`/`OnRebuild`/`OnPrune`/`OnError` in try/catch (Rx-style), route the failure to that observer's `OnError`, and continue notifying siblings. *(Pulled forward to v3.0 — PR #2056. Implemented as isolate + route-to-`OnError` + continue; quarantine/auto-unsubscribe intentionally NOT done per maintainer (isolate-and-continue chosen). If a self-healing quarantine of a persistently-throwing observer is later desired, that's a separate v3.1+ follow-up.)*
- [x] **SR007 — Guard the `_observers` HashSet against Subscribe-during-stream races** (2–3 hours). `src/_common/StreamHub/StreamHub.Observable.cs:40,51,95` mutate `_observers` unsynchronized vs the notify-loop enumeration; constructing a downstream hub concurrent with `Add` is a real (out-of-contract) race. Guard `_observers` or formally document Subscribe as a serialized setup-phase action. Pairs with TC-V31-1. *(PR #2069 — see §L)*
- [ ] **SR008e — Enforce the BufferList chronological-input precondition** (maintainer-gated; 2–4 hours). Code half of §E SR008 (documented + pinned in PR #2054). A guard in `BufferList.AddInternal` (`item.Timestamp < _internalList[^1].Timestamp` → throw) is viable: an experiment broke **exactly one** indicators-project test (`Pmo.CustomBuffer_OverMaxListSize_AutoAdjustsListAndBuffers`, which itself re-feeds `Bars.TakeLast(50)` out of order). Before adopting: confirm blast radius across the integration + public-api test projects and consumer impact (a new exception across all 79 BufferLists is a breaking change at stable), decide whether duplicates (`==`) are also rejected, settle the exception type/message, and update `BufferList.OrderingContract.Tests.cs` (which currently pins the *unenforced* behavior) plus the Pmo test.
- [x] **SR006c — Close the `Reinitialize` rebuild→subscribe race** (part of T205 / `src/_common/StreamHub/StreamHub.cs:233`). Code half of §E SR006: subscribe-before/atomic-with rebuild, or re-run a catch-up rebuild after Subscribe returns. *(PR #2069 — see §L)*
- [ ] **SR017 — Chain lifecycle ergonomics** (3–4 hours). No `Dispose`/`DisposeChain` on `StreamHub` (it does not implement `IDisposable`); teardown is per-hub via `EndTransmission`/`Unsubscribe` and undiscoverable. Document the `EndTransmission`→`OnCompleted` teardown sequence in `stream.md` and consider implementing `IDisposable`. Surface the `Reinitialize` caveat in xmldoc.
- [x] **SR020 — Deep-chain rebuild value-equality tests** (2 hours). `tests/Library/_common/StreamHub/StreamHub.BoundsChecking.Tests.cs:810` asserts count/timestamp only after a chained `RemoveAt`; extend to `IsExactly` vs a Series-equivalent chain. StochRsi already pins one 3-deep chain and the empirical harness found no divergence — low priority. *(PR #2071 — see §L)*
- [x] **SR019 — Aggregator rollback-equivalence coverage** — already tracked as **TC-V31-4**; the review reconfirms the gap (aggregator hubs excluded from the generic contract at `tests/Library/_common/StreamHub/StreamHub.RollbackContract.Tests.cs:16`). *(PR #2071 — see §L)*
- Minor cleanups (Low; batch when next touching the files): `OverflowCount` not reset alongside `LastItem` in `RemoveRange` (replay self-heals; `StreamHub.cs:190`); standalone same-timestamp replacement leaves base `LastItem` stale → one extra rebuild cascade on an idempotent re-send (`Bar.StreamHub.cs:135`); `RemoveRange(DateTime)` is the lone cache mutator with no `CacheLock` (defense-in-depth under the single-writer contract; `StreamHub.cs:175`); `_isRebuilding` is a non-volatile instance bool (add an "only read/written under CacheLock" invariant comment; `:23`); `Add(IEnumerable)` `OrderBy`-allocates + locks per item and `RemoveRange` allocates a delegate per rollback (micro-costs); `stream.md:128` "treat as thread-safe (… RemoveRange …)" wording over-promises versus the documented single-writer model.

### Test coverage v3.1+

- [x] **TC-V31-1 — Concurrency test suite for StreamHub** (4–6 hours). *(PR #2070 — see §L)*
  - Source: Tester F4. Production hardening (PR #1927) shipped; tests are the safety net for future regressions. Parallel `Add` from N threads, interleaved late-arrivals, concurrent observer subscribe/dispose.

- [ ] **TC-V31-2 — Tighten `Catalog.Listings.Tests.cs` per-indicator metadata assertions** (3–4 hours).
  - Source: Tester F7. Loose `NotBeEmpty`/`NotBeNull` should become exact `Results.Count`/`Parameters.Count` per indicator.

- [ ] **TC-V31-3 — BufferList `MaxListSize` runtime trimming test** (1–2 hours).
  - Source: Tester F8. Mirror TradeTickHub's `WithCachePruning` pattern for BufferList.

- [x] **TC-V31-4 — Aggregator hub rollback-equivalence coverage** (2 hours). *(PR #2071 — see §L)*
  - Source: TC001 follow-up. `BarAggregatorHub` and `TradeTickAggregatorHub` override `RollbackState` but are not in the catalog, so TC001 does not exercise them. Decide whether to catalog-register them or add two hand-built rollback-equivalence cases to `StreamHub.RollbackContract.Tests.cs`.

- [ ] **TC-V31-5 — Compound-hub inner↔outer rollback interaction** (3 hours).
  - Source: TC001 follow-up. For compound hubs (StochRsi, Stc, ConnorsRsi, Gator, etc.) `Rebuild` on the outer hub does not exercise the inner hub's `RollbackState`. Inner is still validated via its own listing's TC001 row; the cross-hub interaction is not. Add a targeted test only if a real compound-hub rollback bug surfaces.

- [ ] **TC-V31-6 — Aggregator gap-fill / missing-candle test variants** (2–3 hours).
  - Source: Discussion #1018 @elAndyG. Companion to T236 (`GapFillMode` enum) and G008 (docs).
  - Add tests under `tests/Library/_common/Bars/Bar.AggregatorHub.Tests.cs`: synthesized tick sequences with intentional gaps (e.g., missing 1-minute bars during low-volume periods); assert behavior under each future `GapFillMode` value (None/ForwardFill/Interpolate). Today the test gap is in §D TC005 (boundary tests) — TC-V31-6 extends that pattern once T236 ships. Distinct from TC-V31-4 (which covers rollback equivalence on the aggregator hubs themselves).

- [x] **TC-V31-7 — BufferList parity for bounded-value invariant tests** (1–2 hours). *(PR #2072 — see §L)*
  - Source: PR #2021 scope decision. The bounded-value invariant work (RSI, Stoch, Aroon, MFI, Ultimate, ConnorsRSI, WilliamsR) shipped Series + StreamHub coverage but skipped BufferList. The math is identical across all three styles, so a BufferList violation would be a regression bug rather than an algorithmic edge case; still, a symmetry pass closes the contract.
  - Add `Boundary_WithRandomBars_StaysWithinBounds` to each `*.BufferList.Tests.cs` sibling using `Data.GetRandom(2500)`, matching the Series and StreamHub pattern.

- [x] **TC-V31-8 — Chained-downstream late-arrival aggregator coverage** (1–2 hours). *(PR #2071 — see §L)*
  - Source: TC005 self-review (Tester finding). The new TC005 tests exercise the aggregator hub in isolation. A `BarHub → AggregatorHub → EmaHub` (and tick analog) late-arrival test would additionally pin that the aggregator's upstream-triggered rebuild propagates downstream observer notifications correctly, catching a hypothetical regression where the rebuild silently dropped one notification per replay. Inline test per pattern; assert downstream `EmaHub.Results` bit-equality between late and fresh chains.

- [ ] **TC-V31-9 — Aggregator late-arrival × gap-fill interaction** (1–2 hours).
  - Source: TC005 self-review (Tester observation). Late bar arriving into a bucket that was previously gap-filled should replace the gap bar with a real one rather than leave or duplicate it. Add `LateArrival_IntoGapFilledBucket_ReplacesGapBar` to both aggregator test files.

Random-seed determinism (raised in PR #2021 self-review) was considered and rejected. The boundary assertion is that the indicator stays within its documented range **regardless of the input** — pinning the seed only proves the bound holds on one specific dataset, which actively narrows the test's reach rather than strengthening it. Determinism would hurt wider testability here, not help it. Secondary point: `RandomGbm._random` is a shared static instance, so a seeded overload would not yield reproducible failures under parallel execution anyway.

### Numerical stability (PV001 follow-ups)

- [ ] **PV-V31-1 — Welford/Pébay sliding-window O(1) Slope variant** (4–6 hours).
  - Source: PV001 outcome. The naive four-sum identity (`Σy² − (Σy)²/n`) suffers catastrophic cancellation for stock-price-like inputs and fails the BufferList `IsExactly` contract. Numerically-stable sliding-window updates via Welford running mean + Pébay M2 deltas avoid the cancellation and may bit-match Series. Prototype against `Slope.BufferList` test suite; ship only if `.IsExactly(series)` holds.

- [ ] **PV-V31-2 — Single-pass Slope with running `sumY`** (2–3 hours).
  - Source: PV001 outcome. Less ambitious than PV-V31-1: eliminate the `_buffer.Average()` pre-pass while keeping the deviation foreach. Bit-equality is not guaranteed; measure drift, then decide whether to accept under a relaxed tolerance or revert.

- [ ] **PV-V31-3 — `IsApproximately<T>` test helper + BufferList tolerance audit** (8–12 hours).
  - Source: PV001 outcome. Cross-cutting decision to allow finance-grade approximate comparisons in BufferList tests where bit-equality is incompatible with O(1) numerical reformulations. Today `tests/Library/_tools/TestAssert.cs` exposes only `IsBetween` and `IsExactly`. Add an `IsApproximately(precision)` helper, audit all 79 BufferList test files, and decide per-indicator whether `IsExactly` is load-bearing or whether `IsApproximately(Money6)` is the correct contract. Unlocks naive O(1) identities (PV-V31-1) and similar cancellation-bound reformulations elsewhere.

### Framework / performance

- [ ] **T205 — `Reinitialize()` optimization** (6–8 hours) **[highest leverage]**.
  - File: `src/_common/StreamHub/StreamHub.cs:230` (TODO in source).

- [ ] **P001** — Moving Average family framework overhead (research). Acceptable for intended use (~40,000 bars/sec).
- [ ] **P003** — Alligator/Gator BufferList research (merged with P016).
- [ ] **P006** — Prs streaming support — depends on ARCH-V31-4 `JoinHub` design.

### Series batch processing optimizations

See [Issue #1259](https://github.com/facioquo/stock-indicators-dotnet/issues/1259). Series-side; not streaming.

- [ ] **S001** — Rolling SMA optimization for Series (2–5x for SMA-dependent indicators).
- [ ] **S002** — SMA warmup optimization in EMA family (10–30% for EMA, DEMA, TEMA, T3, MACD).
- [ ] **S004** — Span-based window operations (5–15%).
- [ ] **S005** — `RollingWindowMax/Min` array-based optimization (10–20% for Chandelier, Donchian, Stoch).
- ~~S003 — Array allocation~~ (ON HOLD; PR #1838 unmeasurable improvement).

### Streaming feature enhancements

- [ ] **T206** — `StreamHub.OnAdd` array return pattern (4–6 hours). Evaluate batch-emission need.
- [ ] **T210** — Pivots streaming rewrite (6–8 hours). Enhancement.
- [ ] **T214** — MaEnvelopes ALMA/EPMA/HMA support for StreamHub (8–12 hours).
- [ ] **T212** — Catalog `NotImplementedException` alternative (2–3 hours).
- [ ] **T220** — `StringOut` index range support (3–4 hours, test utility).
- [ ] **T221** — StreamHub stackoverflow test coverage expansion (ongoing).
- [ ] **T224** — Performance benchmark external data cache model (6–8 hours).
- [ ] **T225** — Style comparison benchmark representative indicators (2–3 hours).
- [ ] **T226** — `ISeries.UnixDate` property (3–4 hours, interface change).
- [ ] **T227** — `BarPart.Use` vs `ToBarPart` deprecation decision (1–2 hours).
- [ ] **T228** — `IBarPart` rename to `IBarPartHub` evaluation (2–3 hours).
- [ ] **T236** — `GapFillMode` enum for `Bar.AggregatorHub` (4–6 hours). Source: Discussion #1018 @elAndyG. Add `enum GapFillMode { None, ForwardFill, Interpolate }`; default `None` preserves current behavior. Companion to G008 (v3.0 docs) and TC-V31-6 (tests). Affects only the aggregator; downstream hubs see the post-fill bar stream.

### Cleanup deferred to v3.1+

- [ ] **T235 (was F4 from Stercorator)** — Schedule `Obsolete.V3.Indicators.cs` and `Obsolete.V3.Other.cs` removal with CHANGELOG entry. They use `error: true` shims (compile errors with helpful messages); zero runtime utility, just better error messages during migration. Sunset in v3.1 or v3.2 with documented removal milestone.

### Infrastructure & code quality

- [ ] **#1533** — Consistent test method naming conventions (~280 test classes).
- [ ] **#1739** — Doc site framework migration: VitePress migration largely complete on v3 (PRs #1981, #1991, #1992). Close once `main` carries the new site.

### Complex research (v3.2+)

- [ ] **T170 / #1692 / E001–E003** — ZigZag streaming. Pivot detection requires bounded lookahead vs append-only — fundamental tension.

- [ ] **RES-V32-1 — Watermark / grace-period for late data** (Researcher F6).
  - Source: Apache Flink and Kafka Streams watermarks. Today the library has two modes only: accept-and-rollback or reject. A `StreamHubSettings.LatenessGrace : int` (bars) would let high-frequency consumers reject arrivals older than `Cache.Last.Timestamp - Grace`. Default behavior unchanged.

- [ ] **RES-V32-2 — `RollbackContext` struct vs bare `int`** (Architect F2 follow-up).
  - After v3.1 doc clarification (DOC-ARCH-2 + ARCH-V31-1/2), evaluate enriching the signature with intent (`Reason: enum { LateArrival, ExplicitRemove, Rebuild }`) so subclasses can distinguish causes.

- [ ] **RES-V32-3 — Pine Script-style "rebuild current open bar only" fast path** (Researcher F5).
  - For same-timestamp updates, only recompute from the open bar's prefix; for older out-of-order, opt-in `AllowLateArrivals` with documented repaint cost.

- [ ] **#1323 / #1259** — Heap allocation reduction (struct Bar, ArrayPool, Span). Coordinated v3.2 push.

- [ ] **Review [Discussion #1018](https://github.com/facioquo/stock-indicators-dotnet/discussions/1018)** — community feedback on state rollback.

- [ ] **E004–E006** — BarHub intra-period update semantics.
- [ ] **E007–E008** — ADX DMI property expansion.
- [ ] **E009** — BufferList configuration ([Issue #1831](https://github.com/facioquo/stock-indicators-dotnet/issues/1831)).
- [ ] **E010** — Composite naming for chained indicators.

---

## Completed in the v3.0 cycle (appendix)

Retained for traceability. PR descriptions hold the change narrative; entries here are one-line pointers only. Items below are merged and verified as of 2026-05-26.

### Quality pass — guidance and cleanup

- [x] **RG005** — netstandard2.x drop decision documented; v2 branch path codified *(PR #2047)*.
- [x] **G001** — `_listings.Add(...)` registration order fixed in `indicator-catalog` SKILL *(PR #2025)*.
- [x] **G002** — `.specify/` reference removed from root `AGENTS.md` (verified done, no-op).
- [x] **G003** — Aggregator pattern + thread-safety documented in `indicator-stream` SKILL.
- [x] **G004** — `src/_common/README.md` regenerated to match real directory listing.
- [x] **G005** — Skills tree pruned to 10 tracked; root `AGENTS.md` skills index made exhaustive *(PR #2043)*.
- [x] **G006** — Performance target bands aligned with measured reality (verified done, no-op).
- [x] **G007** — Plan ↔ guidance cross-references confirmed via `AGENTS.md` (`tests/AGENTS.md` layout reconciled as G007-followup).
- [x] **G008** — `fillGaps` semantics documented on `BarAggregatorHub` / `TradeTickAggregatorHub` + `resize-bar-history.md`. Source: Discussion #1018 @elAndyG.
- [x] **T230** — `Stock.Indicators.sln.DotSettings.user` untrack verified (no-op).
- [x] **T232** — 7 legacy `ArgumentNullException` sites migrated to `ThrowIfNull`; CA1510 global suppression removed *(PR #2045)*.
- [x] **T233** — 14 `#pragma warning disable` directives audited; all intentional and load-bearing; DOC-ARCH-6 scope revised accordingly.
- [x] **T234** — 12 TODOs triaged; 3 obsolete removed, 9 retained as in-source intent pointers *(PR #2034, #2036)*.

### Quality pass — test coverage

- [x] **TC001** — Generic rollback-equivalence contract test across 79 catalog listings *(PR #2010)*. Out-of-scope gaps tracked as TC-V31-4 + TC-V31-5.
- [x] **TC002** — Per-indicator late-arrival tests for 11 multi-stage hubs *(PR #2019)*.
- [x] **TC003** — Bounded-value invariant test for RSI/Stoch/Aroon/MFI/Ultimate/ConnorsRSI/WilliamsR *(PR #2021)*. BufferList parity deferred to TC-V31-7.
- [x] **TC004** — Macd StreamHub coverage parity (verified, no new code; cascaded-state audit complete).
- [x] **TC005** — Bar/TradeTick aggregator boundary tests (late-arrival, partial-bucket, late-vs-fresh oracle) *(PR #2022)*. Latent duplicate-bar bug on mid-bucket late arrivals caught and fixed in the same PR via `Rebuild(DateTime)` override on both aggregator hubs.
- [x] **TC006** — Catalog count assertions sharpened to exact `Be(85)/Be(79)/Be(79)/Be(243)`; consolidates **T219** *(PR #2023)*.

### Quality pass — architecture documentation

- [x] **DOC-ARCH-1 + DOC-ARCH-7** — ADR 0001 (`docs/decisions/0001-dual-track-bufferlist-streamhub.md`) in MADR 4.0 format with four considered options, prior-art comparison (ta4j, TA-Lib, Tulip, Pine Script, QuantConnect/Lean), and Confirmation section. `docs/decisions/README.md` codifies the ADR convention; `srcExclude` keeps the folder out of the VitePress site *(PR #2044)*.
- [x] **DOC-ARCH-2** — `RollbackState(int)` xmldoc rewritten to a precise lifecycle contract. 55-override audit deferred to v3.1.
- [x] **DOC-ARCH-3** — `Results` live-view semantics documented on `IStreamObservable<T>.Results`; `Snapshot()` addition reserved for ARCH-V31-3.
- [x] **DOC-ARCH-4** — Boilerplate budget for new streamable indicators added to `src/AGENTS.md` (per-file LOC table calibrated to Ema baseline ~419 LOC across 7 files).
- [x] **DOC-ARCH-5** — Result type convention codified in `src/AGENTS.md` (positional `record`, `Timestamp` first, nullable warmup values, `[Serializable]`, `[JsonIgnore]` on `Value`, `.Null2NaN()`).
- [x] **DOC-ARCH-6** — `_common/` analyzer-suppression footprint stated in `src/_common/AGENTS.md`: exactly one intentional pragma (`IDE0010` at `_common/StreamHub/StreamHub.cs:2`); `_common/BufferLists/` carries zero *(PR #2041)*.

### Quality pass — numerical verification

- [x] **PV001** — Slope BufferList naive O(1) four-sum identity verified algebraically equivalent but precision-incompatible with `IsExactly` bit-equality (catastrophic cancellation on stock-price-like inputs) *(PR #2024)*. Slope.BufferList stays at deviation form; xmldoc on `SlopeList` and `SlopeHub` records the finding. Three follow-up paths promoted to v3.1+ items: PV-V31-1 (Welford/Pébay stable variant), PV-V31-2 (single-pass with running sumY), PV-V31-3 (`IsApproximately` test helper + BufferList tolerance audit).

### Quality pass — documentation

- [x] **D009** — BarPart streaming variants documented (`ToBarPartList` / `ToBarPartHub`); coverage 79 of 79.
- [x] **D010** — `docs/guide/custom-observers.md` with full `IStreamObserver<T>` contract and box-to-`IChainProvider<IReusable>` pattern. Source: Discussion #1018 @JGronholz, resolves Issue [#1895](https://github.com/facioquo/stock-indicators-dotnet/issues/1895) *(PR #2038)*.
- [x] **D011** — `docs/guide/testing.md` with canned-fixture recommendation and explicit rejection of per-type result interfaces (`IEmaResult` anti-pattern) *(PR #2039; page subsequently retired as no-longer-relevant in the final v3.0 docs-cleanup PR — see git history for the original guidance)*.

### Quality pass — BufferList performance decisions

- [x] **P015** — Slope BufferList ships v3.0 at the current ~3.41x ratio per PV001; v3.1 candidates tracked as PV-V31-1/2/3.
- [x] **P016** — Alligator BufferList ships v3.0 at the current 2.16x ratio; research merged with P003 for v3.1+.
- [x] **P017** — Adx BufferList ships v3.0 at the current 2.08x ratio; research merged with v3.1+ bundle.

### Quality pass — low-priority test items

- [x] **T216** — ConnorsRsi `RemoveWarmupPeriods` doc/code reconciliation; doc now states `MAX(R,S,P)+1` matching the implementation *(PR #2046)*.
- [x] **T217** — CMO zero-price-change tests added: `FlatPrices_AcrossEntireWindow_ReturnsZero` and `FlatThenMoving_ReturnsValuesOnceRealMovementEnters` *(PR #2040)*.

### Quality pass — release mechanics docs

- [x] **K008** — v2-branch netstandard2.x maintenance role canonicalized in the branching-strategy plan (since retired); README reciprocal pointer added *(PR #2047)*.

### Correctness & framework

- **#1585** — BarHub self-healing limitation: cache exposures wrapped in `AsReadOnly()` (note: this prevents *mutation* but exposes a live view — see DOC-ARCH-3 above)
- **T200** — TEMA/DEMA StreamHub layered EMA state optimization
- **T201** — Stochastic SMMA re-initialization on NaN (PR #1852)
- **T202** — WilliamsR boundary rounding precision (boundary clamping + tests)
- **T204** — StochRsi `Remove()` auto-healing evaluation (PR #1842)
- **T207** — Removed redundant `RemoveWarmupPeriods` overrides for Epma, Hurst, Mfi, Stoch, Vwap (PR #1842)
- **T208** — `Bar.Date` property deprecation shipped; full removal deferred to v4+ (`[Obsolete]` shim at `src/_common/Bars/Bar.cs:41-46`)
- **T209** — PivotPoints `ToList()` removal (PR #1842)
- **T211** — `ListingExecutor` simplified to use `IBar` interface (PR #1842)
- **T215** — Hurst Anis-Lloyd corrected R/S implementation (`HurstResult.HurstExponentAL` alongside raw `HurstExponent` across Series/BufferList/StreamHub) (PR #2007, #1636, #1643)
- **T218** — Precision analysis test obsolescence review (clarified value of `BoundaryTests`)
- **T222** — StreamHub cache management exact-value verification (Series parity is canonical)
- **T223** — Renko StreamHub alternative testing approach via `ITestBarObserver` / `ITestChainProvider`
- **T229** — ATR utilities incremental method made public
- **P002** — Slope BufferList research closed via PV001 *(PR #2024)*; future work options enumerated as PV-V31-1/2/3.

### Performance — StreamHub

All items implemented in source; baselines refreshed (RG001 complete).

- **P004** — ForceIndex StreamHub O(n²) → O(1) incremental update with rolling sum (PR #1860)
- **P005** — Slope StreamHub: cached slope/intercept state (PR #1859); 43% overhead reduction
- **P007** — Roc StreamHub: investigation closed; inherent framework cost
- **P008** — PivotPoints StreamHub: investigation closed; within acceptable range
- **P009** — Gator StreamHub: AlligatorHub SMMA state caching + `RollbackState` (PR #1986)
- **P010** — Ultimate StreamHub: sliding-window queues (PR #1977)
- **P011** — Adl StreamHub: rolling-total state management (PR #1978 centralized signature)
- **P012** — Pmo StreamHub: layered EMA state O(1) updates (PR #1979)
- **P013** — Smi StreamHub: `RollingWindowMax/Min` deques replaced with fixed-size circular arrays
- **P014** — Chandelier StreamHub: refactored to `ChainHub<IBar, …>` (PR #1988)

### New streaming features

- **Aggregator hubs for bar/tick quantization** (PR #1875) — `BarAggregatorHub`, `TradeTickHub`, `TradeTick.AggregatorHub`. Net-new streaming capability.

### Documentation & site

- **D007** — Migration guide updates: `docs/migration/v3.md` has full streaming section
- **D008** — SmaAnalysis and Tr indicator doc pages created (PR #1989)
- **PRs #1981, #1991, #1992** — Website 3-pillar IA reorganization; VitePress with Vue chart components
- **PRs #1976, #2005** — Streaming plan updates and missing BufferList/StreamHub doc sections for Alligator, AtrStop, Tsi
- **T213** — Performance documentation consolidated under `tools/performance/benchmarking.md` and `tools/performance/baselines/README.md`
