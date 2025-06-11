using Raffinert.FuzzySharp.Utils;
using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp;

public static class IndelHyyro
{
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
        var dist = Distance(s1, s2, scoreCutoff);
        var normDist = maximum == 0 ? 0 : dist / (double)maximum;
        return normDist;
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

        var normDist = NormalizedDistance(s1, s2, null, scoreCutoff);
        var normSim = 1 - normDist;
        return normSim;
    }

    /// <summary>
    /// Computes the Levenshtein distance between two sequences using the Myers bit-parallel algorithm with a score cutoff.
    /// </summary>
    /// <typeparam name="T">Element type, must implement IEquatable&lt;T&gt;.</typeparam>
    /// <param name="source">Source sequence.</param>
    /// <param name="target">Target sequence.</param>
    /// <param name="scoreCutoff">Maximum allowed distance.</param>
    /// <returns>The Levenshtein distance, or scoreCutoff+1 if above cutoff.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Distance<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target, int? scoreCutoff = null) where T : IEquatable<T>
    {
        SequenceUtils.TrimCommonAffixAndSwapIfNeeded(ref source, ref target);

        if (source.Length <= 64)
        {
            return Distancex(source, target, scoreCutoff);
        }

        return BitParallelDistanceMultipleULongs(source, target, scoreCutoff);
    }

    private static int Distancex<T>(ReadOnlySpan<T> pattern,
        ReadOnlySpan<T> text,
        int? scoreCutoff = null)
        where T : IEquatable<T>
    {
        if (pattern.IsEmpty) return text.Length;
        if (pattern.Length > 63) throw new NotSupportedException("pattern > 63");

        int m = pattern.Length;
        ulong fullMask = (1UL << (m + 1)) - 1;   // bits 0 … m
        ulong hiBit = 1UL << m;               // sentinel (bit m)

        ulong Pv = (1UL << m) - 1;   // start “one insertion ahead” in every row
        ulong Nv = 0;
        int D = m;                // D[m,0]  = |pattern|

        foreach (var tj in text)
        {
            // 1. equality mask for the current text symbol
            ulong Eq = 0;
            for (int i = 0; i < m; ++i)
                if (pattern[i].Equals(tj)) Eq |= 1UL << i;

            // 2. lines (1)–(5) in the paper
            ulong Z = (((Eq & Pv) + Pv) ^ Pv) | Eq | Nv; Z &= fullMask;
            ulong Nh = Pv & Z;
            ulong X = Nv | ~(Pv | Z);
            ulong Y = (Pv - Nh) >> 1;
            ulong Ph = (X + Y) ^ Y; Ph &= fullMask;

            // 3. lines (6)–(7)
            ulong PhSh = (Ph << 1);
            Nv = PhSh & Z;
            Pv = (Nh << 1) | ~(PhSh | Z) | (PhSh & (Pv - Nh));
            Pv &= fullMask;                            // keep the sentinel!

            // 4. lines (8)–(9)  ——  ONLY place where D changes
            if ((Ph & hiBit) != 0) ++D;   // insertion completed
            if ((Nh & hiBit) != 0) --D;   // deletion completed

            // 5. optional early cut-off
            if (D - (text.Length - (tj.GetHashCode() & 0)) > scoreCutoff)
                return D;                 // cannot beat the threshold
        }

        return D;   // all pending edits already charged through the sentinel bit
    }

    public static int BitParallelDistanceSingleULong<T>(ReadOnlySpan<T> pattern,
                                      ReadOnlySpan<T> text,
                                      int? scoreCutoff = null)
        where T : IEquatable<T>
    {
        if (pattern.IsEmpty)
            return text.Length;               // nothing to match → all insertions
        if (pattern.Length > 63)
            throw new NotSupportedException("Single-word version supports pattern ≤ 63.");
        if (scoreCutoff < 0)
            throw new ArgumentOutOfRangeException(nameof(scoreCutoff));

        int m = pattern.Length;
        int n = text.Length;
        int min = Math.Abs(m - n);
        if (min > scoreCutoff)               // even the best-possible distance is too large
            return min;

        ulong fullMask = (1UL << (m + 1)) - 1;   // bits 0 … m
        ulong hiBit = 1UL << m;
        ulong M = hiBit - 1;           // mask for the active m bits
        ulong Pv = M;                        // +1 vertical differences  (all 1’s)
        ulong Nv = 0UL;                      // −1 vertical differences  (all 0’s)
        int D = m;                        // D[m,0]  – initial distance

        // ---- main scan ------------------------------------------------------
        for (int j = 0; j < n; ++j)
        {
            // 1. build the equality mask on the fly (no alphabet table):
            ulong Eq = 0;
            for (int i = 0; i < m; ++i)
                if (pattern[i].Equals(text[j]))
                    Eq |= 1UL << i;

            //------------------------------------------------------------------
            // 2. diagonal zero-difference vector (Myers’ trick, with Nv added)
            ulong Z = (((Eq & Pv) + Pv) ^ Pv) | Eq | Nv;              // Z_dj  (line 1)
            Z &= fullMask;

            // 3. horizontal −1 / +1 difference vectors
            ulong Nh = Pv & Z;                                 // line 2
            ulong Xh = Nv | ~(Pv | Z);                         // line 3
            ulong Yh = (Pv - Nh) >> 1;                         // line 4
            ulong Ph = ((Xh + Yh) ^ Yh) & fullMask;                   // line 5

            // 4. vertical −1 / +1 difference vectors
            Nv = (Ph << 1) & Z;                          // line 6
            Nv &= fullMask;
            Pv = (Nh << 1) | ~((Ph << 1) | Z) | ((Ph << 1) & (Pv - Nh));
            Pv &= fullMask;                                      // line 7

            //------------------------------------------------------------------
            // 5. update the current distance D[m,j]   (lines 8–9)
            if ((Ph & hiBit) != 0) ++D;     // insertion at last row
            if ((Nh & hiBit) != 0) --D;     // deletion  at last row

            //------------------------------------------------------------------
            // 6. early cut-off  (each remaining char can at best reduce D by 1)
            int optimistic = D - (n - j - 1);
            if (optimistic > scoreCutoff)
                return optimistic;

        }
        return D;
    }

    /// <summary>
    /// Indel distance (insertions + deletions; no substitutions).
    /// </summary>
    /// <remarks>
    ///   Distance = |P| + |T| − 2·LCS(P,T).  
    ///   Pattern length is limited to 63 so the two DP rows fit on the stack
    ///   (64 × 4 bytes = 256 B).  Lift this restriction by switching to a
    ///   rented array if you ever need longer patterns.
    /// </remarks>
    public static int BitParallelDistanceSingleULongx<T>(ReadOnlySpan<T> pattern,
                                  ReadOnlySpan<T> text,
                                  int? scoreCutoff = null)
        where T : IEquatable<T>
    {
        if (pattern.IsEmpty)
            throw new ArgumentException("Pattern must be non-empty.", nameof(pattern));
        if (pattern.Length > 63)
            throw new NotSupportedException("Pattern length > 63 not supported.");
        if (scoreCutoff < 0)
            throw new ArgumentOutOfRangeException(nameof(scoreCutoff));

        int m = pattern.Length;
        int n = text.Length;

        // Even a perfect overlap can’t beat |m–n|.
        int diff = Math.Abs(m - n);
        if (diff > scoreCutoff)
            return diff;

        // Rolling rows for Longest-Common-Subsequence DP (O(m·n)).
        Span<int> prev = stackalloc int[64];   // m ≤ 63 ⇒ row ≤ 64
        Span<int> curr = stackalloc int[64];

        int bestLcs = 0;

        for (int j = 0; j < n; ++j)
        {
            curr[0] = 0;

            for (int i = 1; i <= m; ++i)
            {
                if (pattern[i - 1].Equals(text[j]))
                    curr[i] = prev[i - 1] + 1;
                else
                    curr[i] = Math.Max(prev[i], curr[i - 1]);
            }

            curr.Slice(0, m + 1).CopyTo(prev);
            bestLcs = prev[m];

            // Early cut-off: the most extra matches we can still make
            // equal the shorter of the remaining tails.
            int maxFutureLcs = bestLcs + Math.Min(m - bestLcs,
                                                  n - j - 1);
            int optimisticDistance = m + n - 2 * maxFutureLcs;
            if (optimisticDistance > scoreCutoff)
                return optimisticDistance;        // guaranteed above cut-off
        }

        return m + n - 2 * bestLcs;
    }

    public static int BitParallelDistanceSingleULong0<T>(ReadOnlySpan<T> pattern,
                                       ReadOnlySpan<T> text,
                                       int? scoreCutoff = null)
            where T : IEquatable<T>
    {
        if (pattern.IsEmpty)
            throw new ArgumentException("Pattern must be non‑empty.", nameof(pattern));
        if (pattern.Length > 63)
            throw new NotSupportedException("Pattern length > 63 not supported in this single‑word variant.");
        if (scoreCutoff < 0)
            throw new ArgumentOutOfRangeException(nameof(scoreCutoff), "Cutoff must be non‑negative.");

        int m = pattern.Length;
        int n = text.Length;
        int lenDiff = Math.Abs(m - n);

        // Quick lower‑bound test: the empty‑edit lower bound is the
        // length difference.  If even that exceeds the cutoff we can
        // return immediately.
        if (lenDiff > scoreCutoff)
            return lenDiff; // already guaranteed to be > cutoff

        ulong bitMask = m == 64 ? ulong.MaxValue : (1UL << m) - 1UL;
        ulong highBit = 1UL << (m - 1);

        ulong Pv = bitMask; // P v_0 = 1^m
        ulong Nv = 0UL;     // N v_0 = 0^m
        int dist = m;     // D[m,0] = m (all deletions)

        for (int j = 0; j < n; j++)
        {
            // Build equality mask Eq for text[j]
            ulong Eq = 0UL;
            T tj = text[j];
            for (int i = 0; i < m; i++)
            {
                if (pattern[i].Equals(tj))
                    Eq |= 1UL << i;
            }

            // (1) Z d_j
            ulong Zd = (((Eq & Pv) + Pv) ^ Pv) | Eq | Nv;
            Zd &= bitMask;

            // (2) N h_j
            ulong Nh = Pv & Zd;

            // (3) X
            ulong X = Nv | (~(Pv | Zd) & bitMask);

            // (4) Y
            ulong Y = (Pv - Nh) >> 1;

            // (5) P h_j
            ulong Ph = (X + Y) ^ Y;
            Ph &= bitMask;

            // (6) N v_j
            ulong NvNew = (Ph << 1) & Zd & bitMask;

            // (7) P v_j
            ulong shiftedPh = (Ph << 1) & bitMask;
            ulong PvNew = ((Nh << 1)
                           | (~(shiftedPh | Zd) & bitMask)
                           | (shiftedPh & (Pv - Nh)))
                          & bitMask;

            // (8) Distance bookkeeping
            if ((Ph & highBit) != 0) dist++;
            if ((Nh & highBit) != 0) dist--;

            // Early‑exit heuristic: the distance can drop by at most one
            // per remaining character, therefore
            //   minPossible = Max(dist - rem, |m - n|).
            if (scoreCutoff.HasValue)
            {
                int rem = n - j - 1;
                int minPossible = Math.Max(dist - rem, lenDiff);
                if (minPossible > scoreCutoff)
                    return minPossible; // > cutoff ⇒ caller can treat as failure
            }

            // (9) roll state
            Pv = PvNew;
            Nv = NvNew;
        }

        return dist;
    }

    //public static int BitParallelDistanceSingleULong<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target, int? scoreCutoff = null)
    //    where T : IEquatable<T>
    //{
    //    int m = source.Length;
    //    int n = target.Length;

    //    if (m == 0) return n;
    //    if (n == 0) return m;
    //    if (m > 64) throw new ArgumentException("Source length must be ≤ 64 for bit-parallel implementation.");

    //    // Preprocess: build bitmasks
    //    var peq = new Dictionary<T, ulong>();
    //    ulong bit = 1;
    //    for (int i = 0; i < m; i++, bit <<= 1)
    //    {
    //        var c = source[i];
    //        if (!peq.TryGetValue(c, out var mask))
    //            mask = 0;
    //        mask |= (1UL << i);
    //        peq[c] = mask;
    //    }

    //    ulong VP = ~0UL; // All 1s
    //    ulong VN = 0UL;
    //    int D = m;       // Distance starts as length of source (full deletion)

    //    ulong highestBit = 1UL << (m - 1);

    //    for (int i = 0; i < n; i++)
    //    {
    //        T tc = target[i];
    //        ulong PM = peq.TryGetValue(tc, out var pm) ? pm : 0;

    //        ulong X = PM | VN;
    //        ulong D0 = (((X & VP) + VP) ^ VP) | X;

    //        ulong HP = VN | ~(D0 | VP);
    //        ulong HN = VP & D0;

    //        ulong shiftHP = (HP << 1) | 1UL;

    //        VP = (HN << 1) | ~(D0 | shiftHP);
    //        VN = shiftHP & D0;

    //        // Update distance
    //        if ((HP & highestBit) != 0)
    //            D++;
    //        else if ((HN & highestBit) != 0)
    //            D--;

    //        if (scoreCutoff.HasValue && D > scoreCutoff.Value)
    //                    return scoreCutoff.Value + 1;
    //    }

    //    return D;
    //}
    //public static int BitParallelDistanceSingleULong<T>(
    //    ReadOnlySpan<T> source,
    //    ReadOnlySpan<T> target,
    //    int? scoreCutoff
    //) where T : IEquatable<T>
    //{
    //    int m = source.Length;
    //    int n = target.Length;

    //    if (m == 0)
    //    {
    //        int d = n;
    //        return (scoreCutoff.HasValue && d > scoreCutoff.Value)
    //            ? scoreCutoff.Value + 1
    //            : d;
    //    }

    //    // build mask on the fly
    //    ulong VP = ~0UL;
    //    ulong VN = 0UL;
    //    int dist = m;
    //    ulong highestBit = 1UL << (m - 1);

    //    foreach (var c in target)
    //    {
    //        // 1) pattern mask for this character
    //        ulong PM = 0;
    //        for (var i = 0; i < m; i++)
    //        {
    //            if (EqualityComparer<T>.Default.Equals(source[i], c))
    //                PM |= 1UL << i;
    //        }

    //        // 2) core bit‐parallel operations
    //        ulong X = PM | VN;
    //        ulong D0 = (((X & VP) + VP) ^ VP) | X;
    //        ulong HP = VN | ~(D0 | VP);
    //        ulong HN = VP & D0;

    //        // 3) update distance from the top bit
    //        if ((HP & highestBit) != 0UL) dist++;
    //        if ((HN & highestBit) != 0UL) dist--;
    //        if (scoreCutoff.HasValue && dist > scoreCutoff.Value)
    //            return scoreCutoff.Value + 1;

    //        // 4) D0
    //        ulong X2 = (D0 << 1) | 1UL;
    //        VN = X2 & HP;
    //        VP = HN | ~(X2 | HP);
    //    }

    //    return dist;
    //}

    //public static int BitParallelDistanceSingleULong<T>(
    //    ReadOnlySpan<T> pattern,
    //    ReadOnlySpan<T> text,
    //    int? scoreCutoff
    //) where T : IEquatable<T>
    //{
    //    int m = pattern.Length;
    //    int n = text.Length;
    //    if (m == 0)
    //        return n;

    //    // Preprocessing: build match masks
    //    //var M = new Dictionary<char, ulong>();
    //    //for (int i = 0; i < m; i++)
    //    //{
    //    //    T c = pattern[i];
    //    //    if (!M.ContainsKey(c))
    //    //        M[c] = 0ul;
    //    //    M[c] |= 1ul << i;
    //    //}

    //    ulong Pv = ~0ul;               // all 1s
    //    ulong Nv = 0ul;                // all 0s
    //    int currDist = m;

    //    for (int j = 0; j < n; j++)
    //    {
    //        ulong charMask = 0ul;
    //        for (var i = 0; i < m; i++)
    //        {
    //            if (EqualityComparer<T>.Default.Equals(text[j], pattern[i]))
    //                charMask |= 1UL << i;
    //        }
    //        // Step 1: compute Z_d
    //        //ulong charMask = M.ContainsKey(text[j]) ? M[text[j]] : 0ul;
    //        ulong X = (charMask & Pv) + Pv;
    //        ulong Z = (X ^ Pv) | charMask | Nv;

    //        // Step 2: compute N_h
    //        ulong Nh = Pv & Z;

    //        // Prepare for P_h computation
    //        ulong PvPrev = Pv;
    //        ulong NvPrev = Nv;

    //        // Step 3,4: compute X and Y for Ph
    //        // X = NvPrev | ~(PvPrev | Z)
    //        ulong Xh = NvPrev | ~(PvPrev | Z);
    //        // Y = (PvPrev - Nh) >> 1
    //        ulong Yh = (PvPrev - Nh) >> 1;

    //        // Step 5: Ph = (Xh + Yh) & Yh
    //        ulong Ph = (Xh + Yh) & Yh;

    //        // Step 6: N_v
    //        ulong NvNew = (Ph << 1) & Z;

    //        // Step 7: P_v
    //        ulong PvNew = ((Nh << 1) | ~((Ph << 1) | Z))
    //                      | ((Ph << 1) & (PvPrev - Nh));

    //        Pv = PvNew;
    //        Nv = NvNew;

    //        // Update current distance
    //        if ((Ph & (1ul << (m - 1))) != 0)
    //            currDist++;
    //        if ((Nh & (1ul << (m - 1))) != 0)
    //            currDist--;

    //        if (scoreCutoff.HasValue && currDist > scoreCutoff.Value)
    //            return scoreCutoff.Value + 1;
    //    }

    //    return currDist;
    //}

    //private static int BitParallelDistanceSingleULong<T>(
    //    ReadOnlySpan<T> source,
    //    ReadOnlySpan<T> target,
    //    int? scoreCutoff
    //) where T : IEquatable<T>
    //{
    //    int m = source.Length;
    //    if (m == 0) return target.Length;
    //    //if (m > 64)
    //    //    throw new ArgumentException("Pattern length must be <= 64 for bit-parallel algorithm.");

    //    // Preprocessing: build Peq table
    //    //var Peq = new Dictionary<char, ulong>();
    //    //for (int i = 0; i < m; i++)
    //    //{
    //    //    char c = pattern[i];
    //    //    if (!Peq.ContainsKey(c)) Peq[c] = 0UL;
    //    //    Peq[c] |= 1UL << i;
    //    //}

    //    ulong Pv = ~0UL;    // All 1s
    //    ulong Mv = 0UL;     // All 0s
    //    int Score = m;
    //    ulong Xv, Xh, Ph, Mh;

    //    foreach (T t in target)
    //    {
    //        // 1) pattern mask for this character
    //        ulong Eq = 0;
    //        for (var i = 0; i < m; i++)
    //        {
    //            if (EqualityComparer<T>.Default.Equals(source[i], t))
    //                Eq |= 1UL << i;
    //        }

    //        Xv = Eq | Mv;
    //        Xh = (((Eq & Pv) + Pv) ^ Pv) | Eq;
    //        Ph = Mv | ~(Xh | Pv);
    //        Mh = Pv & Xh;

    //        // Update score based on highest bit (position m-1)
    //        if (((Ph >> (m - 1)) & 1) != 0)
    //            Score++;
    //        else if (((Mh >> (m - 1)) & 1) != 0)
    //            Score--;

    //        if (scoreCutoff.HasValue && Score > scoreCutoff.Value)
    //            return scoreCutoff.Value + 1;

    //        // Shift and update Pv, Mv
    //        Ph = (Ph << 1) | 1UL;
    //        Mh = (Mh << 1);
    //        Pv = Mh | ~(Xv | Ph);
    //        Mv = Ph & Xv;
    //    }

    //    return Score;
    //}




    /// <summary>
    /// Returns the minimal number of insertions+deletions
    /// to transform s1 into s2.
    /// Uses a bit-parallel LCS for |s1| ≤ 64.
    /// </summary>
    //private static int BitParallelDistanceSingleULong<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2, int? scoreCutoff) where T : IEquatable<T>
    //{
    //    int m = s1.Length;
    //    int n = s2.Length;
    //    if (m == 0) return n;

    //    // Build per-char mask for s1 (positions 0..m-1 mapped into bits 0..m-1)
    //    var mask = new Dictionary<T, ulong>(EqualityComparer<T>.Default);
    //    for (int i = 0; i < m; i++)
    //    {
    //        ulong bit = 1UL << i;
    //        T c = s1[i];
    //        var cur = mask.GetValueOrDefault(c, 0UL);
    //        mask[c] = cur | bit;
    //    }

    //    // S is our accumulator (initially 0)
    //    ulong S = 0UL;

    //    for (int j = 0; j < n; j++)
    //    {
    //        // 1) load mask or zero
    //        mask.TryGetValue(s2[j], out ulong M);

    //        // 2) X = M|S, Y = (S<<1)|1
    //        ulong X = M | S;
    //        ulong Y = (S << 1) | 1UL;

    //        // 3) D = X - Y  (single-word, borrow discarded)
    //        ulong D = X - Y;

    //        // 4) update S = X & (X ^ D)
    //        S = X & (X ^ D);

    //        // early cutoff?
    //        if (scoreCutoff.HasValue)
    //        {
    //            int matched = NumericsPolyfill.PopCount(S);
    //            int processed = j + 1;
    //            int nRem = n - processed;
    //            // at most we can match all remaining in the shorter of the two
    //            int maxLcs = matched + Math.Min(m - matched, nRem);
    //            int minDist = m + n - 2 * maxLcs;
    //            if (minDist > scoreCutoff.Value)
    //                return scoreCutoff.Value + 1;
    //        }
    //    }

    //    int lcs = NumericsPolyfill.PopCount(S);
    //    // indel distance = #deletes + #inserts = m + n − 2·lcs
    //    return m + n - 2 * lcs;
    //}

    private const int STACKALLOC_THRESHOLD_ULONGS = 1024 * 8;

    public static int BitParallelDistanceMultipleULongs<T>(
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        int? scoreCutoff
    ) where T : IEquatable<T>
    {
        int m = s1.Length;
        int n = s2.Length;

        if (m == 0)
        {
            int d = n;
            return (scoreCutoff.HasValue && d > scoreCutoff.Value)
                ? scoreCutoff.Value + 1
                : d;
        }

        int blocks = (m + 63) >> 6;
        var pool = ArrayPool<ulong>.Shared;

        // Build the per-character bitmasks
        using var charMask = new CharMaskBuffer<T>(64, blocks, pool);
        for (int i = 0; i < m; i++)
            charMask.AddBit(s1[i], i);

        int totalScratch = 6 * blocks;
        int result;

        if (totalScratch <= STACKALLOC_THRESHOLD_ULONGS)
        {
            // all on the stack
            Span<ulong> scratch = stackalloc ulong[totalScratch];
            result = IndelComputeWithDictionary(s2, scoreCutoff, m, blocks, charMask, scratch);
        }
        else
        {
            // rent one big array for all six lanes + zeroMask
            var scratchArray = pool.Rent(totalScratch);
            try
            {
                result = IndelComputeWithDictionary(s2, scoreCutoff, m, blocks, charMask, scratchArray);
            }
            finally
            {
                pool.Return(scratchArray);
            }
        }

        return result;
    }

    private static int IndelComputeWithDictionary<T>(
        ReadOnlySpan<T> target,
        int? scoreCutoff,
        int m,
        int blocks,
        CharMaskBuffer<T> charMask,
        Span<ulong> scratch
    ) where T : IEquatable<T>
    {
        // Partition scratch into six lanes + zeroMask
        var VP = scratch.Slice(0 * blocks, blocks);
        var VN = scratch.Slice(1 * blocks, blocks);
        var X = scratch.Slice(2 * blocks, blocks);
        var D = scratch.Slice(3 * blocks, blocks);

        // Clear what needs clearing
        VP.Clear();
        VN.Clear();

        int last = blocks - 1;
        ulong highestBitMask = 1UL << ((m - 1) & 63);
        int dist = m;

        foreach (var c2 in target)
        {
            var M = charMask.GetOrZero(c2);

            // X = M | S  (we reuse VN for “previous S” here)
            // actually: VN holds the prior S
            for (int b = 0; b < blocks; b++)
            {
                X[b] = M[b] | VN[b];
            }

            // compute D = X - ((VN<<1)|1) with multi-word borrow
            ulong borrow = 1;  // we blend the “|1” into the first subtract
            for (int b = 0; b < blocks; b++)
            {
                ulong x = X[b];
                ulong y = (VN[b] << 1) | borrow;
                borrow = (x < y) ? 1UL : 0UL;
                D[b] = x - y;
            }

            // New S in VP: bits where X & ~D
            for (int b = 0; b < blocks; b++)
            {
                VP[b] = X[b] & ~D[b];
            }

            // update distance from high bit of last block
            if ((VP[last] & highestBitMask) != 0UL) dist++;
            if (scoreCutoff.HasValue && dist > scoreCutoff.Value)
                return scoreCutoff.Value + 1;

            // shift S left by 1 into VN (for next iteration)
            ulong carry = 1;
            for (int b = 0; b < blocks; b++)
            {
                ulong s = VP[b];
                ulong nextCarry = s >> 63;
                VN[b] = (s << 1) | carry;
                carry = nextCarry;
            }
        }

        return dist;
    }
}