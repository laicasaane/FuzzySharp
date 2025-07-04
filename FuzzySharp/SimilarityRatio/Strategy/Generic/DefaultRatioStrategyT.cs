﻿using System;

namespace Raffinert.FuzzySharp.SimilarityRatio.Strategy.Generic;

internal static class DefaultRatioStrategy<T> where T : IEquatable<T>
{
    public static int Calculate(T[] input1, T[] input2)
    {
        if (input1.Length == 0 || input2.Length == 0)
        {
            return 0;
        }
            
        var result = (int)Math.Round(100 * Indel.NormalizedSimilarity((ReadOnlySpan<T>)input1, (ReadOnlySpan<T>)input2));

        return result;
    }
}