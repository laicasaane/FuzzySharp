using System;
using System.Buffers;

namespace Raffinert.FuzzySharp.Benchmarks;

// original https://github.com/DanHarltey/Fastenshtein/blob/master/benchmarks/Fastenshtein.Benchmarking/RandomWords.cs
public static class RandomWords
{
    private static readonly char[] Letters = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

    public static string[] Create(int count, int maxWordSize)
    {
        var words = new string[count];

        // using a const seed to make sure runs of the performance tests are consistent.
        var random = new Random(37);

        for (var i = 0; i < words.Length; i++)
        {
            var wordSize = random.Next(3, maxWordSize);

            words[i] = StringCompat.Create(wordSize, random, static (word, r) =>
            {
                for (var j = 0; j < word.Length; j++)
                {
                    var index = r.Next(0, Letters.Length);
                    word[j] = Letters[index];
                }
            });
        }

        return words;
    }

    public static class StringCompat
    {
        public static string Create<TState>(int length, TState state, SpanAction<char, TState> action)
        {
#if NETCOREAPP
            return string.Create(length, state, action);
#else

            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var chars = new char[length];
            action(chars.AsSpan(), state);
            return new string(chars);
#endif
        }
    }
#if !NETCOREAPP
    public delegate void SpanAction<T, in TArg>(Span<T> span, TArg arg);
#endif
}