// Project Renfrew
// Copyright(C) 2025 Stephen Workman (workman.stephen@gmail.com)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see<http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Solving;
using Renfrew.NatSpeakInterop;

namespace GrammarTests {
   [TestFixture]
   public class PathSolvingTests {
      private Mock<Grammar> _grammarMock;
      private static readonly RuleFactory RuleFactory = new();

      #region Convenience Functions

      private static Sequence CreateSequenceFromRule(
         Expression<Action<IRule>> expression
      ) {
         return RuleFactory.Create("-", new IdGenerator(), expression).Sequence;
      }

      private static List<SpokenWord> CreateSpokenWordList(string spokenWords) {
         var lines = spokenWords.Split(spokenWords.Contains(",") ? ',' : '\n')
            .Select(line => line.Trim());

         return lines.Select(line => line.Split(':', '@'))
            .Select(
               parts => new SpokenWord(
                  word: parts[0],
                  wordId: uint.Parse(parts[1]),
                  ruleId: uint.Parse(parts[2])
               )
            )
            .ToList();
      }

      private static object[] Params(Sequence sequence, string spokenWords) {
         return new object[] { sequence, CreateSpokenWordList(spokenWords) };
      }

      #endregion

      #region Simple Sequences Test Sequences

      private static readonly Sequence TestSequence0 = CreateSequenceFromRule(
         rule => rule.Say("this", "is", "a", "test")
      );

      #endregion

      #region Optionals Test Sequences

      private static readonly Sequence TestSequence1 = CreateSequenceFromRule(
         rule => rule.Say("hello").OptionallySay("cosmo").Say("kramer")
      );

      private static readonly Sequence TestSequence2 = CreateSequenceFromRule(
         rule => rule.Say("i'm", "cosmo", "kramer")
            .Optionally(optional => optional.Say("the", "ass", "man"))
      );

      private static readonly Sequence TestSequence3 = CreateSequenceFromRule(
         rule => rule.Say("one", "two", "three")
            .Optionally(optional => optional.Say("four", "five"))
            .Say("four", "five")
      );

      // Nested optionals.
      private static readonly Sequence TestSequence4 = CreateSequenceFromRule(
         rule => rule.Optionally(
               optional =>
                  optional.Say("one", "two", "three").OptionallySay("four")
            )
            .Say("five")
      );

      #endregion

      #region Repeated Test Sequences

      private static readonly Sequence TestRepeatedSequence1 =
         CreateSequenceFromRule(
            rule => rule.Say("hello").Repeat(repeat => repeat.Say("jerk"))
         );

      private static readonly Sequence TestRepeatedSequence2 =
         CreateSequenceFromRule(
            rule => rule.Say("hello")
               .Repeat(repeat => repeat.Say("jerk"))
               .Say("face")
         );

      private static readonly Sequence TestRepeatedSequence3 =
         CreateSequenceFromRule(
            rule => rule.Say("one")
               .Repeat(repeat => repeat.Say("two", "three"))
               .Say("two", "three", "four")
         );

      #endregion

      #region Simple Seqences Test Cases

      private static readonly object[] GoodSimpleSequenceCases = {
         Params(TestSequence0, "this:1@1,is:2@1,a:3@1,test:4@1"),
      };

      private static readonly object[] BadSimpleSequenceCases = {
         Params(TestSequence0, "this:1@1,is:1@1"),
         Params(TestSequence0, "this:1@1,is:2@1,a:3@1,test:4@1,too:5@1"),
         Params(TestSequence0, "a:3@1,is:2@1,test:4@1,this:1@1"),
         Params(TestSequence0, "a:3@1,a:3@1,a:3@1,a:3@1"),
      };

      #endregion

      #region Optionals Test Cases

      // @formatter:off
      private static readonly object[] GoodOptionalsCases = {
         Params(TestSequence1, "hello:1@1,cosmo:2@1,kramer:3@1"),
         Params(TestSequence1, "hello:1@1,kramer:3@1"),
         Params(TestSequence2, "i'm:1@1,cosmo:2@1,kramer:3@1"),
         Params(TestSequence2, "i'm:1@1,cosmo:2@1,kramer:3@1,the:4@1,ass:5@1,man:6@1" ),
         Params(TestSequence3, "one:1@1,two:2@1,three:3@1,four:4@1,five:5@1"),
         Params(TestSequence3, "one:1@1,two:2@1,three:3@1,four:4@1,five:5@1,four:4@1,five:5@1"),
         Params(TestSequence4, "five:5@1"),
         Params(TestSequence4, "one:1@1,two:2@1,three:3@1,five:5@1"),
         Params(TestSequence4, "one:1@1,two:2@1,three:3@1,four:4@1,five:5@1"),
      };
      // @formatter:on

