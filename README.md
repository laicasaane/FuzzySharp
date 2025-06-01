# Raffinert.FuzzySharp

C# .NET fuzzy string matching implementation of Seat Geek's well known python FuzzyWuzzy algorithm. 

Refined version of original [FuzzySharp](https://github.com/JakeBayer/FuzzySharp). The original one looks abandoned.

Benchcmark comparison of naive DP implementation (base line), FuzzySharp, Fastenshtein and Quickenshtein:

Random words of 3 to 64 random chars (LevenshteinSmall.cs):

| Method        | Mean     | Error     | StdDev    | Ratio | Gen0      | Gen1    | Allocated  | Alloc Ratio |
|-------------- |---------:|----------:|----------:|------:|----------:|--------:|-----------:|------------:|
| BaseLine      | 8.876 ms | 1.6983 ms | 0.0931 ms |  1.00 | 2078.1250 | 46.8750 | 13036006 B |       1.000 |
| Fastenshtein  | 4.537 ms | 0.9515 ms | 0.0522 ms |  0.51 |         - |       - |     8939 B |       0.001 |
| Quickenshtein | 2.735 ms | 0.2194 ms | 0.0120 ms |  0.31 |         - |       - |        4 B |       0.000 |
| FuzzySharp    | 2.229 ms | 0.3800 ms | 0.0208 ms |  0.25 |         - |       - |        2 B |       0.000 |

Random words of 3 to 128 random chars (LevenshteinNormal.cs):

| Method        | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0      | Gen1     | Allocated  | Alloc Ratio |
|-------------- |----------:|----------:|----------:|------:|--------:|----------:|---------:|-----------:|------------:|
| BaseLine      | 43.596 ms | 50.118 ms | 2.7471 ms |  1.00 |    0.08 | 8000.0000 | 769.2308 | 50620095 B |       1.000 |
| Fastenshtein  | 20.224 ms | 13.203 ms | 0.7237 ms |  0.47 |    0.03 |         - |        - |    16076 B |       0.000 |
| Quickenshtein |  7.816 ms |  1.877 ms | 0.1029 ms |  0.18 |    0.01 |         - |        - |       17 B |       0.000 |
| FuzzySharp    |  5.860 ms |  2.558 ms | 0.1402 ms |  0.13 |    0.01 |  320.3125 |        - |  2013603 B |       0.040 |

Random words of 3 to 1024 random chars (LevenshteinLarge.cs):

| Method        | Mean        | Error        | StdDev    | Ratio | RatioSD | Gen0        | Gen1        | Allocated    | Alloc Ratio |
|-------------- |------------:|-------------:|----------:|------:|--------:|------------:|------------:|-------------:|------------:|
| BaseLine      | 1,813.26 ms | 1,413.980 ms | 77.505 ms |  1.00 |    0.05 | 330000.0000 | 265000.0000 | 2070298480 B |       1.000 |
| Fastenshtein  |   964.00 ms |   660.636 ms | 36.212 ms |  0.53 |    0.03 |           - |           - |      93256 B |       0.000 |
| Quickenshtein |    91.74 ms |     6.908 ms |  0.379 ms |  0.05 |    0.00 |           - |           - |        123 B |       0.000 |
| FuzzySharp    |    43.35 ms |     2.757 ms |  0.151 ms |  0.02 |    0.00 |   1416.6667 |           - |    9341905 B |       0.005 |

# Release Notes:
v 3.0.0

Ported from [RapidFuzz](https://github.com/rapidfuzz/RapidFuzz) with Copilot:

- The Levenshtein naive dynamic programming algorithm was replaced with the faster Myers' bit-parallel version.
- Fuzz.PartialRatio and Fuzz.Ratio uses the longest common subsequence algorithm, see [ratio implementation](https://github.com/rapidfuzz/RapidFuzz/blob/main/api_differences.md#ratio-implementation) and [partial_ratio implememtation](https://github.com/rapidfuzz/RapidFuzz/blob/main/api_differences.md#partial_ratio-implementation).

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
