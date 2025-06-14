```

BenchmarkDotNet v0.15.1, Windows 11 (10.0.26100.4351/24H2/2024Update/HudsonValley)
12th Gen Intel Core i7-1255U 2.60GHz, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.301
  [Host]   : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2
  ShortRun : .NET 9.0.6 (9.0.625.26613), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                               | Mean         | Error         | StdDev     | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|--------------:|-----------:|-------:|-------:|----------:|
| Ratio1                               |    234.27 ns |     46.574 ns |   2.553 ns | 0.0191 |      - |     120 B |
| Ratio2                               |     89.81 ns |     19.289 ns |   1.057 ns | 0.0191 |      - |     120 B |
| PartialRatio                         |  2,070.64 ns |    705.606 ns |  38.677 ns | 1.5869 |      - |    9960 B |
| TokenSortRatio                       |    632.37 ns |     64.890 ns |   3.557 ns | 0.1097 |      - |     688 B |
| PartialTokenSortRatio                |  3,614.58 ns |    381.693 ns |  20.922 ns | 2.9106 | 0.0076 |   18264 B |
| TokenSetRatio                        |  1,066.00 ns |    224.842 ns |  12.324 ns | 0.3910 |      - |    2464 B |
| PartialTokenSetRatio                 |  4,562.94 ns |  6,320.477 ns | 346.447 ns | 3.2501 |      - |   20392 B |
| WeightedRatio                        |  6,068.17 ns |  5,196.749 ns | 284.851 ns | 0.8240 |      - |    5184 B |
| TokenInitialismRatio1                |    206.38 ns |     36.504 ns |   2.001 ns | 0.0815 |      - |     512 B |
| TokenInitialismRatio2                |    198.84 ns |     27.316 ns |   1.497 ns | 0.0739 |      - |     464 B |
| TokenInitialismRatio3                |    305.15 ns |     71.803 ns |   3.936 ns | 0.1297 |      - |     816 B |
| PartialTokenInitialismRatio          |    609.36 ns |    416.041 ns |  22.805 ns | 0.3500 |      - |    2200 B |
| TokenAbbreviationRatio               |    740.84 ns |    183.778 ns |  10.074 ns | 0.2737 |      - |    1720 B |
| PartialTokenAbbreviationRatio        |    898.27 ns |    218.763 ns |  11.991 ns | 0.3672 | 0.0010 |    2304 B |
| Ratio1Classic                        |    238.59 ns |     22.000 ns |   1.206 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     46.04 ns |      6.078 ns |   0.333 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    898.65 ns |    102.412 ns |   5.614 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,430.41 ns |    397.920 ns |  21.811 ns | 0.3414 |      - |    2152 B |
| PartialTokenSortRatioClassic         |  1,431.39 ns |    260.886 ns |  14.300 ns | 0.3929 |      - |    2472 B |
| TokenSetRatioClassic                 |  2,092.42 ns |    102.865 ns |   5.638 ns | 0.6714 |      - |    4224 B |
| PartialTokenSetRatioClassic          |  2,394.45 ns |    847.194 ns |  46.438 ns | 0.9079 |      - |    5712 B |
| WeightedRatioClassic                 | 10,699.39 ns |    607.476 ns |  33.298 ns | 2.0294 |      - |   12770 B |
| TokenInitialismRatio1Classic         |    468.94 ns |     69.266 ns |   3.797 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    377.78 ns |     74.198 ns |   4.067 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    867.62 ns |    210.920 ns |  11.561 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |    993.77 ns |    140.682 ns |   7.711 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,373.13 ns |  2,821.911 ns | 154.678 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,533.18 ns |    769.594 ns |  42.184 ns | 0.6180 |      - |    3896 B |
| ExtractOne                           | 11,420.56 ns |  1,415.464 ns |  77.586 ns | 1.9379 |      - |   12208 B |
| ExtractOneClassic                    | 21,752.48 ns | 12,737.471 ns | 698.184 ns | 4.4556 |      - |   28003 B |
| FuzzySharpClassicDistance            |  1,079.45 ns |     34.149 ns |   1.872 ns | 0.0496 |      - |     320 B |
| FuzzySharpDistance                   |    424.81 ns |    102.176 ns |   5.601 ns | 0.0191 |      - |     120 B |
| FastenshteinDistance                 |    712.12 ns |     89.437 ns |   4.902 ns | 0.0229 |      - |     144 B |
| FuzzySharpDistanceFrom               |    132.46 ns |     12.827 ns |   0.703 ns |      - |      - |         - |
| FastenshteinDistanceFrom             |    764.37 ns |    152.403 ns |   8.354 ns |      - |      - |         - |
| QuickenshteinDistance                |    672.38 ns |    168.508 ns |   9.237 ns |      - |      - |         - |
