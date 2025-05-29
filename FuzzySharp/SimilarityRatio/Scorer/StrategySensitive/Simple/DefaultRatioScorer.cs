using System;
using Raffinert.FuzzySharp.SimilarityRatio.Strategy;

namespace Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive
{
    public class DefaultRatioScorer : SimpleRatioScorerBase
    {
        protected override FuzzySharp.Scorer Scorer => DefaultRatioStrategy.Calculate;
    }
}
