using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp;

public static class Indel
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, 
        Processor<T> processor = null, 
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        } 

        var maximum = s1.Length + s2.Length;
        var lcsSim = LongestCommonSequence.Similarity(s1, s2);
        var dist = maximum - 2 * lcsSim;
        
        var result = scoreCutoff == null || dist <= scoreCutoff.Value 
            ? dist 
            : scoreCutoff.Value + 1;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizedDistance<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, 
        Processor<T> processor = null,
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        }

        var maximum = s1.Length + s2.Length;
        var dist = Distance(s1, s2);
        var normDist = maximum == 0 ? 0 : dist/(double)maximum;
        
        var result = scoreCutoff == null || normDist <= scoreCutoff.Value
            ? normDist
            : scoreCutoff.Value + 1;

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizedSimilarity<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        Processor<T> processor = null,
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        }

        var normDist = NormalizedDistance(s1, s2);
        var normSim = 1 - normDist;

        var result = scoreCutoff == null || normSim >= scoreCutoff.Value
            ? normSim
            : 0;

        return result;
    }

    public static int BlockDistance<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        var maximum = s1.Length + s2.Length;
        var lcsSim = LongestCommonSequence.BlockSimilarityMultipleMachineWords(block, s1, s2);
        var dist = maximum - 2 * lcsSim;
        
        var result = scoreCutoff == null || dist <= scoreCutoff.Value 
            ? dist 
            : scoreCutoff.Value + 1;

        return result;
    }

    public static double BlockNormalizedDistance<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, 
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        var maximum = s1.Length + s2.Length;
        var dist = BlockDistance(block, s1, s2);
        var normDist = maximum == 0 ? 0 : dist / (double)maximum;
        
        var result = scoreCutoff == null || normDist <= scoreCutoff.Value
            ? normDist
            : 1;

        return result;
    }

    public static double BlockNormalizedSimilarity<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, 
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        var normDist = BlockNormalizedDistance(block, s1, s2);
        var normSim = 1.0 - normDist;
        
        var result = scoreCutoff == null || normSim >= scoreCutoff.Value 
            ? normSim 
            : 0;
        
        return result;
    }
}