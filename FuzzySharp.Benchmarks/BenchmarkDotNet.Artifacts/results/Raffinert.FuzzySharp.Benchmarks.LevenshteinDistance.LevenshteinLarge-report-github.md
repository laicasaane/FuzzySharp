```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
12th Gen Intel Core i7-1255U 2.60GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.301
  [Host]   : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method            | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0       | Gen1       | Allocated   | Alloc Ratio |
|------------------ |-----------:|------------:|----------:|------:|--------:|-----------:|-----------:|------------:|------------:|
| NaiveDp           | 243.037 ms |  45.1515 ms | 2.4749 ms |  1.00 |    0.01 | 43666.6667 | 35333.3333 | 275312853 B |       1.000 |
| FuzzySharpClassic | 149.659 ms |  10.5321 ms | 0.5773 ms |  0.62 |    0.01 |          - |          - |   1545732 B |       0.006 |
| Fastenshtein      | 137.856 ms | 121.5120 ms | 6.6605 ms |  0.57 |    0.02 |          - |          - |     34028 B |       0.000 |
| Quickenshtein     |  13.651 ms |   1.8536 ms | 0.1016 ms |  0.06 |    0.00 |          - |          - |        12 B |       0.000 |
| FuzzySharp        |   5.478 ms |   0.4256 ms | 0.0233 ms |  0.02 |    0.00 |          - |          - |      3051 B |       0.000 |
