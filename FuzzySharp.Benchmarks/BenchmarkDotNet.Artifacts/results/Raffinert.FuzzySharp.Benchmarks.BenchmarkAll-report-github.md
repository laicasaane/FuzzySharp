```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                               | Mean         | Error        | StdDev       | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|-------------:|-------------:|-------:|-------:|----------:|
| Ratio1                               |    291.49 ns |     50.20 ns |     2.752 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    235.27 ns |     28.78 ns |     1.577 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,318.14 ns |  2,833.91 ns |   155.336 ns | 1.9073 | 0.0038 |   11968 B |
| TokenSortRatio                       |    799.39 ns |    153.52 ns |     8.415 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  6,587.04 ns | 21,368.14 ns | 1,171.260 ns | 3.0975 | 0.0153 |   19456 B |
| TokenSetRatio                        |  1,574.43 ns |  2,916.06 ns |   159.839 ns | 0.8087 |      - |    5080 B |
| PartialTokenSetRatio                 |  5,269.14 ns |  2,611.82 ns |   143.163 ns | 3.7994 | 0.0153 |   23872 B |
| WeightedRatio                        | 11,923.68 ns | 17,209.35 ns |   943.303 ns | 2.3880 | 0.0076 |   14984 B |
| TokenInitialismRatio1                |    315.85 ns |    155.00 ns |     8.496 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    174.34 ns |    180.70 ns |     9.905 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    402.56 ns |    693.11 ns |    37.992 ns | 0.1845 |      - |    1160 B |
| PartialTokenInitialismRatio          |    566.65 ns |    399.73 ns |    21.910 ns | 0.3805 |      - |    2392 B |
| TokenAbbreviationRatio               |    709.60 ns |    334.15 ns |    18.316 ns | 0.3290 |      - |    2064 B |
| PartialTokenAbbreviationRatio        |  1,045.13 ns |  3,688.61 ns |   202.185 ns | 0.3977 | 0.0010 |    2496 B |
| Ratio1Classic                        |    254.88 ns |    222.22 ns |    12.181 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     44.95 ns |     47.77 ns |     2.619 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    915.14 ns |    694.25 ns |    38.054 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,327.40 ns |    553.93 ns |    30.363 ns | 0.3414 |      - |    2152 B |
| PartialTokenSortRatioClassic         |  1,371.74 ns |    484.91 ns |    26.580 ns | 0.3929 |      - |    2472 B |
| TokenSetRatioClassic                 |  1,960.89 ns |    373.76 ns |    20.487 ns | 0.6714 |      - |    4224 B |
| PartialTokenSetRatioClassic          |  2,521.34 ns |  2,799.32 ns |   153.440 ns | 0.9079 |      - |    5712 B |
| WeightedRatioClassic                 | 10,585.96 ns |  6,341.72 ns |   347.611 ns | 2.0294 |      - |   12770 B |
| TokenInitialismRatio1Classic         |    461.98 ns |    663.31 ns |    36.358 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    376.57 ns |    145.40 ns |     7.970 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    911.28 ns |    137.67 ns |     7.546 ns | 0.2460 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |  1,338.13 ns |  9,319.62 ns |   510.840 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,330.24 ns |  1,065.78 ns |    58.419 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,533.43 ns |    851.42 ns |    46.669 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 16,131.35 ns | 28,741.94 ns | 1,575.443 ns | 5.6763 | 0.0305 |   35704 B |
| ExtractOneClassic                    | 21,484.51 ns |  3,693.55 ns |   202.456 ns | 4.4556 |      - |   28003 B |
| LevenshteinDistance                  |    310.10 ns |    116.26 ns |     6.372 ns |      - |      - |         - |
| FastenshteinDistance                 |    638.76 ns |     92.10 ns |     5.048 ns | 0.0229 |      - |     144 B |
