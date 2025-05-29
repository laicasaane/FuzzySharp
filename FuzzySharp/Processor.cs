using System;

namespace Raffinert.FuzzySharp;

public delegate void Processor<T>(ref ReadOnlySpan<T> str) where T : IEquatable<T>;