```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8737/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i9-13900H 2.60GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.301
  [Host]   : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  ShortRun : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method              | Mean           | Error          | StdDev        |
|-------------------- |---------------:|---------------:|--------------:|
| **Aggregate**           |   **100,821.5 ns** |    **17,281.4 ns** |     **947.25 ns** |
| **RemoveWarmupPeriods** |    **20,960.4 ns** |     **3,960.3 ns** |     **217.08 ns** |
| **ToCandleResults**     |    **15,263.0 ns** |     **2,169.8 ns** |     **118.94 ns** |
| **ToListBarD**          |    **10,327.7 ns** |     **6,600.4 ns** |     **361.79 ns** |
| **ToReusableClose**     |    **19,410.7 ns** |     **8,985.4 ns** |     **492.52 ns** |
| **ToReusableOhlc4**     |    **27,500.7 ns** |     **1,561.6 ns** |      **85.60 ns** |
| **ToSortedList**        |     **7,269.1 ns** |     **1,428.4 ns** |      **78.29 ns** |
| **ToStringOutList**     |   **556,312.6 ns** | **1,424,055.1 ns** |  **78,057.27 ns** |
| **ToStringOutType**     | **6,020,280.2 ns** | **4,838,406.3 ns** | **265,209.38 ns** |
| **Validate**            |       **680.7 ns** |       **251.0 ns** |      **13.76 ns** |
