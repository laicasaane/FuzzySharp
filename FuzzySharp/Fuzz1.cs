using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Raffinert.FuzzySharp
{
    public struct ScoreAlignment(double score, int srcStart, int srcEnd, int destStart, int destEnd)
    {
        /// <summary>Normalized score in [0,100].</summary>
        public double Score = score;
        public int SrcStart = srcStart, SrcEnd = srcEnd;
        public int DestStart = destStart, DestEnd = destEnd;
    }

    public static class Fuzz1
    {
        /// <summary>
        /// Searches for the optimal alignment of the shorter span in the longer span
        /// and returns the partial fuzz.ratio for that alignment, as a value in [0…100].
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double PartialRatio<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            LongestCommonSequence.Processor<T>? processor = null,
            double? scoreCutoff = null
        ) where T : IEquatable<T>
        {
            var alignment = PartialRatioAlignment(s1, s2, processor, scoreCutoff);
            return alignment?.Score ?? 0.0;
        }

        /// <summary>
        /// Searches for the optimal alignment of the shorter span in the longer span
        /// and returns a ScoreAlignment (with a score in [0…100]) or null if below cutoff.
        /// </summary>
        public static ScoreAlignment? PartialRatioAlignment<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            LongestCommonSequence.Processor<T>? processor = null,
            double? scoreCutoff = null
        ) where T : IEquatable<T>
        {
            // 1) Optional preprocessing
            if (processor != null)
            {
                processor(ref s1);
                processor(ref s2);
            }

            // 2) Normalize cutoff to 0…100
            double cutoff100 = scoreCutoff.GetValueOrDefault(0.0);

            // 3) Handle both empty → perfect match
            if (s1.IsEmpty && s2.IsEmpty)
            {
                return new ScoreAlignment(100.0, 0, 0, 0, 0);
            }

            // 4) Determine shorter/longer
            ReadOnlySpan<T> shorter = s1, longer = s2;
            bool swapped = false;
            if (s1.Length > s2.Length)
            {
                shorter = s2;
                longer = s1;
                swapped = true;
            }

            // 5) Call the core PartialRatioImpl with cutoff in [0..1]
            double fracCutoff = cutoff100 / 100.0;
            var res = PartialRatioImpl(shorter, longer, fracCutoff);

            // 6) If same-length inputs and not perfect, try the other direction
            if (res.Score < 100.0 && s1.Length == s2.Length)
            {
                // bump cutoff to whatever we got
                double newCutoff100 = Math.Max(cutoff100, res.Score);
                double newFracCutoff = newCutoff100 / 100.0;

                var res2 = PartialRatioImpl(longer, shorter, newFracCutoff);
                if (res2.Score > res.Score)
                {
                    // swap src/dest
                    res = new ScoreAlignment(
                        res2.Score,
                        srcStart: res2.DestStart,
                        srcEnd: res2.DestEnd,
                        destStart: res2.SrcStart,
                        destEnd: res2.SrcEnd
                    );
                }
            }

            // 7) If below cutoff, return null
            if (res.Score < cutoff100)
                return null;

            // 8) If we swapped at step 4, swap back the src/dest in the result
            if (swapped)
            {
                res = new ScoreAlignment(
                    res.Score,
                    srcStart: res.DestStart,
                    srcEnd: res.DestEnd,
                    destStart: res.SrcStart,
                    destEnd: res.SrcEnd
                );
            }

            return res;
        }

        /// <summary>
        /// C# equivalent of rapidfuzz.distance._partial_ratio_impl
        /// Assumes s1.Length <= s2.Length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ScoreAlignment PartialRatioImpl<T>(
            ReadOnlySpan<T> s1,
            ReadOnlySpan<T> s2,
            double? scoreCutoff = null
        ) where T : IEquatable<T>
        {
            int len1 = s1.Length, len2 = s2.Length;
            if (len1 > len2)
                throw new ArgumentException("Requires s1.Length <= s2.Length");

            // Initial best covers s2[0..len1)
            var res = new ScoreAlignment(0, 0, len1, 0, len1);

            // Build bit-mask dictionary for s1
            int segCount = (len1 + 63) / 64;
            var block = new Dictionary<T, ulong[]>();
            for (int i = 0; i < len1; i++)
            {
                var key = s1[i];
                int seg = i / 64, bit = i % 64;
                if (!block.TryGetValue(key, out var arr))
                    block[key] = arr = new ulong[segCount];
                arr[seg] |= 1UL << bit;
            }

            // Precompute s1’s character set for fast Contains
            var charSet = new HashSet<T>(s1.ToArray());

            double? cutoff = scoreCutoff;
            // 1) Prefixes shorter than len1
            for (int i = 1; i < len1; i++)
            {
                if (!charSet.Contains(s2[i - 1])) continue;
                var slice = s2[..i];
                double sim = BitParallelIndel.BlockNormalizedSimilarity(block, s1, slice);
                if (sim > res.Score && (!cutoff.HasValue || sim >= cutoff.Value))
                {
                    res.Score = sim;
                    cutoff = sim;
                    res.DestStart = 0;
                    res.DestEnd = i;
                    if (sim == 1.0) { res.Score = 100.0; return res; }
                }
            }

            // 2) Full-width windows of length len1
            for (int i = 0; i <= len2 - len1; i++)
            {
                if (!charSet.Contains(s2[i + len1 - 1])) continue;
                var window = s2[i..(i + len1)];
                double sim = BitParallelIndel.BlockNormalizedSimilarity(block, s1, window);
                if (sim > res.Score && (!cutoff.HasValue || sim >= cutoff.Value))
                {
                    res.Score = sim;
                    cutoff = sim;
                    res.DestStart = i;
                    res.DestEnd = i + len1;
                    if (sim == 1.0) { res.Score = 100.0; return res; }
                }
            }

            // 3) Suffixes shorter than len1
            for (int i = len2 - len1 + 1; i < len2; i++)
            {
                if (!charSet.Contains(s2[i])) continue;
                var tail = s2[i..];
                double sim = BitParallelIndel.BlockNormalizedSimilarity(block, s1, tail);
                if (sim > res.Score && (!cutoff.HasValue || sim >= cutoff.Value))
                {
                    res.Score = sim;
                    cutoff = sim;
                    res.DestStart = i;
                    res.DestEnd = len2;
                    if (sim == 1.0) { res.Score = 100.0; return res; }
                }
            }

            // Scale best [0..1] → [0..100]
            res.Score *= 100.0;
            return res;
        }
    }
}
