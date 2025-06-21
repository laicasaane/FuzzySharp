using System;
using Raffinert.FuzzySharp.SimilarityRatio.Strategy.Generic;

namespace Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public sealed class PartialTokenDifferenceScorer : TokenDifferenceScorerBase
{
    protected override Func<string[], string[], int> Scorer => PartialRatioStrategy<string>.Calculate;
}