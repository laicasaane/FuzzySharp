using Raffinert.FuzzySharp.Utils;
using System;

namespace Raffinert.FuzzySharp;

public sealed partial class Indel : IDisposable
{
    private readonly string _source;
    private readonly CharMaskBuffer<char> _charMask;

    public Indel(string source)
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
        return DistanceImpl(_source.AsSpan(), value.AsSpan(), _charMask);
    }

    public void Dispose()
    {
        _charMask.Dispose();
    }
}
