﻿using NUnit.Framework;
using Raffinert.FuzzySharp.PreProcess;
using Raffinert.FuzzySharp.SimilarityRatio;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.Composite;
using Raffinert.FuzzySharp.SimilarityRatio.Scorer.StrategySensitive;

namespace Raffinert.FuzzySharp.Test.EvaluationTests;

[TestFixture]
public class EvaluationTests
{
    [Test]
    public void Evaluate()
    {
        var a1 = Fuzz.Ratio("mysmilarstring", "myawfullysimilarstirng");
        var a2 = Fuzz.Ratio("mysmilarstring", "mysimilarstring");

        var b1 = Fuzz.PartialRatio("similar", "somewhresimlrbetweenthisstring");

        var c1 = Fuzz.TokenSortRatio("order words out of", "  words out of order");
        var c2 = Fuzz.PartialTokenSortRatio("order words out of", "  words out of order");

        var d1 = Fuzz.TokenSetRatio("fuzzy was a bear", "fuzzy fuzzy fuzzy bear");
        var d2 = Fuzz.PartialTokenSetRatio("fuzzy was a bear", "fuzzy fuzzy fuzzy bear");

        var e1 = Fuzz.WeightedRatio("The quick brown fox jimps ofver the small lazy dog", "the quick brown fox jumps over the small lazy dog");

        var f1 = Fuzz.TokenInitialismRatio("NASA", "National Aeronautics and Space Administration");
        var f2 = Fuzz.TokenInitialismRatio("NASA", "National Aeronautics Space Administration");

        var f3 = Fuzz.TokenInitialismRatio("NASA", "National Aeronautics Space Administration, Kennedy Space Center, Cape Canaveral, Florida 32899");
        var f4 = Fuzz.PartialTokenInitialismRatio("NASA", "National Aeronautics Space Administration, Kennedy Space Center, Cape Canaveral, Florida 32899");

        var g1 = Fuzz.TokenAbbreviationRatio("bl 420", "Baseline section 420", PreprocessMode.Full);
        var g2 = Fuzz.PartialTokenAbbreviationRatio("bl 420", "Baseline section 420", PreprocessMode.Full);



        var h1 = Process.ExtractOne("cowboys", ["Atlanta Falcons", "New York Jets", "New York Giants", "Dallas Cowboys"]);
        var h2 = string.Join(", ", Process.ExtractTop("goolge", ["google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl"], limit: 3));
        var h3 = string.Join(", ", Process.ExtractAll("goolge", ["google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl"]));
        var h4 = string.Join(", ", Process.ExtractAll("goolge", ["google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl"], cutoff: 40));
        var h5 = string.Join(", ", Process.ExtractSorted("goolge", ["google", "bing", "facebook", "linkedin", "twitter", "googleplus", "bingnews", "plexoogl"]));

        var i1 = Process.ExtractOne("cowboys", ["Atlanta Falcons", "New York Jets", "New York Giants", "Dallas Cowboys"], s => s, ScorerCache.Get<DefaultRatioScorer>());

        string[][] events =
        [
            ["chicago cubs vs new york mets", "CitiField", "2011-05-11", "8pm"],
            ["new york yankees vs boston red sox", "Fenway Park", "2011-05-11", "8pm"],
            ["atlanta braves vs pittsburgh pirates", "PNC Park", "2011-05-11", "8pm"]
        ];
        var query = new[] { "new york mets vs chicago cubs", "CitiField", "2017-03-19", "8pm" };

        var best = Process.ExtractOne(query, events, strings => strings[0]);

        var ratio = ScorerCache.Get<DefaultRatioScorer>();
        var partial = ScorerCache.Get<PartialRatioScorer>();
        var tokenSet = ScorerCache.Get<TokenSetScorer>();
        var partialTokenSet = ScorerCache.Get<PartialTokenSetScorer>();
        var tokenSort = ScorerCache.Get<TokenSortScorer>();
        var partialTokenSort = ScorerCache.Get<PartialTokenSortScorer>();
        var tokenAbbreviation = ScorerCache.Get<TokenAbbreviationScorer>();
        var partialTokenAbbreviation = ScorerCache.Get<PartialTokenAbbreviationScorer>();
        var weighted = ScorerCache.Get<WeightedRatioScorer>();
    }

    [Test]
    public void TokenInitialismScorer_WhenGivenStringWithTrailingSpaces_DoesNotBreak()
    {
        // arrange
        var longer = "lusiki plaza share block ";
        var shorter = "jmft";

        // act
        var ratio = Fuzz.TokenInitialismRatio(shorter, longer);

        // assert
        Assert.IsTrue(ratio >= 0);
    }
}