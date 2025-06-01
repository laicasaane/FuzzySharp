using System;
using System.Collections.Generic;

namespace Raffinert.FuzzySharp.Extractor;

public class ExtractedResult<T>(T value, int score, int index) : IComparable<ExtractedResult<T>>
{
    public readonly T Value = value;
    public readonly int Score = score;
    public readonly int Index = index;

    public ExtractedResult(T value, int score) : this(value, score, 0)
    { }

    public int CompareTo(ExtractedResult<T> other)
    {
        return Comparer<int>.Default.Compare(this.Score, other.Score);
    }

    public override string ToString()
    {
        if (typeof(T) == typeof(string))
        {
            return $"(string: {Value}, score: {Score}, index: {Index})";
        }
        return $"(value: {Value}, score: {Score}, index: {Index})";
    }
}