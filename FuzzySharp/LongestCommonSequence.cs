using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Raffinert.FuzzySharp.Utils;

namespace Raffinert.FuzzySharp
{
    public static class LongestCommonSequence
    {
        private const int MaxLength = 64;

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
            if (len1 > MaxLength)
                throw new ArgumentException($"s1 length must be ≤ {MaxLength}", nameof(s1));

            // Build bit-mask for each character in s1
            ulong mask = len1 == MaxLength ? ulong.MaxValue : (1UL << len1) - 1UL;
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
                    carry = (sum < S[i] || (carry == 1 && sum == S[i])) ? 1UL : 0UL;
                    add[i] = sum;
                }

                // sub = S - u  (multi-precision)
                var sub = new ulong[segCount];
                ulong borrow = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong diff = S[i] - u[i] - borrow;
                    // borrow if original S[i] < u[i] + borrow
                    borrow = (S[i] < u[i] + borrow) ? 1UL : 0UL;
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
        private static int CountZeroBits(ulong S, int length)
        {
            ulong mask = length == MaxLength ? ulong.MaxValue : (1UL << length) - 1UL;
            ulong inv = ~S & mask;
            int count = 0;
            while (inv != 0)
            {
                if ((inv & 1UL) != 0)
                    count++;
                inv >>= 1;
            }
            return count;
        }

        public static int SimilarityMultipleMachineWords<T>(
        ReadOnlySpan<T> s1,
        ReadOnlySpan<T> s2,
        Processor<T>? processor = null
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
                arr[seg] |= (1UL << bit);
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
                    carry = (sum < S[i] || (carry == 1 && sum == S[i])) ? 1UL : 0UL;
                    add[i] = sum;
                }

                // sub = S - u  (multi-precision)
                var sub = new ulong[segCount];
                ulong borrow = 0;
                for (int i = 0; i < segCount; i++)
                {
                    ulong diff = S[i] - u[i] - borrow;
                    // borrow if original S[i] < u[i] + borrow
                    borrow = (S[i] < u[i] + borrow) ? 1UL : 0UL;
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

        private static int CountZeroBits(ulong[] S, int length)
        {
            int segCount = S.Length;
            int rem = length & 63;
            // rebuild the per-segment mask
            var mask = new ulong[segCount];
            for (int i = 0; i < segCount; i++)
                mask[i] = ulong.MaxValue;
            if (rem != 0)
                mask[segCount - 1] = (1UL << rem) - 1;

            int zeros = 0;
            for (int i = 0; i < segCount; i++)
            {
                // invert & mask → bits that are zero in S
                ulong inv = ~S[i] & mask[i];
                // popcount of inv = number of zero bits
                zeros += PopCount(inv);
            }

            return zeros;
        }

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
        private static (int sim, List<ulong> matrix) Matrix<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2) where T : IEquatable<T>
        {
            if (s1.IsEmpty)
                return (0, []);

            int len1 = s1.Length;
            if (len1 > MaxLength)
                throw new ArgumentException($"s1 length must be ≤ {MaxLength}", nameof(s1));

            ulong S = len1 == MaxLength ? ulong.MaxValue : (1UL << len1) - 1UL;
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

            var matrix = new List<ulong>(s2.Length);
            foreach (var ch2 in s2)
            {
                block.TryGetValue(ch2, out ulong Matches);
                ulong u = S & Matches;
                unchecked { S = (S + u) | (S - u); }
                matrix.Add(S);
            }

            int sim = CountZeroBits(S, len1);
            return (sim, matrix);
        }

        /// <summary>
        /// Describes a single edit operation.
        /// </summary>
        public class Editop
        {
            public string Tag { get; }
            public int SrcPos { get; }
            public int DestPos { get; }

            public Editop(string tag, int srcPos, int destPos)
            {
                Tag = tag;
                SrcPos = srcPos;
                DestPos = destPos;
            }
        }

        /// <summary>
        /// Collection of edit operations and conversion to opcodes.
        /// </summary>
        public record Editops(List<Editop> Ops, int SrcLen, int DestLen)
        {
            /// <summary>
            /// Converts edit operations to opcodes, grouping "equal" runs.
            /// </summary>
            public List<Opcode> AsOpcodes()
            {
                var opcodes = new List<Opcode>();
                int prev_i = 0, prev_j = 0;
                foreach (var op in Ops)
                {
                    int i = op.SrcPos;
                    int j = op.DestPos;
                    // equal segment
                    if (prev_i < i && prev_j < j)
                        opcodes.Add(new Opcode("equal", prev_i, i, prev_j, j));
                    // delete
                    if (op.Tag == "delete")
                    {
                        opcodes.Add(new Opcode("delete", i, i + 1, j, j));
                        prev_i = i + 1;
                        prev_j = j;
                    }
                    // insert
                    else if (op.Tag == "insert")
                    {
                        opcodes.Add(new Opcode("insert", i, i, j, j + 1));
                        prev_i = i;
                        prev_j = j + 1;
                    }
                }

                // final equal segment
                if (prev_i < SrcLen && prev_j < DestLen)
                    opcodes.Add(new Opcode("equal", prev_i, SrcLen, prev_j, DestLen));
                
                return opcodes;
            }

            public List<Editop> Ops { get; } = Ops;
            public int SrcLen { get; } = SrcLen;
            public int DestLen { get; } = DestLen;
        }

        /// <summary>
        /// Represents a grouped opcode for diff operations.
        /// </summary>
        public record struct Opcode(string Tag, int I1, int I2, int J1, int J2);

        
        public delegate void Processor<T>(ref ReadOnlySpan<T> str) where T : IEquatable<T>;

        /// <summary>
        /// Computes edit operations to turn s1 into s2.
        /// </summary>
        public static Editops GetEditops<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            var editops = new Editops([], s1.Length, s2.Length);

            var (prefixLen, _) = SequenceUtils.TrimIfNeeded(ref s1, ref s2);
            var (sim, matrix) = Matrix(s1, s2);
            
            int dist = s1.Length + s2.Length - 2 * sim;
            if (dist == 0)
                return editops;

            var editopList = new Editop[dist];
            int col = s1.Length;
            int row = s2.Length;
            int d = dist;

            while (row != 0 && col != 0)
            {
                // deletion
                if ((matrix[row - 1] & (1UL << (col - 1))) != 0)
                {
                    d--;
                    col--;
                    editopList[d] = new Editop("delete", col + prefixLen, row + prefixLen);
                }
                else
                {
                    row--;
                    // insertion
                    if (row != 0 && (matrix[row - 1] & (1UL << (col - 1))) == 0)
                    {
                        d--;
                        editopList[d] = new Editop("insert", col + prefixLen, row + prefixLen);
                    }
                    else
                    {
                        col--;
                    }
                }
            }

            // remaining deletions
            while (col != 0)
            {
                d--;
                col--;
                editopList[d] = new Editop("delete", col + prefixLen, row + prefixLen);
            }
            // remaining insertions
            while (row != 0)
            {
                d--;
                row--;
                editopList[d] = new Editop("insert", col + prefixLen, row + prefixLen);
            }

            editops.Ops.AddRange(editopList);
            return editops;
        }

        /// <summary>
        /// Computes grouped opcodes for turning s1 into s2.
        /// </summary>
        public static List<Opcode> Opcodes<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            Processor<T> processor = null) where T : IEquatable<T>
        {
            return GetEditops(s1, s2, processor).AsOpcodes();
        }
    }
}
