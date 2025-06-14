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
| Ratio1                               |    307.04 ns |    706.09 ns |    38.703 ns | 0.0191 |      - |     120 B |
| Ratio2                               |    129.14 ns |    102.86 ns |     5.638 ns | 0.0191 |      - |     120 B |
| PartialRatio                         |  2,539.51 ns |  2,921.08 ns |   160.114 ns | 1.5869 |      - |    9960 B |
| TokenSortRatio                       |    810.80 ns |    378.79 ns |    20.763 ns | 0.1097 |      - |     688 B |
| PartialTokenSortRatio                |  5,764.13 ns |  3,249.17 ns |   178.098 ns | 2.9068 | 0.0076 |   18264 B |
| TokenSetRatio                        |  1,525.59 ns |    646.29 ns |    35.425 ns | 0.3910 |      - |    2464 B |
| PartialTokenSetRatio                 |  7,589.84 ns |  8,172.15 ns |   447.943 ns | 3.2501 |      - |   20392 B |
| WeightedRatio                        |  6,905.35 ns |  1,432.27 ns |    78.508 ns | 0.8240 |      - |    5184 B |
| TokenInitialismRatio1                |    424.21 ns |  2,342.81 ns |   128.417 ns | 0.0815 |      - |     512 B |
| TokenInitialismRatio2                |    274.64 ns |    227.72 ns |    12.482 ns | 0.0739 |      - |     464 B |
| TokenInitialismRatio3                |    604.76 ns |  1,345.20 ns |    73.735 ns | 0.1297 |      - |     816 B |
| PartialTokenInitialismRatio          |    834.24 ns |  1,018.95 ns |    55.852 ns | 0.3490 |      - |    2200 B |
| TokenAbbreviationRatio               |  1,051.41 ns |     32.58 ns |     1.786 ns | 0.2728 |      - |    1720 B |
| PartialTokenAbbreviationRatio        |  1,510.53 ns |  1,444.21 ns |    79.162 ns | 0.3662 |      - |    2304 B |
| Ratio1Classic                        |    297.40 ns |    242.50 ns |    13.292 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     56.62 ns |     23.92 ns |     1.311 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |  1,128.52 ns |    373.71 ns |    20.484 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,711.46 ns |  1,238.74 ns |    67.900 ns | 0.3414 |      - |    2152 B |
| PartialTokenSortRatioClassic         |  1,901.65 ns |  4,033.33 ns |   221.080 ns | 0.3929 |      - |    2472 B |
| TokenSetRatioClassic                 |  3,857.32 ns |  2,329.69 ns |   127.698 ns | 0.6714 |      - |    4224 B |
| PartialTokenSetRatioClassic          |  2,898.19 ns |    552.22 ns |    30.269 ns | 0.9079 |      - |    5712 B |
| WeightedRatioClassic                 | 13,659.51 ns | 18,333.47 ns | 1,004.919 ns | 2.0294 |      - |   12770 B |
| TokenInitialismRatio1Classic         |    603.20 ns |    201.26 ns |    11.032 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    457.80 ns |    103.45 ns |     5.671 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |  1,235.10 ns |     75.36 ns |     4.131 ns | 0.2460 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |  1,343.69 ns |  1,267.60 ns |    69.481 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,493.74 ns |    667.17 ns |    36.570 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,795.85 ns |  2,026.87 ns |   111.099 ns | 0.6199 | 0.0019 |    3896 B |
| ExtractOne                           | 16,890.04 ns |  9,793.73 ns |   536.827 ns | 1.9379 |      - |   12208 B |
| ExtractOneClassic                    | 26,967.47 ns |  4,814.40 ns |   263.894 ns | 4.4556 |      - |   28003 B |
| FuzzySharpClassicDistance            |  1,096.72 ns |    205.53 ns |    11.266 ns | 0.0496 |      - |     320 B |
| FuzzySharpDistance                   |    484.93 ns |    152.26 ns |     8.346 ns | 0.0191 |      - |     120 B |
| FastenshteinDistance                 |    797.85 ns |    263.06 ns |    14.419 ns | 0.0229 |      - |     144 B |
| FuzzySharpDistanceFrom               |    141.41 ns |     35.55 ns |     1.948 ns |      - |      - |         - |
| FastenshteinDistanceFrom             |    827.50 ns |    338.72 ns |    18.567 ns |      - |      - |         - |
| QuickenshteinDistance                |    685.36 ns |    192.51 ns |    10.552 ns |      - |      - |         - |
