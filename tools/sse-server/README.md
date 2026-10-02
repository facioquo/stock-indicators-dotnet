# SSE server

`Test.SseServer` is an ASP.NET Core app that streams bars as Server-Sent Events. The [simulation tool](../simulate/README.md) and the SSE thread-safety tests in `tests/Integration` start it for you; run it directly to stream by hand.

```bash
dotnet run --project tools/sse-server -- --urls http://localhost:5001
```

The VS Code task `Run: SSE Server` runs the same command.

## Endpoints

| Endpoint | Streams |
| -------- | ------- |
| `/bars/random` | Random bars, continuously until `batchSize` is reached or the client disconnects |
| `/bars/longest` | The longest bundled bar dataset, deterministically |
| `/openapi/v1.json` | The OpenAPI document (Development environment only) |

Both bar endpoints take these query parameters:

| Parameter | Type | Default | Description |
| --------- | ---- | ------- | ----------- |
| `interval` | _`int`_ | `100` | Delay between bars, in milliseconds |
| `batchSize` | _`int`_ | none | Number of bars to send before closing the stream; unlimited for `/bars/random`, the whole dataset for `/bars/longest` |
| `barIntervalCode` | _`string`_ | `1m` | Timestamp spacing between bars, such as `1s`, `5m`, `1h`, or `1d` |

`/bars/longest` also takes `scenario` (`stc-rollbacks` or `allhubs-rollbacks`), which the integration tests use to send rollback events after the bars.

Because `barIntervalCode` sets timestamp spacing independently of `interval`, the stream compresses time: `barIntervalCode=1h&interval=100` delivers 24 hours of hourly bars in 2.4 seconds.

Example:

```text
http://localhost:5001/bars/random?interval=100&batchSize=1000&barIntervalCode=1h
```

## Stopping

Press `Ctrl+C` in the terminal running the server. To stop a host that does not exit, run the `Stop: SseServer hosts` VS Code task or its script:

```bash
bash tools/scripts/stop-sseserver.sh
```
