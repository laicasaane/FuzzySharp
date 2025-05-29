using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Raffinert.FuzzySharp.Edits;
using Raffinert.FuzzySharp.Utils;

namespace Raffinert.FuzzySharp
{
    public static class LongestCommonSequence
    {
        private const int WordSize = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Similarity<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            if (s1.Length > 64)
            {
                return SimilarityMultipleMachineWords(s1, s2, null);
            }
            
            return SimilaritySingleMachineWord(s1, s2, null);
        }

        /// <summary>
        /// Calculates the length of the longest common subsequence.
        /// </summary>
        public static int SimilaritySingleMachineWord<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            if (s1.IsEmpty)
                return 0;

            int len1 = s1.Length;
            if (len1 > WordSize)
                throw new ArgumentException($"s1 length must be ≤ {WordSize}", nameof(s1));

            // Build bit-mask for each character in s1
            ulong mask = len1 == WordSize ? ulong.MaxValue : (1UL << len1) - 1UL;
            var block = new Dictionary<T, ulong>();
            ulong x = 1UL;
            foreach (T ch in s1)
            {
                if (block.ContainsKey(ch))
                    block[ch] |= x;
                else
                    block[ch] = x;
                x <<= 1;
            }

            // Bit-parallel LCS loop
            ulong S = mask;
            foreach (T ch2 in s2)
            {
                block.TryGetValue(ch2, out ulong Matches);
                ulong u = S & Matches;
                unchecked
                {
                    S = (S + u) | (S - u);
                }
            }

