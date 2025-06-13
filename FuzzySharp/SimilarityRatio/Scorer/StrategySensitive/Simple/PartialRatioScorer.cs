using Raffinert.FuzzySharp.SimilarityRatio.Strategy;

namespace Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

public class PartialRatioScorer : SimpleRatioScorerBase
{
    protected override FuzzySharp.Scorer Scorer => PartialRatioStrategy.Calculate;
}