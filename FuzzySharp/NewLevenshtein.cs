using Raffinert.FuzzySharp.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp;

public static class NewLevenshtein
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
            return BitParallelIndel.Distance(source, target); //, scoreCutoff);
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