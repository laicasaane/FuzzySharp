```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method            | Mean       | Error      | StdDev     | Ratio | RatioSD | Gen0       | Gen1       | Allocated   | Alloc Ratio |
|------------------ |-----------:|-----------:|-----------:|------:|--------:|-----------:|-----------:|------------:|------------:|
| NaiveDp           | 278.116 ms | 341.771 ms | 18.7336 ms |  1.00 |    0.08 | 43500.0000 | 34500.0000 | 275312920 B |       1.000 |
| FuzzySharpClassic | 171.440 ms | 258.661 ms | 14.1781 ms |  0.62 |    0.06 |          - |          - |   1545732 B |       0.006 |
| Fastenshtein      | 149.119 ms | 122.272 ms |  6.7022 ms |  0.54 |    0.04 |          - |          - |     34028 B |       0.000 |
| Quickenshtein     |  17.234 ms |  38.280 ms |  2.0982 ms |  0.06 |    0.01 |          - |          - |        23 B |       0.000 |
| FuzzySharp        |   5.638 ms |   1.617 ms |  0.0886 ms |  0.02 |    0.00 |          - |          - |      3051 B |       0.000 |
