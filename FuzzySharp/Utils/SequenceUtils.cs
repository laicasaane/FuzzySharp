using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp.Utils;

public static class SequenceUtils
{
    public static int CommonPrefix<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        int prefixLength = 0;
        int minLength = Math.Min(s1.Length, s2.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (!s1[i].Equals(s2[i]))
            {
                break;
            }
            prefixLength++;
        }
        return prefixLength;
    }
    public static int CommonSuffix<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        int suffixLength = 0;
        int minLength = Math.Min(s1.Length, s2.Length);
        for (int i = 1; i <= minLength; i++)
        {
            if (!s1[^i].Equals(s2[^i]))
            {
                break;
            }
            suffixLength++;
        }
        return suffixLength;
    }
    
    public static (int PrefixLength, int SuffixLength) CommonAffix<T>(ReadOnlySpan<T> s1, ReadOnlySpan<T> s2) where T : IEquatable<T>
    {
        int prefixLength = CommonPrefix(s1, s2);
        int suffixLength = CommonSuffix(s1[prefixLength..], s2[prefixLength..]);
        return (prefixLength, suffixLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TrimCommonAffixAndSwapIfNeeded<T>(ref ReadOnlySpan<T> source, ref ReadOnlySpan<T> target) where T : IEquatable<T>
    {
        _ = TrimCommonAffix(ref source, ref target);

        SwapIfSourceIsLonger(ref source, ref target);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int PrefixLength, int SuffixLength) TrimCommonAffix<T>(ref ReadOnlySpan<T> source, ref ReadOnlySpan<T> target) where T : IEquatable<T>
    {
        var startIndex = 0;
        var sourceEnd = source.Length;
        var targetEnd = target.Length;

        while (startIndex < sourceEnd && startIndex < targetEnd && EqualityComparer<T>.Default.Equals(source[startIndex], target[startIndex]))
        {
            startIndex++;
        }
        while (startIndex < sourceEnd && startIndex < targetEnd && EqualityComparer<T>.Default.Equals(source[sourceEnd - 1], target[targetEnd - 1]))
        {
            sourceEnd--;
            targetEnd--;
        }

        var sourceLength = sourceEnd - startIndex;
        var targetLength = targetEnd - startIndex;

        source = source.Slice(startIndex, sourceLength);
        target = target.Slice(startIndex, targetLength);

        return (startIndex, source.Length - sourceEnd);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapIfSourceIsLonger<T>(ref ReadOnlySpan<T> source, ref ReadOnlySpan<T> target)
    {
        if (source.Length <= target.Length)
        {
            return;
        }

        var temp = source;
        source = target;
        target = temp;
    }
}