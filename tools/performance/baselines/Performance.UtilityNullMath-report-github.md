```

BenchmarkDotNet v0.15.8, Windows 11 (10.0.26200.8737/25H2/2025Update/HudsonValley2)
13th Gen Intel Core i9-13900H 2.60GHz, 1 CPU, 20 logical and 14 physical cores
.NET SDK 10.0.301
  [Host]     : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3
  Job-YBWYST : .NET 10.0.9 (10.0.9, 10.0.926.27113), X64 RyuJIT x86-64-v3


```
| Method         | Mean      | Error     | StdDev    |
|--------------- |----------:|----------:|----------:|
| **AbsDblNul**      | **0.0087 ns** | **0.0072 ns** | **0.0048 ns** |
| **AbsDblVal**      | **0.0031 ns** | **0.0020 ns** | **0.0013 ns** |
| **NaN2NullDblNul** | **0.0105 ns** | **0.0062 ns** | **0.0041 ns** |
| **NaN2NullDblVal** | **0.0397 ns** | **0.0035 ns** | **0.0019 ns** |
| **NaN2NullNanNul** | **0.0042 ns** | **0.0017 ns** | **0.0011 ns** |
| **NaN2NullNaNVal** | **0.0045 ns** | **0.0034 ns** | **0.0023 ns** |
| **Null2NaNDblNul** | **0.0004 ns** | **0.0008 ns** | **0.0005 ns** |
| **Null2NaNDblVal** | **0.0008 ns** | **0.0021 ns** | **0.0014 ns** |
| **Null2NaNDecNul** | **0.0035 ns** | **0.0040 ns** | **0.0024 ns** |
| **Null2NaNDecVal** | **0.0053 ns** | **0.0049 ns** | **0.0033 ns** |
