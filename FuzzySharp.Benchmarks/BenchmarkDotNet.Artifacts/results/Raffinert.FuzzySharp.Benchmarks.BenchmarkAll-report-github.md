```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                               | Mean         | Error       | StdDev     | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|------------:|-----------:|-------:|-------:|----------:|
| Ratio1                               |    265.21 ns |   163.07 ns |   8.938 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    249.50 ns |    59.43 ns |   3.257 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,033.30 ns |   337.97 ns |  18.525 ns | 1.9073 | 0.0038 |   11968 B |
| TokenSortRatio                       |    836.67 ns |   173.89 ns |   9.532 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  3,881.22 ns | 1,778.38 ns |  97.479 ns | 3.1013 | 0.0191 |   19456 B |
| TokenSetRatio                        |  1,429.63 ns |   274.39 ns |  15.040 ns | 0.8240 | 0.0019 |    5176 B |
| PartialTokenSetRatio                 |  4,908.61 ns | 6,610.46 ns | 362.342 ns | 3.8147 | 0.0153 |   23968 B |
| WeightedRatio                        |  6,995.87 ns |   155.02 ns |   8.497 ns | 2.4033 | 0.0076 |   15080 B |
| TokenInitialismRatio1                |    329.47 ns | 1,497.10 ns |  82.061 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    171.33 ns |    42.10 ns |   2.307 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    361.49 ns |    42.47 ns |   2.328 ns | 0.1845 |      - |    1160 B |
| PartialTokenInitialismRatio          |    576.48 ns |   102.63 ns |   5.625 ns | 0.3805 |      - |    2392 B |
| TokenAbbreviationRatio               |    641.49 ns |    80.72 ns |   4.425 ns | 0.3242 |      - |    2040 B |
| PartialTokenAbbreviationRatio        |    783.04 ns |   284.87 ns |  15.615 ns | 0.3939 | 0.0010 |    2472 B |
| Ratio1Classic                        |    239.91 ns |    33.31 ns |   1.826 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     40.93 ns |    15.50 ns |   0.849 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    853.88 ns |   273.82 ns |  15.009 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,364.37 ns |    89.97 ns |   4.932 ns | 0.3529 |      - |    2216 B |
| PartialTokenSortRatioClassic         |  1,416.83 ns |   365.20 ns |  20.018 ns | 0.4025 |      - |    2536 B |
| TokenSetRatioClassic                 |  1,979.65 ns |   586.92 ns |  32.171 ns | 0.6905 |      - |    4352 B |
| PartialTokenSetRatioClassic          |  2,223.32 ns |   113.43 ns |   6.217 ns | 0.9308 |      - |    5840 B |
| WeightedRatioClassic                 | 10,467.55 ns |   715.61 ns |  39.225 ns | 2.1362 |      - |   13482 B |
| TokenInitialismRatio1Classic         |    430.88 ns |    81.89 ns |   4.489 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    343.63 ns |    59.73 ns |   3.274 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    785.68 ns |    24.93 ns |   1.366 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |    928.16 ns |   373.27 ns |  20.460 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,247.55 ns |    78.86 ns |   4.323 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,461.82 ns |   458.48 ns |  25.131 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 13,757.82 ns | 9,918.52 ns | 543.667 ns | 5.7373 | 0.0153 |   35992 B |
| ExtractOneClassic                    | 21,413.23 ns |   893.69 ns |  48.986 ns | 4.6082 |      - |   29011 B |
| LevenshteinDistance                  |    315.34 ns |   106.67 ns |   5.847 ns |      - |      - |         - |
| FastenshteinDistance                 |    814.36 ns |    88.06 ns |   4.827 ns | 0.0229 |      - |     144 B |
