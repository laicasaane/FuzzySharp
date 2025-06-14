```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method            | Mean       | Error       | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
|------------------ |-----------:|------------:|---------:|------:|--------:|---------:|-------:|----------:|------------:|
| NaiveDp           | 2,433.8 μs | 1,410.50 μs | 77.31 μs |  1.00 |    0.04 | 371.0938 | 7.8125 | 2335170 B |       1.000 |
| FuzzySharpClassic | 1,476.4 μs | 1,653.94 μs | 90.66 μs |  0.61 |    0.04 |  23.4375 |      - |  149793 B |       0.064 |
| Fastenshtein      | 1,057.4 μs |   174.89 μs |  9.59 μs |  0.43 |    0.01 |        - |      - |    3729 B |       0.002 |
| Quickenshtein     |   647.5 μs |   782.37 μs | 42.88 μs |  0.27 |    0.02 |        - |      - |       1 B |       0.000 |
| FuzzySharp        |   172.3 μs |    69.35 μs |  3.80 μs |  0.07 |    0.00 |   0.2441 |      - |    3040 B |       0.001 |