            int res = CountZeroBits(S, len1);
            return res;
        }

        //public static int BlockSimilaritySingleMachineWord<T>(
        //    Dictionary<T, ulong> block,
        //    ReadOnlySpan<T> s1,
        //    ReadOnlySpan<T> s2) where T : IEquatable<T>
        //{
        //    if (s1.IsEmpty)
        //        return 0;

        //    int len1 = s1.Length;
        //    if (len1 > MaxLength)
        //        throw new ArgumentException($"s1 length must be ≤ {MaxLength}", nameof(s1));

        //    ulong S = len1 == MaxLength ? ulong.MaxValue : (1UL << len1) - 1UL;
            
        //    // Bit-parallel LCS loop
        //    foreach (T ch2 in s2)
        //    {
        //        block.TryGetValue(ch2, out ulong Matches);
        //        ulong u = S & Matches;
        //        unchecked
        //        {
        //            S = (S + u) | (S - u);
        //        }
        //    }

        //    int res = CountZeroBits(S, len1);
        //    return res;
        //}

        public static int BlockSimilarityMultipleMachineWords<T>(
            Dictionary<T, ulong[]> block,
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2
        ) where T : IEquatable<T>
        {
            if (s1.IsEmpty)
                return 0;

            int len1 = s1.Length;
            int segCount = (len1 + 63) / 64;

            // --- 2) prepare the \"all-ones up to len1\" mask and state S ---
            ulong[] S = new ulong[segCount];
            for (int i = 0; i < segCount; i++)
                S[i] = ulong.MaxValue;
            // clear high bits in the final segment if len1 % 64 != 0
            int rem = len1 & 63;
            if (rem != 0)
                S[segCount - 1] = (1UL << rem) - 1;

            // --- 3) main bit-parallel loop: S = (S + u) | (S - u)  ---
            foreach (T ch in s2)
            {
                block.TryGetValue(ch, out var M);
                // if no occurrences in s1, M will be all-zeros
                if (M == null) M = new ulong[segCount];

                // u = S & M
                var u = new ulong[segCount];
                for (int i = 0; i < segCount; i++)
                    u[i] = S[i] & M[i];

                // add = S + u  (multi-precision)
                var add = new ulong[segCount];
                ulong carry = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong sum = S[i] + u[i] + carry;
                    // carry if sum < S[i] or (carry==1 && sum==S[i])
                    carry = sum < S[i] || (carry == 1 && sum == S[i]) ? 1UL : 0UL;
                    add[i] = sum;
                }

                // sub = S - u  (multi-precision)
                var sub = new ulong[segCount];
                ulong borrow = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong diff = S[i] - u[i] - borrow;
                    // borrow if original S[i] < u[i] + borrow
                    borrow = S[i] < u[i] + borrow ? 1UL : 0UL;
                    sub[i] = diff;
                }

                // new S = add | sub
                for (int i = 0; i < segCount; i++)
                    S[i] = add[i] | sub[i];
            }

            // --- 4) count zero bits in the lower len1 positions of S ---
            int lcs = CountZeroBits(S, len1);

            return lcs;
        }

        // Helper to count zero bits in lower 'length' bits of S
        //private static int CountZeroBits(ulong S, int length)
        //{
        //    ulong mask = length == WordSize ? ulong.MaxValue : (1UL << length) - 1UL;
        //    ulong inv = ~S & mask;
        //    int count = 0;
        //    while (inv != 0)
        //    {
        //        if ((inv & 1UL) != 0)
        //            count++;
        //        inv >>= 1;
        //    }
        //    return count;
        //}

        public static int SimilarityMultipleMachineWords<T>(
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        Processor<T> processor = null
        ) where T : IEquatable<T>
        {
            // optional preprocessing
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            if (s1.IsEmpty)
                return 0;

            int len1 = s1.Length;
            int segCount = (len1 + 63) / 64;

            // --- 1) build per-symbol bit-masks (one ulong[] per distinct T) ---
            var block = new Dictionary<T, ulong[]>(EqualityComparer<T>.Default);
            for (int i = 0; i < len1; i++)
            {
                T key = s1[i];
                int seg = i / 64;
                int bit = i % 64;
                if (!block.TryGetValue(key, out var arr))
                {
                    arr = new ulong[segCount];
                    block[key] = arr;
                }
                arr[seg] |= 1UL << bit;
            }

            // --- 2) prepare the \"all-ones up to len1\" mask and state S ---
            ulong[] mask = new ulong[segCount];
            for (int i = 0; i < segCount; i++)
                mask[i] = ulong.MaxValue;
            // clear high bits in the final segment if len1 % 64 != 0
            int rem = len1 & 63;
            if (rem != 0)
                mask[segCount - 1] = (1UL << rem) - 1;

            ulong[] S = (ulong[])mask.Clone();

            // --- 3) main bit-parallel loop: S = (S + u) | (S - u)  ---
            foreach (T ch in s2)
            {
                block.TryGetValue(ch, out var M);
                // if no occurrences in s1, M will be all-zeros
                if (M == null) M = new ulong[segCount];

                // u = S & M
                var u = new ulong[segCount];
                for (int i = 0; i < segCount; i++)
                    u[i] = S[i] & M[i];

                // add = S + u  (multi-precision)
                var add = new ulong[segCount];
                ulong carry = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong sum = S[i] + u[i] + carry;
                    // carry if sum < S[i] or (carry==1 && sum==S[i])
                    carry = sum < S[i] || (carry == 1 && sum == S[i]) ? 1UL : 0UL;
                    add[i] = sum;
                }

                // sub = S - u  (multi-precision)
                var sub = new ulong[segCount];
                ulong borrow = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong diff = S[i] - u[i] - borrow;
                    // borrow if original S[i] < u[i] + borrow
                    borrow = S[i] < u[i] + borrow ? 1UL : 0UL;
                    sub[i] = diff;
                }

                // new S = add | sub
                for (int i = 0; i < segCount; i++)
                    S[i] = add[i] | sub[i];
            }

            // --- 4) count zero bits in the lower len1 positions of S ---
            int lcs = CountZeroBits(S, len1);

            return lcs;
        }

        //private static int CountZeroBits(ulong[] S, int length)
        //{
        //    int segCount = S.Length;
        //    int rem = length & 63;
        //    // rebuild the per-segment mask
        //    var mask = new ulong[segCount];
        //    for (int i = 0; i < segCount; i++)
        //        mask[i] = ulong.MaxValue;
        //    if (rem != 0)
        //        mask[segCount - 1] = (1UL << rem) - 1;

        //    int zeros = 0;
        //    for (int i = 0; i < segCount; i++)
        //    {
        //        // invert & mask → bits that are zero in S
        //        ulong inv = ~S[i] & mask[i];
        //        // popcount of inv = number of zero bits
        //        zeros += PopCount(inv);
        //    }

        //    return zeros;
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PopCount(ulong value)
        {

#if NET6_0_OR_GREATER
        return System.Numerics.BitOperations.PopCount(value);
#else
            value -= value >> 1 & 6148914691236517205UL /*0x5555555555555555*/;
            value = (ulong)(((long)value & 3689348814741910323L /*0x3333333333333333*/) + ((long)(value >> 2) & 3689348814741910323L /*0x3333333333333333*/));
            value = (ulong)(((long)value + (long)(value >> 4) & 1085102592571150095L) * 72340172838076673L >>> 56);
            return (int)value;
#endif
        }

        /// <summary>
        /// Calculates the LCS distance = max(len1, len2) - similarity.
        /// </summary>
        public static int Distance<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            int maximum = Math.Max(s1.Length, s2.Length);
            int sim = Similarity(s1, s2, null);
            int dist = maximum - sim;
            return dist;
        }

        /// <summary>
        /// Normalized distance in [0,1].
        /// </summary>
        public static double NormalizedDistance<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null,
            int? scoreCutoff = null) where T : IEquatable<T>
        {
            if (s1 == null && s2 == null)
            {
                return 1;
            }

            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            if (s1.IsEmpty || s2.IsEmpty)
                return 0.0;

            int maximum = Math.Max(s1.Length, s2.Length);
            double norm = Distance(s1, s2) / (double)maximum;
            return !scoreCutoff.HasValue || norm <= scoreCutoff.Value ? norm : 1.0;
        }

        /// <summary>
        /// Normalized similarity in [0,1].
        /// </summary>
        public static double NormalizedSimilarity<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null,
            int? scoreCutoff = null) where T : IEquatable<T>
        {
            
            if (s1 == null || s2 == null)
                return 0.0;

            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            double normSim = 1.0 - NormalizedDistance(s1, s2, null, null);
            return !scoreCutoff.HasValue || normSim >= scoreCutoff.Value ? normSim : 0.0;
        }

        //todo: reimplement like in NewLevenshtein for single and multiple machine words
        // Internal: builds LCS matrix of bitmasks for editops
        /// <summary>
        /// Computes the bit-parallel LCS matrix (as 64-bit blocks) and final similarity (LCS length).
        /// Automatically dispatches to single-word or multi-word implementation.
        /// </summary>
        public static (int Sim, List<ulong[]> Matrix) Matrix<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2) where T : IEquatable<T>
        {
            return s1.Length <= WordSize
                ? MatrixSingleMachineWord(s1, s2)
                : MatrixMultipleMachineWords(s1, s2);
        }

        /// <summary>
        /// Single-machine-word LCS (pattern length ≤ 64).
        /// </summary>
        private static (int Sim, List<ulong[]> Matrix) MatrixSingleMachineWord<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2) where T : IEquatable<T>
        {
            if (s1.IsEmpty)
                return (0, new List<ulong[]>(s2.Length));

            int m = s1.Length;
            ulong S = m == WordSize ? ulong.MaxValue : (1UL << m) - 1UL;

            // build bit-mask
            var block = new Dictionary<T, ulong>();
            ulong bit = 1;
            foreach (var x in s1)
            {
                if (block.ContainsKey(x)) block[x] |= bit;
                else block[x] = bit;
                bit <<= 1;
            }

            var matrix = new List<ulong[]>(s2.Length);
            foreach (var y in s2)
            {
                block.TryGetValue(y, out var M);
                // u = S & M
                ulong u = S & M;
                // S = (S + u) | (S - u)
                unchecked { S = (S + u) | (S - u); }
                matrix.Add([S]);
            }

            int sim = CountZeroBits(S, m);
            return (sim, matrix);
        }

        /// <summary>
        /// Multi-machine-word LCS (any pattern length).
        /// </summary>
        private static (int Sim, List<ulong[]> Matrix) MatrixMultipleMachineWords<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2) where T : IEquatable<T>
        {
            int m = s1.Length;
            if (m == 0)
                return (0, new List<ulong[]>(s2.Length));

            int blocks = (m + WordSize - 1) / WordSize;
            // initialize S[] = all-1 in low m bits
            var S = new ulong[blocks];
            for (int i = 0; i < blocks; i++)
            {
                if (i < blocks - 1 || m % WordSize == 0)
                    S[i] = ulong.MaxValue;
                else
                    S[i] = (1UL << (m % WordSize)) - 1;
            }

            // build blockTable: element → bit-mask array
            var blockTable = new Dictionary<T, ulong[]>();
            for (int i = 0; i < m; i++)
            {
                var key = s1[i];
                int b = i / WordSize, off = i % WordSize;
                if (!blockTable.TryGetValue(key, out var arr))
                {
                    arr = new ulong[blocks];
                    blockTable[key] = arr;
                }
                arr[b] |= 1UL << off;
            }
            var zeroMask = new ulong[blocks];

            var matrix = new List<ulong[]>(s2.Length);
            var U = new ulong[blocks];
            var Sum = new ulong[blocks];
            var Diff = new ulong[blocks];

            foreach (var y in s2)
            {
                // load mask for y
                if (!blockTable.TryGetValue(y, out U))
                    U = zeroMask;

                // big-integer add: Sum = S + U
                ulong carry = 0;
                for (int b = 0; b < blocks; b++)
                {
                    ulong s = S[b], u = U[b];
                    ulong t = unchecked(s + u);
                    ulong c1 = t < s ? 1UL : 0UL;
                    ulong t2 = unchecked(t + carry);
                    ulong c2 = t2 < t ? 1UL : 0UL;
                    Sum[b] = t2;
                    carry = c1 | c2;
                }

                // big-integer subtract: Diff = S - U
                ulong borrow = 0;
                for (int b = 0; b < blocks; b++)
                {
                    ulong s = S[b], u = U[b];
                    ulong t1 = unchecked(s - u);
                    ulong b1 = s < u ? 1UL : 0UL;
                    ulong t2 = unchecked(t1 - borrow);
                    ulong b2 = t1 < borrow ? 1UL : 0UL;
                    Diff[b] = t2;
                    borrow = b1 | b2;
                }

                // update S = Sum | Diff
                for (int b = 0; b < blocks; b++)
                    S[b] = Sum[b] | Diff[b];

                // snapshot row
                matrix.Add((ulong[])S.Clone());
            }

            int sim = CountZeroBits(S, m);
            return (sim, matrix);
        }

        /// <summary>
        /// Count zero-bits in the low 'length' bits of a single ulong.
        /// </summary>
        private static int CountZeroBits(ulong x, int length)
        {
            // invert and mask
            ulong inv = ~x & (length == WordSize ? ulong.MaxValue : (1UL << length) - 1UL);
            return PopCount(inv);
        }

        /// <summary>
        /// Count zero-bits over a multi-word S[] across low 'length' bits.
        /// </summary>
        private static int CountZeroBits(ulong[] S, int length)
        {
            int fullBlocks = length / WordSize;
            int remBits = length % WordSize;
            int zeros = 0;

            // all full blocks
            for (int i = 0; i < fullBlocks; i++)
                zeros += PopCount(~S[i]);

            // last partial block
            if (remBits > 0)
            {
                ulong mask = (1UL << remBits) - 1;
                zeros += PopCount(~S[fullBlocks] & mask);
            }

            return zeros;
        }

        /// <summary>
        /// Computes edit operations to turn s1 into s2.
        /// </summary>
        public static EditOp[] GetEditOps<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            // strip any common prefix/suffix if you like
            var (prefixLen, _) = SequenceUtils.TrimIfNeeded(ref s1, ref s2);

            // now Matrix returns List<ulong[]> — one ulong[] per char of s2
            var (sim, matrix) = Matrix(s1, s2);

            int dist = s1.Length + s2.Length - 2 * sim;
            if (dist == 0)
                return [];

            var opsArray = new EditOp[dist];
            int nextIndex = dist;

            int row = s2.Length, col = s1.Length;
            const int WordSize = 64;

            while (row > 0 && col > 0)
            {
                // pick up the bit-mask vector for the previous row
                var bits = matrix[row - 1];

                // compute which block and which bit in that block is "col-1"
                int bitIndex = col - 1;
                int block = bitIndex / WordSize;
                int offset = bitIndex % WordSize;
                ulong mask = 1UL << offset;

                // if bit is set ⇒ this was a deletion in LCS fallback
                if ((bits[block] & mask) != 0)
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
                    // no deletion ⇒ move up a row
                    row--;

                    // but if still in-bounds and the bit is still zero ⇒ insertion
                    if (row > 0)
                    {
                        bits = matrix[row - 1];
                        if ((bits[block] & mask) == 0)
                        {
                            nextIndex--;
                            opsArray[nextIndex] = new EditOp
                            {
                                EditType = EditType.INSERT,
                                SourcePos = col + prefixLen,
                                DestPos = row + prefixLen
                            };
                            continue;
                        }
                    }

                    // otherwise it was a match/move-left in LCS
                    col--;
                }
            }

            // any remaining deletes on the left edge
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
            // any remaining inserts on the top edge
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


        public static List<OpCode> Opcodes<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            var editOps = GetEditOps(s1, s2, processor);
            var opCodes = editOps.AsOpCodes(s1.Length, s2.Length);
            return opCodes;
        }

        public static List<MatchingBlock> MatchingBlocks<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            var editOps = GetEditOps(s1, s2, processor);
            var matchingBlocks = editOps.AsMatchingBlocks(s1.Length, s2.Length);
            return matchingBlocks;
        }
    }
}
