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
| Ratio1                               |    358.50 ns |  2,083.89 ns |   114.225 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    289.04 ns |    282.34 ns |    15.476 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,126.11 ns |  1,405.77 ns |    77.055 ns | 1.8196 |      - |   11424 B |
| TokenSortRatio                       |  1,008.38 ns |  1,003.47 ns |    55.004 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  4,181.22 ns |  1,916.51 ns |   105.050 ns | 2.9068 | 0.0076 |   18256 B |
| TokenSetRatio                        |  1,576.98 ns |    197.60 ns |    10.831 ns | 0.8087 | 0.0019 |    5080 B |
| PartialTokenSetRatio                 |  4,985.15 ns |  2,172.85 ns |   119.101 ns | 3.2425 |      - |   20368 B |
| WeightedRatio                        |  7,293.84 ns |  8,752.32 ns |   479.744 ns | 2.3880 | 0.0076 |   14984 B |
| TokenInitialismRatio1                |    207.49 ns |     46.30 ns |     2.538 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    169.36 ns |    118.49 ns |     6.495 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    363.31 ns |     84.22 ns |     4.616 ns | 0.1845 |      - |    1160 B |
| PartialTokenInitialismRatio          |    577.10 ns |    372.96 ns |    20.443 ns | 0.3490 |      - |    2192 B |
| TokenAbbreviationRatio               |    743.23 ns |    701.61 ns |    38.457 ns | 0.3290 |      - |    2064 B |
| PartialTokenAbbreviationRatio        |    764.30 ns |    450.01 ns |    24.666 ns | 0.3710 | 0.0010 |    2328 B |
| Ratio1Classic                        |    224.75 ns |    147.63 ns |     8.092 ns | 0.0508 |      - |     320 B |
| Ratio2Classic                        |     52.67 ns |     18.35 ns |     1.006 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |  1,042.90 ns |  3,651.91 ns |   200.174 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,258.49 ns |    223.59 ns |    12.256 ns | 0.3414 |      - |    2152 B |
| PartialTokenSortRatioClassic         |  1,394.65 ns |    972.10 ns |    53.284 ns | 0.3929 |      - |    2472 B |
| TokenSetRatioClassic                 |  2,736.42 ns |  5,678.59 ns |   311.263 ns | 0.6714 |      - |    4224 B |
| PartialTokenSetRatioClassic          |  2,293.80 ns |  1,747.16 ns |    95.768 ns | 0.9079 |      - |    5712 B |
| WeightedRatioClassic                 | 10,634.40 ns |  4,949.97 ns |   271.325 ns | 2.0294 |      - |   12770 B |
| TokenInitialismRatio1Classic         |    424.46 ns |    112.42 ns |     6.162 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    356.26 ns |    345.33 ns |    18.929 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    825.27 ns |    541.81 ns |    29.698 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |  1,065.48 ns |  2,187.22 ns |   119.889 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,528.49 ns |  3,978.29 ns |   218.064 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,853.71 ns |  3,209.11 ns |   175.902 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 18,716.58 ns | 98,495.68 ns | 5,398.881 ns | 5.6915 | 0.0305 |   35704 B |
| ExtractOneClassic                    | 25,707.00 ns | 30,187.36 ns | 1,654.671 ns | 4.4556 |      - |   28003 B |
| FuzzySharpDistance                   |    431.06 ns |    178.86 ns |     9.804 ns |      - |      - |         - |
| FastenshteinDistance                 |    782.00 ns |    612.26 ns |    33.560 ns | 0.0229 |      - |     144 B |
| QuickenshteinDistance                |    693.60 ns |    188.01 ns |    10.306 ns |      - |      - |         - |
