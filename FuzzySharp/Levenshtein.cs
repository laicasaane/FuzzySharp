using Raffinert.FuzzySharp.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Raffinert.FuzzySharp.Edits;

namespace Raffinert.FuzzySharp;

public static class Levenshtein
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int LevenshteinMaximum(int len1, int len2, int insertCost, int deleteCost, int replaceCost)
    {
        // Cost of deleting all of s1 then inserting all of s2
        int totalDelIns = len1 * deleteCost + len2 * insertCost;

        // Cost of replacing common prefix and handling extra characters
        int common = len1 < len2 ? len1 : len2;
        int extraCost = len1 >= len2
            ? len1 - len2 * deleteCost
            : len2 - len1 * insertCost;

        int replaceAndDiff = common * replaceCost + extraCost;

        // Return the smaller of the two worst-case scenarios
        return totalDelIns < replaceAndDiff ? totalDelIns : replaceAndDiff;
    }

    /// <summary>
    /// Generic DP (one-row) implementation.
    /// </summary>
    private static int GenericDistance<T>(
        ReadOnlySpan<T> source, ReadOnlySpan<T> target,
        int insertCost, int deleteCost, int replaceCost,
        int? scoreCutoff) where T : IEquatable<T>
    {
        var len1 = source.Length;
        // allocate a single row of len1+1
        Span<int> row = new int[len1 + 1];
        // initial: cost of deleting all of s1's prefix
        for (var i = 0; i <= len1; i++)
            row[i] = i * deleteCost;

        foreach (var c2 in target)
        {
            var prev = row[0];
            row[0] += insertCost;
            for (var i = 0; i < len1; i++)
            {
                var curr = row[i + 1];
                var cost = prev;
                if (!EqualityComparer<T>.Default.Equals(source[i], c2))
                {
                    var del = row[i] + deleteCost;
                    var ins = row[i + 1] + insertCost;
                    var rep = prev + replaceCost;
                    cost = del < ins
                        ? del < rep ? del : rep
                        : ins < rep ? ins : rep;
                }
                prev = curr;
                row[i + 1] = cost;
            }
            if (scoreCutoff.HasValue && row[len1] > scoreCutoff.Value)
                return scoreCutoff.Value + 1;
        }
        return row[len1];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FastDistance<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target) where T : IEquatable<T>
    {
        SequenceUtils.SwapIfSourceIsLonger(ref source, ref target);

        if (source.Length <= 64)
        {
            return MyersDistanceSingleMachineWord(source, target);
        }

        SequenceUtils.TrimAndSwapIfNeeded(ref source, ref target);

        if (source.Length <= 64)
        {
            return MyersDistanceSingleMachineWord(source, target);
        }

        return MyersDistanceMultipleMachineWords(source, target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FastDistance<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target, int scoreCutoff) where T : IEquatable<T>
    {
        SequenceUtils.SwapIfSourceIsLonger(ref source, ref target);

        if (source.Length <= 64)
        {
            return MyersDistanceSingleMachineWord(source, target, scoreCutoff);
        }

        SequenceUtils.TrimAndSwapIfNeeded(ref source, ref target);

        if (source.Length <= 64)
        {
            return MyersDistanceSingleMachineWord(source, target, scoreCutoff);
        }

        return MyersDistanceMultipleMachineWords(source, target, scoreCutoff);
    }

    /// <summary>
    /// Bit-parallel Myers algorithm for unit weights (insert=delete=replace=1), 
    /// zero-alloc.
    /// </summary>
    private static int MyersDistanceSingleMachineWord<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target, int scoreCutoff) where T : IEquatable<T>
    {
        var m = source.Length;
        if (m == 0) return target.Length;

        // initial bitmask: lower m bits set
        var VP = m < 64 ? (1UL << m) - 1 : ulong.MaxValue;
        ulong VN = 0;
        var highestBit = 1UL << (m - 1);
        var dist = m;

        foreach (var c2 in target)
        {
            // build mask for c2: bits set where s1[i] == c2
            ulong PM = 0;
            for (var i = 0; i < m; i++)
            {
                if (EqualityComparer<T>.Default.Equals(source[i], c2))
                    PM |= 1UL << i;
            }

            // Myers bit-parallel update
            var X = PM | VN;
            var D0 = (((X & VP) + VP) ^ VP) | X;
            D0 |= VN;
            var HP = VN | ~(D0 | VP);
            var HN = D0 & VP;

            if ((HP & highestBit) != 0) dist++;
            if ((HN & highestBit) != 0) dist--;

            if (dist > scoreCutoff)
                return scoreCutoff + 1;

            // shift in
            HP = (HP << 1) | 1;
            HN <<= 1;
            VP = HN | ~(D0 | HP);
            VN = HP & D0;
        }

        return dist;
    }

    private static int MyersDistanceSingleMachineWord<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target) where T : IEquatable<T>
    {
        var m = source.Length;
        if (m == 0) return target.Length;

        // initial bitmask: lower m bits set
        var VP = m < 64 ? (1UL << m) - 1 : ulong.MaxValue;
        ulong VN = 0;
        var highestBit = 1UL << (m - 1);
        var dist = m;

        foreach (var c2 in target)
        {
            // build mask for c2: bits set where s1[i] == c2
            ulong PM = 0;
            for (var i = 0; i < m; i++)
            {
                if (EqualityComparer<T>.Default.Equals(source[i], c2))
                    PM |= 1UL << i;
            }

            // Myers bit-parallel update
            var X = PM | VN;
            var D0 = (((X & VP) + VP) ^ VP) | X;
            D0 |= VN;
            var HP = VN | ~(D0 | VP);
            var HN = D0 & VP;

            if ((HP & highestBit) != 0) dist++;
            if ((HN & highestBit) != 0) dist--;

            // shift in
            HP = (HP << 1) | 1;
            HN <<= 1;
            VP = HN | ~(D0 | HP);
            VN = HP & D0;
        }

        return dist;
    }

    private static int MyersDistanceMultipleMachineWords<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target, int? scoreCutoff) where T : IEquatable<T>
    {
        var m = source.Length;
        //var n = target.Length;
        //if (m == 0) return n;
        //if (n == 0) return m;

        // number of 64-bit blocks needed to cover pattern length m
        var blocks = (m + 63) >> 6;

        // build per-character bitmasks over those blocks
        var charMask = new Dictionary<T, ulong[]>();
        for (var i = 0; i < m; i++)
        {
            var c = source[i];
            var b = i >> 6;
            var offset = i & 63;

            if (!charMask.TryGetValue(c, out var maskArr))
                charMask[c] = maskArr = new ulong[blocks];

            maskArr[b] |= 1UL << offset;
        }
        // a zero-mask for characters not in s1
        var zeroMask = new ulong[blocks];

        // VP/VN state arrays, one ulong per block
        var VP = new ulong[blocks];
        var VN = new ulong[blocks];

        // initialize VP so that the low m bits are 1
        for (var b = 0; b < blocks; b++)
        {
            if (b < blocks - 1)
                VP[b] = ulong.MaxValue;
            else
            {
                var rem = m - ((blocks - 1) << 6);
                VP[b] = rem == 64 ? ulong.MaxValue : (1UL << rem) - 1;
            }
            VN[b] = 0;
        }

        var last = blocks - 1;
        // mask for the “highest” bit of the entire pattern (for score update)
        var highestBitMask = 1UL << ((m - 1) & 63);

        var dist = m;

        // per-iteration working arrays
        var X = new ulong[blocks];
        var D0 = new ulong[blocks];
        var HP = new ulong[blocks];
        var HN = new ulong[blocks];

        foreach (var c2 in target)
        {
            // grab the precomputed mask for this text char
            var PMitem = charMask.GetValueOrDefault(c2, zeroMask);

            // ========== Myers’s “D0” loop, but with carry across blocks ==========
            ulong carry = 0;
            for (var b = 0; b < blocks; b++)
            {
                var pm = PMitem[b];
                var vp = VP[b];
                var vn = VN[b];

                // ordinary bit-parallel ops
                var x = pm | vn;
                X[b] = x;
                var tmp = x & vp;

                // do tmp + vp + carry, detecting overflow
                var sum1 = tmp + vp;
                var c1 = sum1 < tmp ? 1UL : 0UL;
                var sum = sum1 + carry;
                var c2o = sum < sum1 ? 1UL : 0UL;
                carry = c1 | c2o;

                // D0 = ((tmp + vp + carry) ^ vp) | x
                var d0 = (sum ^ vp) | x;
                D0[b] = d0;

                // HP/HN before shifting
                HP[b] = vn | ~(d0 | vp);
                HN[b] = d0 & vp;
            }

            // update the current edit distance by inspecting the top bit of the last block
            if ((HP[last] & highestBitMask) != 0) dist++;
            if ((HN[last] & highestBitMask) != 0) dist--;
            if (dist > scoreCutoff)
                return scoreCutoff.Value + 1;

            // ========== shift HP/HN left by 1 across blocks, then compute new VP/VN ==========
            ulong carryHP = 1, carryHN = 0;
            for (var b = 0; b < blocks; b++)
            {
                var hp = HP[b];
                var hn = HN[b];

                // capture the bit that will spill into the next block
                var hpHigh = hp >> 63;
                var hnHigh = hn >> 63;

                // shift in the carry bits
                hp = (hp << 1) | carryHP;
                hn = (hn << 1) | carryHN;

                // update VP/VN for next round
                var d0 = D0[b];
                VP[b] = hn | ~(d0 | hp);
                VN[b] = hp & d0;

                carryHP = hpHigh;
                carryHN = hnHigh;
            }
        }

        return dist;
    }

    private static int MyersDistanceMultipleMachineWords<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> target) where T : IEquatable<T>
    {
        var m = source.Length;

        // number of 64-bit blocks needed to cover pattern length m
        var blocks = (m + 63) >> 6;

        // build per-character bitmasks over those blocks
        var charMask = new Dictionary<T, ulong[]>();
        for (var i = 0; i < m; i++)
        {
            var c = source[i];
            var b = i >> 6;
            var offset = i & 63;

            if (!charMask.TryGetValue(c, out var maskArr))
                charMask[c] = maskArr = new ulong[blocks];

            maskArr[b] |= 1UL << offset;
        }
        // a zero-mask for characters not in s1
        var zeroMask = new ulong[blocks];

        // VP/VN state arrays, one ulong per block
        var VP = new ulong[blocks];
        var VN = new ulong[blocks];

        // initialize VP so that the low m bits are 1
        for (var b = 0; b < blocks; b++)
        {
            if (b < blocks - 1)
                VP[b] = ulong.MaxValue;
            else
            {
                var rem = m - ((blocks - 1) << 6);
                VP[b] = rem == 64 ? ulong.MaxValue : (1UL << rem) - 1;
            }
            VN[b] = 0;
        }

        var last = blocks - 1;
        // mask for the “highest” bit of the entire pattern (for score update)
        var highestBitMask = 1UL << ((m - 1) & 63);

        var dist = m;

        // per-iteration working arrays
        var X = new ulong[blocks];
        var D0 = new ulong[blocks];
        var HP = new ulong[blocks];
        var HN = new ulong[blocks];

        foreach (var c2 in target)
        {
            // grab the precomputed mask for this text char
            var PMitem = charMask.GetValueOrDefault(c2, zeroMask);

            // ========== Myers’s “D0” loop, but with carry across blocks ==========
            ulong carry = 0;
            for (var b = 0; b < blocks; b++)
            {
                var pm = PMitem[b];
                var vp = VP[b];
                var vn = VN[b];

                // ordinary bit-parallel ops
                var x = pm | vn;
                X[b] = x;
                var tmp = x & vp;

                // do tmp + vp + carry, detecting overflow
                var sum1 = tmp + vp;
                var c1 = sum1 < tmp ? 1UL : 0UL;
                var sum = sum1 + carry;
                var c2o = sum < sum1 ? 1UL : 0UL;
                carry = c1 | c2o;

                // D0 = ((tmp + vp + carry) ^ vp) | x
                var d0 = (sum ^ vp) | x;
                D0[b] = d0;

                // HP/HN before shifting
                HP[b] = vn | ~(d0 | vp);
                HN[b] = d0 & vp;
            }

            // update the current edit distance by inspecting the top bit of the last block
            if ((HP[last] & highestBitMask) != 0) dist++;
            if ((HN[last] & highestBitMask) != 0) dist--;

            // ========== shift HP/HN left by 1 across blocks, then compute new VP/VN ==========
            ulong carryHP = 1, carryHN = 0;
            for (var b = 0; b < blocks; b++)
            {
                var hp = HP[b];
                var hn = HN[b];

                // capture the bit that will spill into the next block
                var hpHigh = hp >> 63;
                var hnHigh = hn >> 63;

                // shift in the carry bits
                hp = (hp << 1) | carryHP;
                hn = (hn << 1) | carryHN;

                // update VP/VN for next round
                var d0 = D0[b];
                VP[b] = hn | ~(d0 | hp);
                VN[b] = hp & d0;

                carryHP = hpHigh;
                carryHN = hnHigh;
            }
        }

        return dist;
    }

    /// <summary>
    /// Core distance entry point with custom weights and optional cutoff.
    /// </summary>
    public static int Distance<T>(
        ReadOnlySpan<T> source, ReadOnlySpan<T> target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        int? scoreCutoff = null) where T : IEquatable<T>
    {
        // unit-weight fast path
        if (insertCost == 1 && deleteCost == 1 && replaceCost == 1)
        {

            return scoreCutoff.HasValue
                ? FastDistance(source, target, scoreCutoff.Value)
                : FastDistance(source, target);
        }

        if (insertCost == 1 && deleteCost == 1 && replaceCost == 2)
        {
            return Indel.Distance(source, target); //, scoreCutoff);
        }

        SequenceUtils.TrimAndSwapIfNeeded(ref source, ref target);

        // otherwise generic
        return GenericDistance(source, target, insertCost, deleteCost, replaceCost, scoreCutoff);
    }



    /// <summary>
    /// Convenience overloads for strings.
    /// </summary>
    public static int Distance(
        string source, string target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        int? scoreCutoff = null)
        => Distance(source.AsSpan(), target.AsSpan(), insertCost, deleteCost, replaceCost, scoreCutoff);

    /// <summary>
    /// Similarity = maximum possible distance – actual distance.
    /// </summary>
    public static int Similarity(
        ReadOnlySpan<char> source, ReadOnlySpan<char> target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        int? scoreCutoff = null)
    {
        int len1 = source.Length, len2 = target.Length;
        var maximum = LevenshteinMaximum(len1, len2, insertCost, deleteCost, replaceCost);
        var dist = Distance(source, target, insertCost, deleteCost, replaceCost, scoreCutoff);
        var sim = maximum - dist;
        return sim < scoreCutoff ? 0 : sim;
    }

    public static int Similarity(
        string source, string s2,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        int? scoreCutoff = null)
        => Similarity(source.AsSpan(), s2.AsSpan(), insertCost, deleteCost, replaceCost, scoreCutoff);

    /// <summary>
    /// Normalized distance in [0,1].
    /// </summary>
    public static double NormalizedDistance(
        ReadOnlySpan<char> source, ReadOnlySpan<char> target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        double? scoreCutoff = null)
    {
        int len1 = source.Length, len2 = target.Length;
        if (len1 == 0 && len2 == 0) return 0.0;
        var maximum = LevenshteinMaximum(len1, len2, insertCost, deleteCost, replaceCost);
        if (maximum == 0) return 0.0;

        var dist = Distance(source, target, insertCost, deleteCost, replaceCost, scoreCutoff.HasValue ? (int?)Math.Floor(scoreCutoff.Value * maximum) : null);
        var nd = dist / (double)maximum;
        return nd > scoreCutoff ? 1.0 : nd;
    }

    public static double NormalizedDistance(
        string source, string target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        double? scoreCutoff = null)
        => NormalizedDistance(source.AsSpan(), target.AsSpan(), insertCost, deleteCost, replaceCost, scoreCutoff);

    /// <summary>
    /// Normalized similarity in [0,1] = 1 − normalized distance.
    /// </summary>
    public static double NormalizedSimilarity(
        ReadOnlySpan<char> source, ReadOnlySpan<char> target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        double? scoreCutoff = null)
    {
        var nd = NormalizedDistance(source, target, insertCost, deleteCost, replaceCost, scoreCutoff);
        var ns = 1.0 - nd;

        return ns < scoreCutoff ? 0.0 : ns;
    }

    public static double NormalizedSimilarity(
        string source, string target,
        int insertCost = 1, int deleteCost = 1, int replaceCost = 1,
        double? scoreCutoff = null)
        => NormalizedSimilarity(source.AsSpan(), target.AsSpan(), insertCost, deleteCost, replaceCost, scoreCutoff);


    public static double GetRatio<T>(T[] input1, T[] input2) where T : IEquatable<T>
    {
        int len1 = input1.Length;
        int len2 = input2.Length;
        int lensum = len1 + len2;

        int editDistance = Distance<T>(input1.AsSpan(), input2.AsSpan(), replaceCost:2);

        return editDistance == 0 ? 1 : (lensum - editDistance) / (double)lensum;
    }

    public static double GetRatio<T>(ReadOnlySpan<T> input1, ReadOnlySpan<T> input2) where T : IEquatable<T>
    {
        int len1 = input1.Length;
        int len2 = input2.Length;
        int lensum = len1 + len2;

        int editDistance = Distance(input1, input2, replaceCost: 2);

        return editDistance == 0 ? 1 : (lensum - editDistance) / (double)lensum;
    }

    // Special Case
    public static double GetRatio(string s1, string s2)
    {
        return GetRatio(s1.AsSpan(), s2.AsSpan());
    }

    public static EditOp[] GetEditOps(
        string s1,
        string s2,
        Func<string, string> processor = null,
        int? scoreHint = null
    )
    {
        if (processor != null)
        {
            s1 = processor(s1);
            s2 = processor(s2);
        }

        return GetEditOps(s1.AsSpan(), s2.AsSpan(), scoreHint: scoreHint);
    }

    /// <summary>
    /// High‐level dispatcher you just asked for: computes the edit‐ops
    /// by slicing off common prefixes/suffixes, running the bit-parallel
    /// matrix, then backtracking.
    /// </summary>
    /// <param name="s1">Source sequence.</param>
    /// <param name="s2">Destination sequence.</param>
    /// <param name="processor">Optional preprocessor (e.g. to normalize).</param>
    /// <param name="scoreHint">Ignored here—only used internally for dispatch in Python.</param>
    public static EditOp[] GetEditOps<T>(
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        Processor<T> processor = null,
        int? scoreHint = null
    ) where T : IEquatable<T>
    {
        // 1) Optional preprocessing
        if (processor != null)
        {
            processor(ref s1);
            processor(ref s2);
        }

        // 2) For strings, conv_sequences is identity
        //    (for more general sequences you'd map items to ints)
        // 3) Strip off common prefix+suffix
        var (prefixLen, suffixLen) = SequenceUtils.TrimIfNeeded(ref s1, ref s2);

        // 4) Run the bit-parallel matrix
        var (dist, VPblocks, VNblocks) = Matrix(s1, s2);

        // 5) Initialize backtracking
        int originalDist = dist;
        var opsArray = new EditOp[originalDist];
        int col = s1.Length;
        int row = s2.Length;
        int nextIndex = originalDist; // we’ll decrement before placing

        // 6) If no edits, we’re done
        if (originalDist == 0)
            return [];

        // 7) Backtrack
        while (row > 0 && col > 0)
        {
            // determine which block & bit offset holds the (col-1) bit
            int bitPos = col - 1;
            int blk = bitPos / 64;
            int off = bitPos % 64;
            ulong bit = 1UL << off;

            // deletion?
            if ((VPblocks[row - 1][blk] & bit) != 0)
            {
                nextIndex--;
                col--;
                opsArray[nextIndex] = new EditOp
                {
                    EditType = EditType.DELETE,
                    SourcePos = col + prefixLen,
                    DestPos = row + prefixLen
                };
            }
            else
            {
                row--;
                // insertion?
                if (row > 0 && (VNblocks[row - 1][blk] & bit) != 0)
                {
                    nextIndex--;
                    opsArray[nextIndex] = new EditOp
                    {
                        EditType = EditType.INSERT,
                        SourcePos = col + prefixLen,
                        DestPos = row + prefixLen
                    };
                }
                else
                {
                    // move diagonally
                    col--;
                    // replace?
                    if (!EqualityComparer<T>.Default.Equals(s1[col], s2[row]))
                    {
                        nextIndex--;
                        opsArray[nextIndex] = new EditOp
                        {
                            EditType = EditType.REPLACE,
                            SourcePos = col + prefixLen,
                            DestPos = row + prefixLen
                        };
                    }
                }
            }
        }

        // any remaining deletes
        while (col > 0)
        {
            nextIndex--;
            col--;
            opsArray[nextIndex] = new EditOp
            {
                EditType = EditType.DELETE,
                SourcePos = col + prefixLen,
                DestPos = row + prefixLen
            };
        }
        // any remaining inserts
        while (row > 0)
        {
            nextIndex--;
            row--;
            opsArray[nextIndex] = new EditOp
            {
                EditType = EditType.INSERT,
                SourcePos = col + prefixLen,
                DestPos = row + prefixLen
            };
        }

        return opsArray;
    }

    /// <summary>
    /// Dispatcher that picks the single‐word or multi‐word matrix.  
    /// Returns per‐character VP/VN block arrays.
    /// </summary>
    private static (int Distance, List<ulong[]> VP, List<ulong[]> VN) Matrix<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        return s1.Length <= 64 
            ? MatrixSingleMachineWord(s1, s2) 
            : MatrixMultipleMachineWords(s1, s2);
    }

    /// <summary>
    /// Computes the Myers bit-parallel VP/VN matrices and final edit distance using 64-bit words.
    /// </summary>
    /// <param name="s1">Pattern (must be ≤ 64 chars).</param>
    /// <param name="s2">Text.</param>
    /// <returns>
    /// (Distance, VP-list, VN-list)
    /// </returns>
    public static (int Distance, List<ulong[]> VP, List<ulong[]> VN) MatrixSingleMachineWord<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        if (s1.IsEmpty)
            return (s2.Length, [], []);

        if (s1.Length > 64)
            throw new ArgumentException("Pattern too long for 64-bit bit-parallel algorithm.", nameof(s1));

        // Initial bitmasks
        ulong VP = (1UL << s1.Length) - 1;
        ulong VN = 0;
        int currDist = s1.Length;
        ulong mask = 1UL << (s1.Length - 1);

        // Build the “block” table: for each character in s1, which bit(s) it sets
        var block = new Dictionary<T, ulong>();
        ulong bit = 1UL;
        foreach (T c in s1)
        {
            if (block.ContainsKey(c))
                block[c] |= bit;
            else
                block[c] = bit;
            bit <<= 1;
        }

        var matrixVP = new List<ulong[]>();
        var matrixVN = new List<ulong[]>();

        foreach (T c in s2)
        {
            block.TryGetValue(c, out ulong PMj);

            // Step 1: D0 = (((PMj & VP) + VP) ^ VP) | PMj | VN
            // Use unchecked so addition wraps modulo 2^64
            ulong X = PMj;
            ulong D0 = unchecked(((X & VP) + VP) ^ VP) | X | VN;

            // Step 2: HP = VN | ~(D0 | VP);  HN = D0 & VP
            ulong HP = VN | ~(D0 | VP);
            ulong HN = D0 & VP;

            // Step 3: adjust distance by looking at the high bit
            if ((HP & mask) != 0) currDist++;
            if ((HN & mask) != 0) currDist--;

            // Step 4: shift and recompute VP, VN
            HP = (HP << 1) | 1UL;
            HN <<= 1;
            VP = HN | ~(D0 | HP);
            VN = HP & D0;

            matrixVP.Add([VP]);
            matrixVN.Add([VN]);
        }

        return (currDist, matrixVP, matrixVN);
    }

    /// <summary>
    /// Computes the Myers bit‐parallel VP/VN matrices and final edit distance
    /// for an arbitrary‐length pattern by slicing into 64‐bit blocks.
    /// </summary>
    /// <param name="pattern">The pattern string (any length).</param>
    /// <param name="text">The text string to compare against.</param>
    /// <returns>
    /// A tuple containing:
    /// 1) the final edit distance,
    /// 2) the list of VP bit‐mask arrays (one array per text character),
    /// 3) the list of VN bit‐mask arrays (one array per text character).
    /// </returns>
    public static (int Distance, List<ulong[]> VP, List<ulong[]> VN) MatrixMultipleMachineWords<T>(ReadOnlySpan<T> pattern, ReadOnlySpan<T> text) where T : IEquatable<T>
    {
        int m = pattern.Length;
        if (m == 0)
            return (text.Length, new List<ulong[]>(), new List<ulong[]>());

        // Number of 64‐bit blocks needed to cover the pattern
        int blocks = (m + 63) / 64;

        // Initial VP = all 1s in those m bits, VN = 0
        var VP = new ulong[blocks];
        var VN = new ulong[blocks];
        for (int i = 0; i < blocks; i++)
        {
            // For all but the last block, fill with 0xFFFFFFFFFFFFFFFF
            // For the last block, only the low (m % 64) bits are 1
            if (i < blocks - 1 || m % 64 == 0)
                VP[i] = ulong.MaxValue;
            else
                VP[i] = (1UL << (m % 64)) - 1;
            VN[i] = 0UL;
        }

        // Mask to extract the highest‐order bit of the full m‐bit vector
        int lastBlk = blocks - 1;
        int topBitPos = (m - 1) % 64;
        ulong topBitMask = 1UL << topBitPos;

        // Build the “block” table: for each character, which bit(s) in each block it sets
        var blockTable = new Dictionary<T, ulong[]>();
        for (int j = 0; j < m; j++)
        {
            T c = pattern[j];
            int blk = j / 64, offset = j % 64;
            if (!blockTable.TryGetValue(c, out var arr))
            {
                arr = new ulong[blocks];
                blockTable[c] = arr;
            }
            arr[blk] |= 1UL << offset;
        }

        // A reusable zero‐mask for characters not in pattern
        var zeroMask = new ulong[blocks];

        int currDist = m;
        var matrixVP = new List<ulong[]>();
        var matrixVN = new List<ulong[]>();

        // Temporary arrays for per‐character computation
        var D0 = new ulong[blocks];
        var HP = new ulong[blocks];
        var HN = new ulong[blocks];
        var X = new ulong[blocks];
        var sum = new ulong[blocks];
        var HPs = new ulong[blocks];
        var HNs = new ulong[blocks];

        // Process each character of the text
        foreach (T c in text)
        {
            // 1) Load the pattern‐mask for c, or zeros if not present
            blockTable.TryGetValue(c, out X);
            if (X == null) X = zeroMask;

            // 2) Compute D0 = (((X & VP) + VP) ^ VP) | X | VN
            //    -> Must do a big‐integer add and carry across blocks
            ulong carry = 0;
            for (int b = 0; b < blocks; b++)
            {
                ulong Pv = VP[b];
                ulong XandVP = X[b] & Pv;
                // big‐integer add: XandVP + Pv + carry
                ulong t = unchecked(XandVP + Pv);
                ulong c1 = t < XandVP ? 1UL : 0UL;          // carry from first add
                ulong t2 = unchecked(t + carry);
                ulong c2 = t2 < carry ? 1UL : 0UL;           // carry from second add
                carry = c1 | c2;

                sum[b] = t2;
                D0[b] = (sum[b] ^ Pv) | X[b] | VN[b];
            }

            // 3) HP = VN | ~(D0 | VP),   HN = D0 & VP
            for (int b = 0; b < blocks; b++)
            {
                HP[b] = VN[b] | ~(D0[b] | VP[b]);
                HN[b] = D0[b] & VP[b];
            }

            // 4) Update distance by inspecting the highest bit
            if ((HP[lastBlk] & topBitMask) != 0) currDist++;
            if ((HN[lastBlk] & topBitMask) != 0) currDist--;

            // 5) Shift HP and HN left by one over the entire multi‐block vector
            //    and set the low bit of HP[0] to 1
            ulong carryHP = 1, carryHN = 0;
            for (int b = 0; b < blocks; b++)
            {
                ulong hpb = HP[b], hnb = HN[b];
                ulong newCarryHP = hpb >> 63;
                ulong newCarryHN = hnb >> 63;
                HPs[b] = (hpb << 1) | carryHP;
                HNs[b] = hnb << 1;
                carryHP = newCarryHP;
                carryHN = newCarryHN;
            }

            // 6) Recompute VP, VN
            for (int b = 0; b < blocks; b++)
            {
                VP[b] = HNs[b] | ~(D0[b] | HPs[b]);
                VN[b] = HPs[b] & D0[b];
            }

            // 7) Keep a snapshot of VP/VN for this character
            matrixVP.Add((ulong[])VP.Clone());
            matrixVN.Add((ulong[])VN.Clone());
        }

        return (currDist, matrixVP, matrixVN);
    }

    public static List<MatchingBlock> GetMatchingBlocks<T>(T[] s1, T[] s2) where T : IEquatable<T>
    {
        var editOps = GetEditOps(new ReadOnlySpan<T>(s1), new ReadOnlySpan<T>(s2));
        var matchingBlocks = editOps.AsMatchingBlocks(s1.Length, s2.Length);
        return matchingBlocks;
    }

    // Special Case
    public static List<MatchingBlock> GetMatchingBlocks(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    {
        var editOps = GetEditOps(s1, s2);
        var matchingBlocks = editOps.AsMatchingBlocks(s1.Length, s2.Length);
        return matchingBlocks;
    }

    //public static List<EditOp> GetEditOps(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    //{
    //    int m = s1.Length;
    //    int n = s2.Length;
    //    var editOps = new List<EditOp>();

    //    if (m == 0)
    //    {
    //        for (int j = 0; j < n; j++)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.INSERT, SourcePos = 0, DestPos = j });
    //        }

    //        return editOps;
    //    }
    //    if (n == 0)
    //    {
    //        for (int i = 0; i < m; i++)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.DELETE, SourcePos = i, DestPos = 0 });
    //        }

    //        return editOps;
    //    }

    //    // Only use bit-parallel for short strings
    //    if (m > 64)
    //    {
    //        // fallback to classic DP for long strings
    //        return GetEditOpsClassic(s1, s2);
    //    }

    //    // Build bit-parallel matrix
    //    ulong[] PM = new ulong[256]; // ASCII only, for simplicity
    //    for (int i = 0; i < 256; i++) PM[i] = 0;
    //    for (int i = 0; i < m; i++)
    //    {
    //        char c = s1[i];
    //        PM[c] |= 1UL << i;
    //    }

    //    int[,] matrix = new int[m + 1, n + 1];
    //    for (int i = 0; i <= m; i++) matrix[i, 0] = i;
    //    for (int j = 0; j <= n; j++) matrix[0, j] = j;

    //    ulong VP = (1UL << m) - 1;
    //    ulong VN = 0;
    //    int currDist = m;

    //    for (int j = 1; j <= n; j++)
    //    {
    //        ulong PM_j = s2[j - 1] < 256 ? PM[s2[j - 1]] : 0;
    //        ulong X = PM_j | VN;
    //        ulong D0 = (((X & VP) + VP) ^ VP) | X;
    //        ulong HP = VN | ~(D0 | VP);
    //        ulong HN = D0 & VP;

    //        if ((HP & (1UL << (m - 1))) != 0) currDist++;
    //        if ((HN & (1UL << (m - 1))) != 0) currDist--;

    //        matrix[m, j] = currDist;

    //        HP = (HP << 1) | 1;
    //        HN <<= 1;
    //        VP = HN | ~(D0 | HP);
    //        VN = HP & D0;

    //        // Fill rest of matrix for backtracking
    //        for (int i = m - 1; i >= 1; i--)
    //        {
    //            int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
    //            matrix[i, j] = Math.Min(
    //                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
    //                matrix[i - 1, j - 1] + cost
    //            );
    //        }
    //    }

    //    // Backtrack to get edit operations
    //    int x = m, y = n;
    //    while (x > 0 || y > 0)
    //    {
    //        if (x > 0 && y > 0 && matrix[x, y] == matrix[x - 1, y - 1] && s1[x - 1] == s2[y - 1])
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.EQUAL, SourcePos = x - 1, DestPos = y - 1 });
    //            x--; y--;
    //        }
    //        else if (x > 0 && y > 0 && matrix[x, y] == matrix[x - 1, y - 1] + 1)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.REPLACE, SourcePos = x - 1, DestPos = y - 1 });
    //            x--; y--;
    //        }
    //        else if (x > 0 && matrix[x, y] == matrix[x - 1, y] + 1)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.DELETE, SourcePos = x - 1, DestPos = y });
    //            x--;
    //        }
    //        else if (y > 0 && matrix[x, y] == matrix[x, y - 1] + 1)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.INSERT, SourcePos = x, DestPos = y - 1 });
    //            y--;
    //        }
    //        else
    //        {
    //            // Should not reach here
    //            break;
    //        }
    //    }
    //    editOps.Reverse();
    //    return editOps;
    //}

    //public static List<EditOp> EditOps(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    //{
    //    var editOps = new List<EditOp>();
    //    if (s1.IsEmpty && s2.IsEmpty)
    //        return editOps;
    //    int len1 = s1.Length;
    //    int len2 = s2.Length;
    //    int[,] dp = new int[len1 + 1, len2 + 1];
    //    // Initialize DP table
    //    for (int i = 0; i <= len1; i++) dp[i, 0] = i;
    //    for (int j = 0; j <= len2; j++) dp[0, j] = j;
    //    // Fill DP table
    //    for (int i = 1; i <= len1; i++)
    //    {
    //        for (int j = 1; j <= len2; j++)
    //        {
    //            int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
    //            dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
    //        }
    //    }
    //    // Backtrack to find edit operations
    //    int x = len1, y = len2;
    //    while (x > 0 || y > 0)
    //    {
    //        if (x > 0 && dp[x, y] == dp[x - 1, y] + 1)
    //        {
    //            editOps.Add(new EditOp{ EditType = EditType.DELETE, SourcePos = x - 1, DestPos = y});
    //            x--;
    //        }
    //        else if (y > 0 && dp[x, y] == dp[x, y - 1] + 1)
    //        {
    //            editOps.Add(new EditOp { EditType = EditType.DELETE, SourcePos = x, DestPos = y - 1 });
    //            y--;
    //        }
    //        else
    //        {
    //            if (x > 0 && y > 0 && s1[x - 1] != s2[y - 1])
    //            {
    //                editOps.Add(new EditOp { EditType = EditType.DELETE, SourcePos = x - 1, DestPos = y - 1 });
    //            }
    //            x--;
    //            y--;
    //        }
    //    }
    //    editOps.Reverse();
    //    return editOps;
    //}
    //public static List<OpCode> Opcodes(ReadOnlySpan<char> s1, ReadOnlySpan<char> s2)
    //{
    //    var opcodes = new List<OpCode>();
    //    var editOps = EditOps(s1, s2);
    //    int sourceIndex = 0, targetIndex = 0;
    //    foreach (var editOp in editOps)
    //    {
    //        switch (editOp.EditType)
    //        {
    //            case EditType.DELETE:
    //                opcodes.Add(new OpCode
    //                {
    //                    EditType = EditType.DELETE, 
    //                    SourceBegin = editOp.SourcePos, 
    //                    SourceEnd = editOp.SourcePos + 1,
    //                    DestBegin = editOp.DestPos,
    //                    DestEnd = editOp.DestPos
    //                });
    //                //opcodes.Add(("delete", sourcePos, sourcePos + 1, targetIndex, targetIndex));
    //                sourceIndex++;
    //                break;
    //            case EditType.INSERT:
    //                opcodes.Add(new OpCode
    //                {
    //                    EditType = EditType.INSERT,
    //                    SourceBegin = sourceIndex,
    //                    SourceEnd = sourceIndex,
    //                    DestBegin = editOp.DestPos,
    //                    DestEnd = editOp.DestPos + 1
    //                });
    //                //opcodes.Add(("insert", sourceIndex, sourceIndex, targetPos, targetPos + 1));
    //                targetIndex++;
    //                break;
    //            case EditType.REPLACE:
    //                opcodes.Add(new OpCode
    //                {
    //                    EditType = EditType.REPLACE,
    //                    SourceBegin = editOp.SourcePos,
    //                    SourceEnd = editOp.SourcePos + 1,
    //                    DestBegin = editOp.DestPos,
    //                    DestEnd = editOp.DestPos + 1
    //                });
    //                sourceIndex++;
    //                targetIndex++;
    //                break;
    //            default:
    //                throw new InvalidOperationException($"Unknown operation: {editOp.EditType}");
    //        }
    //    }
    //    return opcodes;
    //}


}