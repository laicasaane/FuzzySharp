using System;
using Raffinert.FuzzySharp.SimilarityRatio.Strategy.Generic;

namespace Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public sealed class TokenDifferenceScorer : TokenDifferenceScorerBase
{
    protected override Func<string[], string[], int> Scorer => DefaultRatioStrategy<string>.Calculate;
}