---
title: Stream hub warm start
description: Capture a stream hub's state, persist it, and resume streaming later without replaying history
---

# Warm start

A restarted process normally rebuilds each hub by adding the full bar history again. A snapshot holds what a hub needs to resume instead: its results, its internal state, and how far it has pruned. Capture snapshots before you stop, restore them into new hubs, and keep adding live bars. The results match a chain that never stopped. For the basics, start with [Stream hubs](/guide/styles/stream).

::: warning Preview coverage
Only some hubs support snapshots so far: `BarHub`, `AdxHub`, `EmaHub`, `KvoHub`, `ParabolicSarHub`, `RsiHub`, and `SuperTrendHub`. Every other hub throws `NotSupportedException` from `CaptureSnapshot()` and `RestoreSnapshot()`; rebuild those from history as before.
:::

## Capture and restore

```csharp
// before stopping
HubSnapshot<IBar> barSnapshot = barHub.CaptureSnapshot();
HubSnapshot<EmaResult> emaSnapshot = emaHub.CaptureSnapshot();

// after restarting: build the same chain, then restore from the root hub outward
BarHub barHub = new();
EmaHub emaHub = barHub.ToEmaHub(20);

barHub.RestoreSnapshot(barSnapshot);
emaHub.RestoreSnapshot(emaSnapshot);

// keep streaming
barHub.Add(liveBar);
```

`CaptureSnapshot()` copies the hub under its lock, so the snapshot does not change as the hub keeps running. Capture every hub in the chain between the same two `Add` calls, so they describe the same bars.

## Restore rules

- **Root hub first.** Restore the `BarHub`, then each hub subscribed to it, then theirs. A subscribed hub checks that its results line up with its provider's bars and throws `InvalidDataException` when they do not.
- **Same hub, same settings.** A snapshot restores only into a hub with the same name, such as `EMA(20)`. A snapshot from `EMA(20)` into `EMA(50)` throws `InvalidDataException`.
- **Fresh hubs.** Restore into newly created hubs. A hub that already pruned further than the snapshot keeps its own later boundary and refuses bars the snapshot's hub accepted.
- **Subscribers are not notified.** A restore replaces the hub's cache silently. A subscribed hub that does not support snapshots keeps its old results; call its `Rebuild()` after its provider is restored.

## Persist a snapshot

`HubSnapshot<T>` is a record, so `System.Text.Json` can save and load it. Treat its `State` as opaque bytes.

```csharp
string json = JsonSerializer.Serialize(emaHub.CaptureSnapshot());

// later
HubSnapshot<EmaResult> emaSnapshot = JsonSerializer.Deserialize<HubSnapshot<EmaResult>>(json);
emaHub.RestoreSnapshot(emaSnapshot);
```

A `BarHub` snapshot holds its bars as `IBar`, which a serializer cannot create. Load it as a concrete bar type, then rewrap it:

```csharp
HubSnapshot<Bar> saved = JsonSerializer.Deserialize<HubSnapshot<Bar>>(json);

barHub.RestoreSnapshot(new HubSnapshot<IBar>(
    saved.FormatVersion, saved.HubName, saved.PrunedThrough, saved.Results, saved.State));
```

## Compatibility

A snapshot carries two versions: `FormatVersion` for the snapshot itself, and a version inside each hub's `State`. A library update may change either one when a hub's internal state changes.

- A restore rejects any version it does not support, by throwing `InvalidDataException`, and leaves the hub as it was.
- A restore never converts an older snapshot to a newer version.

Keep the source bars you need to rebuild, and treat a rejected snapshot as a cold start: add the history again, as you would without one.

## See also

- [Stream hubs](/guide/styles/stream) for the basics
- [Corrections and memory](/guide/styles/stream/corrections) for cache sizing, pruning, and `Rebuild()`
