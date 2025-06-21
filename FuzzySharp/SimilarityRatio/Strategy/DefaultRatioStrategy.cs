using System;

namespace Raffinert.FuzzySharp.SimilarityRatio.Strategy;

internal static class DefaultRatioStrategy
{
    public static int Calculate(string input1, string input2)
    {
        if (input1.Length == 0 || input2.Length == 0)
        {
            return 0;
        }

        var input1Span = input1.AsSpan();
        var input2Span = input2.AsSpan();

        return (int)Math.Round(100 * Indel.NormalizedSimilarity(input1Span, input2Span));
    }
}