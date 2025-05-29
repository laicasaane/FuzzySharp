using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp;

public static class Indel
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, Processor<T> processor = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        } 
        //if processor is not None:
        //     s1 = processor(s1)
        //     s2 = processor(s2)

        var maximum = s1.Length + s2.Length;
        var lcs_sim = LongestCommonSequence.Similarity(s1, s2);
        var dist = maximum - 2 * lcs_sim;
        return dist;
        // return dist if (score_cutoff is None or dist <= score_cutoff) else score_cutoff + 1
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizedDistance<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2, 
        Processor<T> processor = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        }

        var maximum = s1.Length + s2.Length;
        var dist = Distance(s1, s2);
        var normDist = maximum == 0 ? 0 : dist/(double)maximum;
        return normDist;
        // return norm_dist if (score_cutoff is None or norm_dist <= score_cutoff) else 1
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormalizedSimilarity<T>(ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        Processor<T> processor = null) where T : IEquatable<T>
    {
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        }

        var normDist = NormalizedDistance(s1, s2);
        var normSim = 1- normDist;
        return normSim;
        //return norm_sim if (score_cutoff is None or norm_sim >= score_cutoff) else 0
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BlockDistance<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        var maximum = s1.Length + s2.Length;
        var lcs_sim = LongestCommonSequence.BlockSimilarityMultipleMachineWords(block, s1, s2);
        var dist = maximum - 2 * lcs_sim;
        return dist;
        //return dist if (score_cutoff is None or dist <= score_cutoff) else score_cutoff + 1
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double BlockNormalizedDistance<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        var maximum = s1.Length + s2.Length;
        var dist = BlockDistance(block, s1, s2);
        var normDist = maximum == 0 ? 0 : dist / (double)maximum;
        return normDist;
        //return norm_dist if (score_cutoff is None or norm_dist <= score_cutoff) else 1
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double BlockNormalizedSimilarity<T>(
        Dictionary<T, ulong[]> block,
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        var normDist = BlockNormalizedDistance(block, s1, s2);
        var normSim = 1.0 - normDist;
        return normSim;
        //return norm_sim if (score_cutoff is None or norm_sim >= score_cutoff) else 0
    }

    //    /// <summary>
    //    /// Returns the minimal number of insertions+deletions
    //    /// to transform s1 into s2.
    //    /// Uses a bit-parallel LCS for |s1| ≤ 64.
    //    /// </summary>
    //    public static int DistanceOneMachineWord<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2, int? scoreCutoff) where T : IEquatable<T>
    //    {
    //        int m = s1.Length;
    //        int n = s2.Length;
    //        if (m == 0) return n;

    //        // Build per-char mask for s1 (positions 0..m-1 mapped into bits 0..m-1)
    //        var mask = new Dictionary<T, ulong>(EqualityComparer<T>.Default);
    //        for (int i = 0; i < m; i++)
    //        {
    //            ulong bit = 1UL << i;
    //            T c = s1[i];
    //            var cur = mask.GetValueOrDefault(c, 0UL);
    //            mask[c] = cur | bit;
    //        }

    //        // S is our accumulator (initially 0)
    //        ulong S = 0UL;

    //        for (int j = 0; j < n; j++)
    //        {
    //            // 1) load mask or zero
    //            mask.TryGetValue(s2[j], out ulong M);

    //            // 2) X = M|S, Y = (S<<1)|1
    //            ulong X = M | S;
    //            ulong Y = (S << 1) | 1UL;

    //            // 3) D = X - Y  (single-word, borrow discarded)
    //            ulong D = X - Y;

    //            // 4) update S = X & (X ^ D)
    //            S = X & (X ^ D);

    //            // early cutoff?
    //            if (scoreCutoff.HasValue)
    //            {
    //                int matched = PopCount(S);
    //                int processed = j + 1;
    //                int nRem = n - processed;
    //                // at most we can match all remaining in the shorter of the two
    //                int maxLcs = matched + Math.Min(m - matched, nRem);
    //                int minDist = m + n - 2 * maxLcs;
    //                if (minDist > scoreCutoff.Value)
    //                    return scoreCutoff.Value + 1;
    //            }
    //        }

    //        // popcount(S) = length of LCS

    //        int lcs = PopCount(S);
    //        // indel distance = #deletes + #inserts = m + n − 2·lcs
    //        return m + n - 2 * lcs;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    public static int PopCount(ulong value)
    //    {

    //#if NET6_0_OR_GREATER
    //        return BitOperations.PopCount(value);
    //#else
    //        value -= value >> 1 & 6148914691236517205UL /*0x5555555555555555*/;
    //        value = (ulong)(((long)value & 3689348814741910323L /*0x3333333333333333*/) + ((long)(value >> 2) & 3689348814741910323L /*0x3333333333333333*/));
    //        value = (ulong)(((long)value + (long)(value >> 4) & 1085102592571150095L) * 72340172838076673L >>> 56);
    //        return (int)value;
    //#endif
    //    }

    //    /// <summary>
    //    /// Bit-parallel indel distance (insertions+deletions only) for ReadOnlySpan<char>
    //    /// of *any* length, using a multi-word LCS.
    //    /// </summary>
    //    public static int DistanceMultipleMachineWords<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2, int? scoreCutoff) where T : IEquatable<T>
    //    {
    //        int m = s1.Length;
    //        int n = s2.Length;

    //        // number of 64-bit blocks needed
    //        int blocks = (m + 63) >> 6;

    //        // build per-char bitmasks
    //        var mask = new Dictionary<T, ulong[]>(EqualityComparer<T>.Default);
    //        for (int i = 0; i < m; i++)
    //        {
    //            T c = s1[i];
    //            int b = i >> 6, off = i & 63;
    //            if (!mask.TryGetValue(c, out var arr))
    //                mask[c] = arr = new ulong[blocks];
    //            arr[b] |= 1UL << off;
    //        }
    //        var zeroMask = new ulong[blocks];

    //        var S = new ulong[blocks];
    //        var X = new ulong[blocks];
    //        var Y = new ulong[blocks];
    //        var D = new ulong[blocks];

    //        // process each char of s2
    //        for (int j = 0; j < n; j++)
    //        {
    //            // 1) load mask or zero
    //            var M = mask.GetValueOrDefault(s2[j], zeroMask);

    //            // 2) X = M|S, Y = (S<<1)|1 with carry
    //            ulong carryY = 1;
    //            for (int b = 0; b < blocks; b++)
    //            {
    //                X[b] = M[b] | S[b];
    //                Y[b] = (S[b] << 1) | carryY;
    //                carryY = S[b] >> 63;
    //            }

    //            // 3) D = X - Y (multi-word subtraction with borrow)
    //            ulong borrow = 0;
    //            for (int b = 0; b < blocks; b++)
    //            {
    //                ulong x = X[b], y = Y[b];
    //                ulong t = x - y;
    //                ulong b1 = (t > x) ? 1UL : 0UL;
    //                ulong r = t - borrow;
    //                ulong b2 = (r > t) ? 1UL : 0UL;
    //                D[b] = r;
    //                borrow = b1 | b2;
    //            }

    //            // 4) S = X & (X ^ D)
    //            for (int b = 0; b < blocks; b++)
    //                S[b] = X[b] & (X[b] ^ D[b]);

    //            // early cutoff check
    //            if (scoreCutoff.HasValue)
    //            {
    //                // current matches so far
    //                int matched = 0;
    //                for (int b = 0; b < blocks; b++)
    //                    matched += PopCount(S[b]);

    //                int processed = j + 1;
    //                int nRemaining = n - processed;
    //                int possibleRem = Math.Min(m - matched, nRemaining);
    //                int maxLcs = matched + possibleRem;
    //                int minDist = m + n - 2 * maxLcs;

    //                if (minDist > scoreCutoff.Value)
    //                    return scoreCutoff.Value + 1;
    //            }
    //        }

    //        // final LCS = popcount(S)
    //        int lcs = 0;
    //        for (int b = 0; b < blocks; b++)
    //            lcs += PopCount(S[b]);

    //        int dist = m + n - 2 * lcs;

    //        return dist;
    //    }
}