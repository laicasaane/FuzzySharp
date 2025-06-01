using System.Runtime.CompilerServices;

namespace Raffinert.FuzzySharp.Utils;

internal static class NumericsPolyfill
{
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
}