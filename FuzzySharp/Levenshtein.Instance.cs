using Raffinert.FuzzySharp.Utils;
using System;

namespace Raffinert.FuzzySharp;

/// <summary>
/// Provides static methods for computing the Levenshtein distance and similarity between sequences.
/// Implements bit-parallel and dynamic programming algorithms inspired by RapidFuzz's Levenshtein implementation.
/// </summary>
public sealed partial class Levenshtein : IDisposable
{
    private readonly string _source;
    private readonly CharMaskBuffer<char> _charMask;

    public Levenshtein(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));

        var blocks = (_source.Length + 63) >> 6;

        _charMask = new CharMaskBuffer<char>(64, blocks);
        for (var i = 0; i < _source.Length; i++)
        {
            _charMask.AddBit(source[i], i);
        }
    }

    public int DistanceFrom(string value)
    {
        return Distance(_source.AsSpan(), value.AsSpan(), _charMask);
    }
    
    public void Dispose()
    {
        _charMask.Dispose();
    }
}