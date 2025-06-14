```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
12th Gen Intel Core i7-1255U 2.60GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.301
  [Host]   : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method            | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0      | Gen1     | Allocated  | Alloc Ratio |
|------------------ |-----------:|------------:|----------:|------:|--------:|----------:|---------:|-----------:|------------:|
| NaiveDp           | 8,476.4 μs | 1,473.04 μs |  80.74 μs |  1.00 |    0.01 | 1593.7500 | 203.1250 | 10012118 B |       1.000 |
| FuzzySharpClassic | 5,263.1 μs |   851.79 μs |  46.69 μs |  0.62 |    0.01 |   46.8750 |        - |   300051 B |       0.030 |
| Fastenshtein      | 4,255.6 μs |   169.65 μs |   9.30 μs |  0.50 |    0.00 |         - |        - |     7067 B |       0.001 |
| Quickenshtein     | 1,674.9 μs | 3,872.17 μs | 212.25 μs |  0.20 |    0.02 |         - |        - |        2 B |       0.000 |
| FuzzySharp        |   463.1 μs |    64.00 μs |   3.51 μs |  0.05 |    0.00 |         - |        - |     3041 B |       0.000 |
