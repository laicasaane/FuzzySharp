using System.Collections.Generic;
using NUnit.Framework;
using Raffinert.FuzzySharp.Benchmarks;

namespace Raffinert.FuzzySharp.Test;

[TestFixture]
public class LevenshteinTests
{
    [Test]
    [TestCase(
    "I had two heart attacks, an abortion, did crack... while I was pregnant. Other than that, I'm fine.", 
    "You couldn't even be a vegetable - even artichokes have a heart.", 
    76)]
    [TestCase("", "", 0)]
    [TestCase("a", "", 1)]
    [TestCase("", "a", 1)]
    [TestCase("kitten", "kitten", 0)]
    [TestCase("kitten", "sitting", 3)]     // substitution + insertion + insertion
    [TestCase("flaw", "lawn", 2)]          // substitution + insertion
    [TestCase("gumbo", "gambol", 2)]       // insertion + substitution
    [TestCase("book", "back", 2)]          // two substitutions
    [TestCase("Sunday", "Saturday", 3)]    // insertion + substitution + insertion
    // Test a few boundary scenarios (longer strings, only one‐character difference)
    [TestCase("a", "b", 1)]
    [TestCase("ab", "ba", 2)]
    [TestCase("abcdef", "azced", 3)]
    [TestCase("distance", "difference", 5)]
    public void TestLevenshteinDistance(string s1, string s2, int expectedDistance)
    {
        int distance = Levenshtein.Distance(s1, s2);
        Assert.AreEqual(expectedDistance, distance);
    }
    
    [Test]
    public void TestLevenshteinDistance()
    {
        var wordA = new string('A', 4112);
        var wordB = new string('B', 4112);
        int distance = Levenshtein.Distance(wordA, wordB);
        int distance1 = Levenshtein.Distance(wordA, wordA);
        Assert.AreEqual(4112, distance);

        var words = RandomWords.Create(50, 1024);

        List<int> distances = [];
        
        for (var i = 0; i < words.Length; i++)
        {
            for (int j = 0; j < words.Length; j++)
            {
                var d = Levenshtein.Distance(words[i], words[j]);
                distances.Add(d);
            }
        }
    }
}