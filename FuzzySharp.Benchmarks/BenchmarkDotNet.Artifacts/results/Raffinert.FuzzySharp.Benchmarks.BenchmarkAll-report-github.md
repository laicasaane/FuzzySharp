```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2


```
| Method                               | Mean         | Error      | StdDev       | Median       | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|-----------:|-------------:|-------------:|-------:|-------:|----------:|
| Ratio1                               |    280.35 ns |   4.999 ns |     6.322 ns |    278.53 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    255.04 ns |   5.106 ns |    11.526 ns |    250.85 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,125.28 ns |  34.112 ns |    30.240 ns |  2,116.86 ns | 1.9875 | 0.0038 |   12480 B |
| TokenSortRatio                       |  1,256.96 ns |  97.465 ns |   282.764 ns |  1,367.90 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  8,546.52 ns | 616.339 ns | 1,748.451 ns |  8,071.70 ns | 3.1891 | 0.0153 |   20032 B |
| TokenSetRatio                        |  2,644.45 ns | 140.935 ns |   399.809 ns |  2,489.84 ns | 0.8240 | 0.0019 |    5176 B |
| PartialTokenSetRatio                 | 10,520.02 ns | 833.291 ns | 2,430.749 ns | 10,007.61 ns | 3.9673 | 0.0153 |   24928 B |
| WeightedRatio                        | 12,408.59 ns | 521.288 ns | 1,461.748 ns | 12,381.82 ns | 2.3956 |      - |   15080 B |
| TokenInitialismRatio1                |    400.77 ns |  19.591 ns |    54.936 ns |    377.18 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    291.76 ns |   7.349 ns |    20.966 ns |    288.88 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    624.19 ns |  20.650 ns |    58.581 ns |    629.57 ns | 0.1841 |      - |    1160 B |
| PartialTokenInitialismRatio          |    949.78 ns |  16.130 ns |    34.025 ns |    953.67 ns | 0.4015 |      - |    2520 B |
| TokenAbbreviationRatio               |    975.59 ns |  21.150 ns |    60.683 ns |    980.37 ns | 0.3242 |      - |    2040 B |
| PartialTokenAbbreviationRatio        |  1,141.50 ns |  22.625 ns |    59.604 ns |  1,136.47 ns | 0.4034 | 0.0010 |    2536 B |
| Ratio1Classic                        |    455.19 ns |   9.942 ns |    27.217 ns |    462.01 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     74.88 ns |   2.326 ns |     6.406 ns |     74.61 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |  1,517.27 ns |  30.100 ns |    58.709 ns |  1,522.12 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  2,166.19 ns |  42.979 ns |   109.395 ns |  2,141.37 ns | 0.3529 |      - |    2216 B |
| PartialTokenSortRatioClassic         |  2,391.41 ns |  85.499 ns |   245.313 ns |  2,323.90 ns | 0.4025 |      - |    2536 B |
| TokenSetRatioClassic                 |  3,436.89 ns | 241.972 ns |   682.486 ns |  3,216.53 ns | 0.6905 |      - |    4352 B |
| PartialTokenSetRatioClassic          |  3,517.82 ns | 122.634 ns |   349.882 ns |  3,432.61 ns | 0.9308 |      - |    5840 B |
| WeightedRatioClassic                 | 16,242.41 ns | 307.259 ns |   341.518 ns | 16,253.70 ns | 2.1362 |      - |   13482 B |
| TokenInitialismRatio1Classic         |    770.23 ns |  45.812 ns |   128.460 ns |    726.13 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    750.31 ns |  76.674 ns |   219.992 ns |    696.03 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |  1,771.79 ns | 117.922 ns |   343.983 ns |  1,684.36 ns | 0.2460 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |  1,552.39 ns |  46.792 ns |   130.437 ns |  1,537.75 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,863.12 ns |  36.863 ns |    57.392 ns |  1,873.97 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  2,753.83 ns | 189.919 ns |   556.998 ns |  2,616.02 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 22,285.47 ns | 433.095 ns | 1,125.671 ns | 22,262.53 ns | 5.7373 | 0.0153 |   35992 B |
| ExtractOneClassic                    | 36,284.37 ns | 838.533 ns | 2,295.469 ns | 36,561.04 ns | 4.6082 |      - |   29011 B |
| LevenshteinDistance                  |    534.70 ns |  10.263 ns |    23.787 ns |    527.15 ns |      - |      - |         - |
| FastenshteinDistance                 |  1,061.30 ns |  12.591 ns |    11.162 ns |  1,059.23 ns | 0.0229 |      - |     144 B |
