using System;
using System.Buffers;
using System.Collections.Generic;
using Microsoft.Collections.Extensions;
using Raffinert.FuzzySharp.Utils;

namespace Raffinert.FuzzySharp;

internal sealed class CharMaskBuffer<T> : IDisposable where T : notnull, IEquatable<T>
{
    private readonly ArrayPool<ulong> _pool;
    private readonly DictionarySlimPooled<T, int> _indexMap;
    private ulong[] _buffer;
    private readonly int _blocks;
    private int _capacity; // max number of characters (buffered)
    private int _next;

    public CharMaskBuffer(int estimatedCharCount, int blocks, ArrayPool<ulong>? pool = null)
    {
        _pool = pool ?? ArrayPool<ulong>.Shared;
        _blocks = blocks;
        _capacity = estimatedCharCount;
        _buffer = _pool.Rent(_capacity * _blocks);
        _indexMap = new DictionarySlimPooled<T, int>();
        _next = 0;
    }

    public void AddBit(T key, int position)
    {
        ref var index = ref _indexMap.GetOrAddValueRef(key);

        if (index == 0)
        {
            if (_next >= _capacity)
            {
                GrowBuffer(); // resize before assigning
            }

            index = ++_next;

            // Clear new slice
            var slice = new Span<ulong>(_buffer, (index-1) * _blocks, _blocks);
            slice.Clear();
        }

        //if (!_indexMap.TryGetValue(key, out var index))
        //{
        //    if (_next >= _capacity)
        //    {
        //        GrowBuffer(); // resize before assigning
        //    }

        //    index = _next++;
        //    _indexMap[key]
        //    _indexMap[key] = index;

        //    // Clear new slice
        //    var slice = new Span<ulong>(_buffer, index * _blocks, _blocks);
        //    slice.Clear();
        //}

        int block = position >> 6;
        int offset = position & 63;
        _buffer[(index - 1) * _blocks + block] |= 1UL << offset;
    }

    private void GrowBuffer()
    {
        int newCapacity = _capacity * 2;
        ulong[] newBuffer = _pool.Rent(newCapacity * _blocks);

        // Copy existing masks
        Array.Copy(_buffer, 0, newBuffer, 0, _capacity * _blocks);

        // Return old buffer
        _pool.Return(_buffer);

        _buffer = newBuffer;
        _capacity = newCapacity;
    }

    public bool TryGetMask(T key, out ReadOnlySpan<ulong> mask)
    {
        if (_indexMap.TryGetValue(key, out var index))
        {
            mask = new ReadOnlySpan<ulong>(_buffer, index * _blocks, _blocks);
            return true;
        }
        mask = default;
        return false;
    }

    public ReadOnlySpan<ulong> GetOrDefault(T key, ReadOnlySpan<ulong> fallback)
    {
        return TryGetMask(key, out var mask) ? mask : fallback;
    }

    public int Capacity => _capacity;
    public int Count => _next;

    public void Dispose()
    {
        _pool.Return(_buffer);
        _indexMap.Dispose();
    }
}