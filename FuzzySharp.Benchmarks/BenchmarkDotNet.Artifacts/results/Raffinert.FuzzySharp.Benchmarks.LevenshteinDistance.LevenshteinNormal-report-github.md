```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method            | Mean       | Error      | StdDev    | Ratio | RatioSD | Gen0      | Gen1     | Allocated  | Alloc Ratio |
|------------------ |-----------:|-----------:|----------:|------:|--------:|----------:|---------:|-----------:|------------:|
| NaiveDp           | 9,993.4 μs | 5,940.7 μs | 325.63 μs |  1.00 |    0.04 | 1593.7500 | 203.1250 | 10012118 B |       1.000 |
| FuzzySharpClassic | 7,035.7 μs | 7,649.4 μs | 419.29 μs |  0.70 |    0.04 |   46.8750 |        - |   300051 B |       0.030 |
| Fastenshtein      | 5,608.7 μs | 8,232.5 μs | 451.25 μs |  0.56 |    0.04 |         - |        - |     7065 B |       0.001 |
| Quickenshtein     | 1,773.3 μs |   135.2 μs |   7.41 μs |  0.18 |    0.01 |         - |        - |        2 B |       0.000 |
| FuzzySharp        |   566.3 μs |   389.3 μs |  21.34 μs |  0.06 |    0.00 |         - |        - |     3041 B |       0.000 |
