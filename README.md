[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/banner2-direct.svg)](https://stand-with-ukraine.pp.ua)

# Raffinert.FuzzySharp

C# .NET fuzzy string matching implementation of Seat Geek's well known python FuzzyWuzzy algorithm. 

Refined version of original [FuzzySharp](https://github.com/JakeBayer/FuzzySharp). The original one looks abandoned.

Benchcmark comparison of naive DP implementation (base line), FuzzySharp, Fastenshtein and Quickenshtein:

Random words of 3 to 64 random chars (LevenshteinSmall.cs):

| Method            | Mean       | Error     | StdDev   | Ratio | RatioSD | Gen0     | Gen1   | Allocated | Alloc Ratio |
|------------------ |-----------:|----------:|---------:|------:|--------:|---------:|-------:|----------:|------------:|
| NaiveDp           | 1,841.4 μs | 753.15 μs | 41.28 μs |  1.00 |    0.03 | 371.0938 | 9.7656 | 2335169 B |       1.000 |
| FuzzySharpClassic | 1,090.0 μs |  23.48 μs |  1.29 μs |  0.59 |    0.01 |  23.4375 |      - |  149793 B |       0.064 |
| Fastenshtein      |   860.4 μs |  80.93 μs |  4.44 μs |  0.47 |    0.01 |        - |      - |    3728 B |       0.002 |
| Quickenshtein     |   531.9 μs |  52.00 μs |  2.85 μs |  0.29 |    0.01 |        - |      - |       1 B |       0.000 |
| FuzzySharp        |   117.7 μs |  11.88 μs |  0.65 μs |  0.06 |    0.00 |   0.3662 |      - |    3040 B |       0.001 |

Random words of 3 to 128 random chars (LevenshteinNormal.cs):

| Method            | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0      | Gen1     | Allocated  | Alloc Ratio |
|------------------ |-----------:|------------:|----------:|------:|--------:|----------:|---------:|-----------:|------------:|
| NaiveDp           | 8,476.4 μs | 1,473.04 μs |  80.74 μs |  1.00 |    0.01 | 1593.7500 | 203.1250 | 10012118 B |       1.000 |
| FuzzySharpClassic | 5,263.1 μs |   851.79 μs |  46.69 μs |  0.62 |    0.01 |   46.8750 |        - |   300051 B |       0.030 |
| Fastenshtein      | 4,255.6 μs |   169.65 μs |   9.30 μs |  0.50 |    0.00 |         - |        - |     7067 B |       0.001 |
| Quickenshtein     | 1,674.9 μs | 3,872.17 μs | 212.25 μs |  0.20 |    0.02 |         - |        - |        2 B |       0.000 |
| FuzzySharp        |   463.1 μs |    64.00 μs |   3.51 μs |  0.05 |    0.00 |         - |        - |     3041 B |       0.000 |

Random words of 3 to 1024 random chars (LevenshteinLarge.cs):

| Method            | Mean       | Error       | StdDev    | Ratio | RatioSD | Gen0       | Gen1       | Allocated   | Alloc Ratio |
|------------------ |-----------:|------------:|----------:|------:|--------:|-----------:|-----------:|------------:|------------:|
| NaiveDp           | 243.037 ms |  45.1515 ms | 2.4749 ms |  1.00 |    0.01 | 43666.6667 | 35333.3333 | 275312853 B |       1.000 |
| FuzzySharpClassic | 149.659 ms |  10.5321 ms | 0.5773 ms |  0.62 |    0.01 |          - |          - |   1545732 B |       0.006 |
| Fastenshtein      | 137.856 ms | 121.5120 ms | 6.6605 ms |  0.57 |    0.02 |          - |          - |     34028 B |       0.000 |
| Quickenshtein     |  13.651 ms |   1.8536 ms | 0.1016 ms |  0.06 |    0.00 |          - |          - |        12 B |       0.000 |
| FuzzySharp        |   5.478 ms |   0.4256 ms | 0.0233 ms |  0.02 |    0.00 |          - |          - |      3051 B |       0.000 |


# Release Notes:
**v3.0.0 (upcoming)** – *Partial Ratio Accuracy and Performance Update*  

- **Improved Partial Matching Accuracy:** The `PartialRatio` algorithm has been overhauled to ensure it always finds the best possible substring match between two strings. In earlier versions, `Fuzz.PartialRatio` could return suboptimal scores in certain cases (for example, when a short string appeared multiple times in a longer string, it didn’t always pick the highest scoring match). This has now been fixed. Partial ratio comparisons are **much more accurate**, resolving bugs inherited from the original FuzzyWuzzy implementation. You may notice that `PartialRatio` scores differ from previous versions – they now reflect the true highest similarity. For instance, if a string “foo” appears twice in “...foo...foo...”, `PartialRatio` will correctly return the higher score of the two occurrences.

- **Consistent Scoring Behavior:** With this fix, partial matches will no longer be misidentified as full matches. Only a complete match will yield a 100 score, whereas a partially matching substring will receive a proportionally lower score. This change makes the behavior of partial scorers more intuitive. Functions like `Fuzz.PartialRatio`, `Fuzz.PartialTokenSortRatio`, `Fuzz.PartialTokenSetRatio`, etc., now all leverage the improved logic, so their results are more reliable across the board. This brings FuzzySharp’s output in line with expected fuzzy matching standards (matching the behavior of well-maintained libraries like RapidFuzz, while still running on .NET).

- **Performance Optimizations:** The new implementation of `PartialRatio` was built with performance in mind. Despite the more sophisticated matching process, **fuzzy searches are just as fast** as before (in many cases faster). Internal algorithms for finding matches and calculating edit distances were optimized to reduce unnecessary allocations and computations. Advanced techniques (such as optimized character scanning and reuse of intermediate results) minimize overhead. This means you get improved accuracy **without any slowdown**. The library continues to be suitable for large-scale fuzzy matching (e.g., thousands of comparisons) with high performance.

- **Usage Notes:** The public API for fuzzy scoring remains the same. You do not need to change any code to benefit from these improvements – all improvements are under the hood. Simply update to this version, and `Fuzz.PartialRatio` and related methods will automatically produce better results. If your application had workaround code or custom post-processing due to the previous partial ratio quirks, you may consider simplifying it now. For example, if you lowered a threshold because partial matches scored unexpectedly high, you might re-tune that threshold given the more faithful scoring. Overall, the upgrade should be smooth: all existing method signatures and return types are unchanged, and only the accuracy of results (and potentially their ordering when ranking matches) has improved.

- Ported from python library [RapidFuzz](https://github.com/rapidfuzz/RapidFuzz)

v.2.0.3

Accent to performance and allocations. See [Benchmark](https://github.com/Raffinert/FuzzySharp/blob/dc2b858dc4cc56d8cdf26411904e255a019b0549/FuzzySharp.Benchmarks/BenchmarkDotNet.Artifacts/results/Raffinert.FuzzySharp.Benchmarks.BenchmarkAll-report-github.md).
Support local languages more naturally (removed regexps "a-zA-Z"). All regexps were replaced with string manipulations (fixes [PR!7](https://github.com/JakeBayer/FuzzySharp/pull/7)).
Extra performance improvement, reused approach [Dmitry Sushchevsky](https://github.com/blowin) - see [PR!42](https://github.com/JakeBayer/FuzzySharp/pull/42).
Implemented new Process.ExtractAll method, see [Issue!46](https://github.com/JakeBayer/FuzzySharp/issues/46).
Remove support of outdated/vulnerable platforms netcoreapp2.0;netcoreapp2.1;netstandard1.6.

v.2.0.0

As of 2.0.0, all empty strings will return a score of 0. Prior, the partial scoring system would return a score of 100, regardless if the other input had correct value or not. This was a result of the partial scoring system returning an empty set for the matching blocks As a result, this led to incorrrect values in the composite scores; several of them (token set, token sort), relied on the prior value of empty strings.

As a result, many 1.X.X unit test may be broken with the 2.X.X upgrade, but it is within the expertise fo all the 1.X.X developers to recommednd the upgrade to the 2.X.X series regardless, should their version accommodate it or not, as it is closer to the ideal behavior of the library.

## Usage

Install-Package Raffinert.FuzzySharp

#### Simple Ratio
```csharp
Fuzz.Ratio("mysmilarstring","myawfullysimilarstirng")
72
Fuzz.Ratio("mysmilarstring","mysimilarstring")
97
```

#### Partial Ratio
```csharp
Fuzz.PartialRatio("similar", "somewhresimlrbetweenthisstring")
71
```

#### Token Sort Ratio
```csharp
Fuzz.TokenSortRatio("order words out of","  words out of order")
100
Fuzz.PartialTokenSortRatio("order words out of","  words out of order")
100
```

#### Token Set Ratio
```csharp
Fuzz.TokenSetRatio("fuzzy was a bear", "fuzzy fuzzy fuzzy bear")
100
Fuzz.PartialTokenSetRatio("fuzzy was a bear", "fuzzy fuzzy fuzzy bear")
100
```

#### Token Initialism Ratio
```csharp
Fuzz.TokenInitialismRatio("NASA", "National Aeronautics and Space Administration");
89
Fuzz.TokenInitialismRatio("NASA", "National Aeronautics Space Administration");
100

Fuzz.TokenInitialismRatio("NASA", "National Aeronautics Space Administration, Kennedy Space Center, Cape Canaveral, Florida 32899");
53
Fuzz.PartialTokenInitialismRatio("NASA", "National Aeronautics Space Administration, Kennedy Space Center, Cape Canaveral, Florida 32899");
100
```

#### Token Abbreviation Ratio
```csharp
Fuzz.TokenAbbreviationRatio("bl 420", "Baseline section 420", PreprocessMode.Full);
40
Fuzz.PartialTokenAbbreviationRatio("bl 420", "Baseline section 420", PreprocessMode.Full);
67      
```


#### Weighted Ratio
```csharp
Fuzz.WeightedRatio("The quick brown fox jimps ofver the small lazy dog", "the quick brown fox jumps over the small lazy dog")
95
```

#### Process
```csharp
Process.ExtractOne("cowboys", new[] { "Atlanta Falcons", "New York Jets", "New York Giants", "Dallas Cowboys"})
(string: Dallas Cowboys, score: 90, index: 3)
```
```csharp
Process.ExtractTop("goolge", new[] { "google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl" }, limit: 3);
[(string: google, score: 83, index: 0), (string: googleplus, score: 75, index: 5), (string: plexoogl, score: 43, index: 7)]
```
```csharp
Process.ExtractAll("goolge", new [] {"google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl" })
[(string: google, score: 83, index: 0), (string: bing, score: 22, index: 1), (string: facebook, score: 29, index: 2), (string: linkedin, score: 29, index: 3), (string: twitter, score: 15, index: 4), (string: googleplus, score: 75, index: 5), (string: bingnews, score: 29, index: 6), (string: plexoogl, score: 43, index: 7)]
// score cutoff
Process.ExtractAll("goolge", new[] { "google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl" }, cutoff: 40)
[(string: google, score: 83, index: 0), (string: googleplus, score: 75, index: 5), (string: plexoogl, score: 43, index: 7)]
```
```csharp
Process.ExtractSorted("goolge", new [] {"google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl" })
[(string: google, score: 83, index: 0), (string: googleplus, score: 75, index: 5), (string: plexoogl, score: 43, index: 7), (string: facebook, score: 29, index: 2), (string: linkedin, score: 29, index: 3), (string: bingnews, score: 29, index: 6), (string: bing, score: 22, index: 1), (string: twitter, score: 15, index: 4)]
```

Extraction will use `WeightedRatio` and `full process` by default. Override these in the method parameters to use different scorers and processing.
Here we use the Fuzz.Ratio scorer and keep the strings as is, instead of Full Process (which will .ToLowercase() before comparing)
```csharp
Process.ExtractOne("cowboys", new[] { "Atlanta Falcons", "New York Jets", "New York Giants", "Dallas Cowboys" }, s => s, ScorerCache.Get<DefaultRatioScorer>());
(string: Dallas Cowboys, score: 57, index: 3)
```

Extraction can operate on objects of similar type. Use the "process" parameter to reduce the object to the string which it should be compared on. In the following example, the object is an array that contains the matchup, the arena, the date, and the time. We are matching on the first (0 index) parameter, the matchup.
```csharp
var events = new[]
{
    new[] { "chicago cubs vs new york mets", "CitiField", "2011-05-11", "8pm" },
    new[] { "new york yankees vs boston red sox", "Fenway Park", "2011-05-11", "8pm" },
    new[] { "atlanta braves vs pittsburgh pirates", "PNC Park", "2011-05-11", "8pm" },
};
var query = new[] { "new york mets vs chicago cubs", "CitiField", "2017-03-19", "8pm" };
var best = Process.ExtractOne(query, events, strings => strings[0]);

best: (value: { "chicago cubs vs new york mets", "CitiField", "2011-05-11", "8pm" }, score: 95, index: 0)
```

### Using Different Scorers
Scoring strategies are stateless, and as such should be static. However, in order to get them to share all the code they have in common via inheritance, making them static was not possible.
Currently one way around having to new up an instance everytime you want to use one is to use the cache. This will ensure only one instance of each scorer ever exists.
```csharp
var ratio = ScorerCache.Get<DefaultRatioScorer>();
var partialRatio = ScorerCache.Get<PartialRatioScorer>();
var tokenSet = ScorerCache.Get<TokenSetScorer>();
var partialTokenSet = ScorerCache.Get<PartialTokenSetScorer>();
var tokenSort = ScorerCache.Get<TokenSortScorer>();
var partialTokenSort = ScorerCache.Get<PartialTokenSortScorer>();
var tokenAbbreviation = ScorerCache.Get<TokenAbbreviationScorer>();
var partialTokenAbbreviation = ScorerCache.Get<PartialTokenAbbreviationScorer>();
var weighted = ScorerCache.Get<WeightedRatioScorer>();
```

## Credits

- SeatGeek
- Adam Cohen
- David Necas (python-Levenshtein)
- Jacob Bayer (original FuzzySharp library)
- Max Bachmann (RapidFuzz)
- Mikko Ohtamaa (python-Levenshtein)
- Antti Haapala (python-Levenshtein)
- Panayiotis (Java implementation I heavily borrowed from)
