```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                               | Mean         | Error        | StdDev       | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|-------------:|-------------:|-------:|-------:|----------:|
| Ratio1                               |    339.80 ns |    717.33 ns |    39.319 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    256.57 ns |    106.05 ns |     5.813 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,059.05 ns |    118.60 ns |     6.501 ns | 1.9073 | 0.0038 |   11968 B |
| TokenSortRatio                       |    972.67 ns |  3,205.02 ns |   175.678 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  3,969.07 ns |  3,710.42 ns |   203.381 ns | 3.0975 | 0.0153 |   19456 B |
| TokenSetRatio                        |  1,684.79 ns |  1,918.88 ns |   105.180 ns | 0.8240 | 0.0019 |    5176 B |
| PartialTokenSetRatio                 |  4,898.67 ns |  1,309.72 ns |    71.790 ns | 3.8147 | 0.0153 |   23968 B |
| WeightedRatio                        |  7,783.53 ns | 11,614.87 ns |   636.650 ns | 2.3956 |      - |   15080 B |
| TokenInitialismRatio1                |    261.68 ns |    795.29 ns |    43.593 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    187.30 ns |    203.07 ns |    11.131 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    418.22 ns |    379.68 ns |    20.812 ns | 0.1841 |      - |    1160 B |
| PartialTokenInitialismRatio          |    585.06 ns |     74.19 ns |     4.067 ns | 0.3805 |      - |    2392 B |
| TokenAbbreviationRatio               |    641.99 ns |     86.79 ns |     4.758 ns | 0.3242 |      - |    2040 B |
| PartialTokenAbbreviationRatio        |    815.93 ns |    738.80 ns |    40.496 ns | 0.3939 | 0.0010 |    2472 B |
| Ratio1Classic                        |    274.14 ns |    608.55 ns |    33.356 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     42.02 ns |     21.01 ns |     1.152 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    875.96 ns |    108.66 ns |     5.956 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,391.31 ns |    157.37 ns |     8.626 ns | 0.3529 |      - |    2216 B |
| PartialTokenSortRatioClassic         |  1,601.75 ns |    578.33 ns |    31.700 ns | 0.4025 |      - |    2536 B |
| TokenSetRatioClassic                 |  2,028.86 ns |    549.40 ns |    30.114 ns | 0.6905 |      - |    4352 B |
| PartialTokenSetRatioClassic          |  2,430.43 ns |  3,224.92 ns |   176.769 ns | 0.9308 |      - |    5840 B |
| WeightedRatioClassic                 | 10,650.20 ns |  1,680.25 ns |    92.100 ns | 2.1362 |      - |   13482 B |
| TokenInitialismRatio1Classic         |    448.48 ns |    225.26 ns |    12.347 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    409.13 ns |    548.27 ns |    30.052 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    813.83 ns |    261.94 ns |    14.358 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |    904.93 ns |     86.79 ns |     4.757 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,265.82 ns |    380.57 ns |    20.861 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,545.58 ns |  1,058.12 ns |    57.999 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 15,233.67 ns | 31,679.80 ns | 1,736.477 ns | 5.7373 |      - |   35992 B |
| ExtractOneClassic                    | 23,025.41 ns | 16,791.08 ns |   920.376 ns | 4.6082 |      - |   29011 B |
| LevenshteinDistance                  |    320.53 ns |     53.53 ns |     2.934 ns |      - |      - |         - |
| FastenshteinDistance                 |    825.59 ns |    581.24 ns |    31.860 ns | 0.0229 |      - |     144 B |
