using System;
using Raffinert.FuzzySharp.SimilarityRatio.Strategy;

namespace Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public class PartialTokenSortScorer : TokenSortScorerBase
{
    protected override FuzzySharp.Scorer Scorer => PartialRatioStrategy.Calculate;
}