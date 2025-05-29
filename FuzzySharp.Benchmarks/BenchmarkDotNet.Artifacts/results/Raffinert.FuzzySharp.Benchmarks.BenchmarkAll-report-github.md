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
| Ratio1                               |    291.22 ns |     60.98 ns |     3.343 ns | 0.1578 |      - |     992 B |
| Ratio2                               |    259.84 ns |    179.06 ns |     9.815 ns | 0.1578 |      - |     992 B |
| PartialRatio                         |  2,104.35 ns |  2,286.91 ns |   125.353 ns | 1.9073 | 0.0038 |   11968 B |
| TokenSortRatio                       |    848.33 ns |    229.40 ns |    12.574 ns | 0.2480 |      - |    1560 B |
| PartialTokenSortRatio                |  3,741.67 ns |    203.04 ns |    11.129 ns | 3.0975 | 0.0153 |   19456 B |
| TokenSetRatio                        |  1,442.78 ns |    192.79 ns |    10.567 ns | 0.8240 | 0.0019 |    5176 B |
| PartialTokenSetRatio                 |  4,836.98 ns |  2,903.06 ns |   159.126 ns | 3.8147 | 0.0153 |   23968 B |
| WeightedRatio                        | 11,535.37 ns | 54,024.18 ns | 2,961.248 ns | 2.4033 | 0.0076 |   15080 B |
| TokenInitialismRatio1                |    209.93 ns |     50.42 ns |     2.764 ns | 0.1364 |      - |     856 B |
| TokenInitialismRatio2                |    177.74 ns |     19.88 ns |     1.090 ns | 0.0892 |      - |     560 B |
| TokenInitialismRatio3                |    486.88 ns |  2,325.85 ns |   127.488 ns | 0.1845 |      - |    1160 B |
| PartialTokenInitialismRatio          |    782.60 ns |    679.15 ns |    37.226 ns | 0.3805 |      - |    2392 B |
| TokenAbbreviationRatio               |    780.67 ns |  1,582.60 ns |    86.748 ns | 0.3242 |      - |    2040 B |
| PartialTokenAbbreviationRatio        |    884.93 ns |    733.41 ns |    40.200 ns | 0.3939 | 0.0010 |    2472 B |
| Ratio1Classic                        |    272.93 ns |     45.00 ns |     2.467 ns | 0.0505 |      - |     320 B |
| Ratio2Classic                        |     45.42 ns |     11.76 ns |     0.645 ns | 0.0318 |      - |     200 B |
| PartialRatioClassic                  |    940.07 ns |     98.44 ns |     5.396 ns | 0.5360 | 0.0019 |    3368 B |
| TokenSortRatioClassic                |  1,952.30 ns |  2,823.72 ns |   154.777 ns | 0.3529 |      - |    2216 B |
| PartialTokenSortRatioClassic         |  1,671.46 ns |    310.67 ns |    17.029 ns | 0.4025 |      - |    2536 B |
| TokenSetRatioClassic                 |  2,207.46 ns |  1,315.52 ns |    72.108 ns | 0.6905 |      - |    4352 B |
| PartialTokenSetRatioClassic          |  2,498.37 ns |  1,460.94 ns |    80.079 ns | 0.9308 |      - |    5840 B |
| WeightedRatioClassic                 | 12,246.52 ns |  5,343.93 ns |   292.919 ns | 2.1362 |      - |   13482 B |
| TokenInitialismRatio1Classic         |    494.37 ns |    342.01 ns |    18.747 ns | 0.1440 |      - |     904 B |
| TokenInitialismRatio2Classic         |    386.37 ns |     46.74 ns |     2.562 ns | 0.1173 |      - |     736 B |
| TokenInitialismRatio3Classic         |    897.10 ns |    315.90 ns |    17.316 ns | 0.2470 |      - |    1552 B |
| PartialTokenInitialismRatioClassic   |  1,103.70 ns |    352.58 ns |    19.326 ns | 0.3414 |      - |    2144 B |
| TokenAbbreviationRatioClassic        |  1,523.74 ns |  3,047.59 ns |   167.049 ns | 0.4749 |      - |    2984 B |
| PartialTokenAbbreviationRatioClassic |  1,667.27 ns |    253.88 ns |    13.916 ns | 0.6199 |      - |    3896 B |
| ExtractOne                           | 16,137.06 ns | 21,060.43 ns | 1,154.393 ns | 5.7373 |      - |   35992 B |
| ExtractOneClassic                    | 25,432.34 ns | 28,663.98 ns | 1,571.169 ns | 4.6082 |      - |   29011 B |
| LevenshteinDistance                  |    387.06 ns |    143.52 ns |     7.867 ns |      - |      - |         - |
| FastenshteinDistance                 |  1,015.74 ns |    793.97 ns |    43.520 ns | 0.0229 |      - |     144 B |