      // @formatter:off
      private static readonly object[] BadOptionalsCases = {
         Params(TestSequence3, "one:1@1,two:2@1,three:3@1,four:4@1,five:5@1,four:4@1,five:5@1,four:4@1,five:5@1"),
         Params(TestSequence3, "one:1@1,two:2@1,three:3@1,four:4@1,five:5@1,four:6@1,five:7@1"),
         Params(TestSequence3, "one:1@1,two:2@1,three:3@1,four:9@1,five:108@1,four:4@1,five:5@1"),
         Params(TestSequence4, "one:1@1,four:4@1"),
         Params(TestSequence4, "one:1@1,four:4@1,five:5@1"),
         Params(TestSequence4, "one:1@1,two:2@1,three:3@1"),
         Params(TestSequence4, "one:1@1,two:2@1,three:3@1,four:4@1"),
         Params(TestSequence4, "one:1@1,two:2@1,five:5@1"),
         Params(TestSequence4, "one:1@1,two:2@1,four:4@1,four:4@1"),
         Params(TestSequence4, "four:4@1,five:5@1"),
      };
      // @formatter:on 

      #endregion

      #region Repeated Test Cases

      // @formatter:off
      private static readonly object[] GoodRepeatedCases = {
         Params(TestRepeatedSequence1, "hello:1@1,jerk:2@1"),
         Params(TestRepeatedSequence1, "hello:1@1,jerk:2@1,jerk:2@1"),
         Params(TestRepeatedSequence1, "hello:1@1,jerk:2@1,jerk:2@1,jerk:2@1,jerk:2@1"),
         Params(TestRepeatedSequence2, "hello:1@1,jerk:2@1,face:3@1"),
         Params(TestRepeatedSequence2, "hello:1@1,jerk:2@1,jerk:2@1,jerk:2@1,face:3@1"),
         Params(TestRepeatedSequence3, "one:1@1,two:2@1,three:3@1,two:2@1,three:3@1,four:4@1"),
         Params(TestRepeatedSequence3, "one:1@1,two:2@1,three:3@1,two:2@1,three:3@1,two:2@1,three:3@1,two:2@1,three:3@1,four:4@1"),
      };
      // @formatter:on

      // @formatter:off
      private static readonly object[] BadRepeatedCases = {
         Params(TestRepeatedSequence1, "hello:1@1"),
         Params(TestRepeatedSequence1, "hello:1@1,jerk:2@1,jerk:2@1,hello:1@1"),
         Params(TestRepeatedSequence2, "hello:1@1,face:3@1"),
         Params(TestRepeatedSequence2, "hello:1@1,hello:1@1,face:3@1"),
         Params(TestRepeatedSequence3, "one:1@1,two:2@1,three:3@1,four:4@1"),
         Params(TestRepeatedSequence3, "one:1@1,two:2@1,three:3@1"),
         Params(TestRepeatedSequence3, "two:2@1,three:3@1,four:4@1"),
      };
      // @formatter:on

      #endregion

      [SetUp]
      public void SetUp() {
         _grammarMock = new Mock<Grammar>(
            new Mock<IGrammarService>().Object,
            new Mock<INatSpeak>().Object
         );
      }

      private SolveResult BasePhraseHandler(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var phrase = new ListWalker<SpokenWord>(spokenWords);

         return Solver.VisitSequence(
            testSequene,
            true,
            phrase,
            _grammarMock.Object
         );
      }

      [Test]
      [TestCaseSource(nameof(GoodSimpleSequenceCases))]
      public void ShouldHandleGoodSimpleSequenceCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Success>(result);
      }

      [Test]
      [TestCaseSource(nameof(BadSimpleSequenceCases))]
      public void ShouldErrorOnBadSimpleSequenceCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Failure>(result);
      }

      [Test]
      [TestCaseSource(nameof(GoodOptionalsCases))]
      public void ShouldHandleGoodOptionalsCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Success>(result);
      }

      [Test]
      [TestCaseSource(nameof(BadOptionalsCases))]
      public void ShouldErrorOnBadOptionalsCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Failure>(result);
      }

      [Test]
      [TestCaseSource(nameof(GoodRepeatedCases))]
      public void ShouldHandleGoodRepeatedCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Success>(result);
      }

      [Test]
      [TestCaseSource(nameof(BadRepeatedCases))]
      public void ShouldErrorOnBadRepeatedCases(
         Sequence testSequene,
         List<SpokenWord> spokenWords
      ) {
         var result = BasePhraseHandler(testSequene, spokenWords);
         Assert.IsInstanceOf<SolveResult.Failure>(result);
      }
   }
}
