```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.4061)
12th Gen Intel Core i7-1255U, 1 CPU, 12 logical and 10 physical cores
.NET SDK 9.0.300
  [Host]   : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  ShortRun : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                               | Mean         | Error        | StdDev     | Gen0   | Gen1   | Allocated |
|------------------------------------- |-------------:|-------------:|-----------:|-------:|-------:|----------:|
| Ratio1                               |    277.91 ns |     2.104 ns |   0.115 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    257.92 ns |   116.782 ns |   6.401 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,441.58 ns | 1,997.522 ns | 109.491 ns | 1.9073 | 0.0038 |   11968 B |
| TokenSortRatio                       |    929.34 ns |   719.984 ns |  39.465 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  4,294.80 ns | 7,349.424 ns | 402.847 ns | 3.0975 | 0.0153 |   19456 B |
| TokenSetRatio                        |  1,556.66 ns | 1,109.873 ns |  60.836 ns | 0.8240 | 0.0019 |    5176 B |
| PartialTokenSetRatio                 |  5,246.76 ns | 4,729.775 ns | 259.255 ns | 3.8147 | 0.0153 |   23968 B |
| WeightedRatio                        |  7,723.74 ns | 2,048.773 ns | 112.300 ns | 2.3956 |      - |   15080 B |
| TokenInitialismRatio1                |    243.14 ns |   302.776 ns |  16.596 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    187.26 ns |   221.761 ns |  12.155 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    392.86 ns |   238.656 ns |  13.082 ns | 0.1845 |      - |    1160 B |
| PartialTokenInitialismRatio          |    635.31 ns |   139.590 ns |   7.651 ns | 0.3805 |      - |    2392 B |
| TokenAbbreviationRatio               |    666.41 ns |    37.377 ns |   2.049 ns | 0.3242 |      - |    2040 B |
| PartialTokenAbbreviationRatio        |    828.97 ns |   902.640 ns |  49.477 ns | 0.3939 | 0.0010 |    2472 B |
| Ratio1Classic                        |    283.60 ns |   139.740 ns |   7.660 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     42.79 ns |    11.904 ns |   0.652 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    894.24 ns |    51.777 ns |   2.838 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,464.75 ns |   614.937 ns |  33.707 ns | 0.3529 |      - |    2216 B |
| PartialTokenSortRatioClassic         |  1,584.76 ns | 2,236.676 ns | 122.600 ns | 0.4025 |      - |    2536 B |
| TokenSetRatioClassic                 |  2,052.71 ns |   399.651 ns |  21.906 ns | 0.6905 |      - |    4352 B |
| PartialTokenSetRatioClassic          |  2,408.52 ns | 1,759.259 ns |  96.431 ns | 0.9308 |      - |    5840 B |
| WeightedRatioClassic                 | 11,479.45 ns | 5,567.268 ns | 305.161 ns | 2.1362 |      - |   13482 B |
| TokenInitialismRatio1Classic         |    443.50 ns |   302.212 ns |  16.565 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    376.05 ns |   211.609 ns |  11.599 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    816.73 ns |   125.123 ns |   6.858 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |    943.61 ns |    49.797 ns |   2.730 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,291.01 ns |   297.395 ns |  16.301 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,543.87 ns |   350.465 ns |  19.210 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 14,880.42 ns | 9,584.673 ns | 525.368 ns | 5.7373 |      - |   35992 B |
| ExtractOneClassic                    | 22,824.81 ns | 4,091.983 ns | 224.295 ns | 4.6082 |      - |   29011 B |
| LevenshteinDistance                  |    328.50 ns |   130.278 ns |   7.141 ns |      - |      - |         - |
| FastenshteinDistance                 |    826.88 ns |   260.021 ns |  14.253 ns | 0.0229 |      - |     144 B |
