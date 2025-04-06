// Project Renfrew
// Copyright(C) 2017 Stephen Workman (workman.stephen@gmail.com)
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
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.Solving;
using Renfrew.NatSpeakInterop;

namespace GrammarTests {
   [TestFixture]
   public class PathSolvingTests {
      private Mock<Grammar> _grammarMock;

      private readonly Sequence _helloCosmoKramerSequence = Sequence.Create(
         new List<ISequenceMember> {
            Word.Create(1, "hello"),
            Optional.Create(Word.Create(2, "cosmo")),
            Word.Create(3, "kramer"),
         }
      );

      [SetUp]
      public void SetUp() {
         _grammarMock = new Mock<Grammar>(
            new Mock<IGrammarService>().Object,
            new Mock<INatSpeak>().Object
         );
      }

      [Test]
      public void ShouldHandleSpokenOptional() {
         var phrase = new ListWalker<SpokenWord>(
            new List<SpokenWord> {
               new(ruleId: 1, word: "hello", wordId: 1),
               new(ruleId: 1, word: "cosmo", wordId: 2),
               new(ruleId: 1, word: "kramer", wordId: 3),
            }
         );

         var result = Solver.VisitSequence(
            _helloCosmoKramerSequence,
            phrase,
            _grammarMock.Object
         );
      }

      [Test]
      public void ShouldHandleUnpokenOptional() {
         var phrase = new ListWalker<SpokenWord>(
            new List<SpokenWord> {
               new(ruleId: 1, word: "hello", wordId: 1),
               new(ruleId: 1, word: "kramer", wordId: 3),
            }
         );

         var result = Solver.VisitSequence(
            _helloCosmoKramerSequence,
            phrase,
            _grammarMock.Object
         );

         Assert.IsInstanceOf<SolveResult.Success>(result);
      }
   }
}
