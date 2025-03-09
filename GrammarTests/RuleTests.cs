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
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;

namespace GrammarTests {
   [TestFixture]
   public class RuleTests {
      [OneTimeSetUp]
      public void OneTimeSetup() {
         var settings = new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.Objects,
            Formatting = Formatting.Indented
         };

         TestContext.AddFormatter<Sequence>(
            o => JsonConvert.SerializeObject(o, settings)
         );
      }

      [SetUp]
      public void Initialize() {
         _factory = new RuleFactory();
         _idGenerator = new IdGenerator();
      }

      private RuleFactory _factory;
      private IIdGenerator _idGenerator;

      [Test]
      public void ShouldProduceSimpleAlternativeRule() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.SayOneOf("hello", "jello");

         var expectedSequence = Sequence.Create(
            Alternatives.Create(
               Word.Create(1, "hello"),
               Word.Create(2, "jello")
            )
         );

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }

      [Test]
      public void ShouldProduceSimpleAlternativeRuleFromEnumerable() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.SayOneOf(
            new List<string> {
               "hello",
               "jello"
            }
         );

         var expectedSequence = Sequence.Create(
            Alternatives.Create(
               Word.Create(1, "hello"),
               Word.Create(2, "jello")
            )
         );

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }

      [Test]
      public void ShouldProduceSimpleOptionalRule() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.Optionally(r => r.Say("hello"));

         var expectedSequence = Sequence.Create(
            Optional.Create(
               Word.Create(1, "hello")
            )
         );

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }

      [Test]
      public void ShouldProduceSimpleRepetitionRule() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.Repeat(r => r.Say("hello"));

         var expectedSequence = Sequence.Create(
            Repeated.Create(
               Word.Create(1, "hello")
            )
         );

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }

      [Test]
      public void ShouldProduceSimpleSequenceRule() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.Say("hello");

         var expectedSequence = Sequence.Create(Word.Create(1, "hello"));

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }

      [Test]
      public void Foo2() {
         var testRule = new Rule("some_rule", _idGenerator);

         testRule.Optionally(e => e.SayOneOf("cheese", "cheese"));
         testRule.Say("Hello").OneOf(r => r.Say("Hi"), r => r.Say("Boo"));

         var expectedSequence = Sequence.Create(
            Optional.Create(
               Alternatives.Create(
                  Word.Create(1, "cheese"),
                  Word.Create(1, "cheese")
               )
            ),
            Word.Create(2, "Hello"),
            Alternatives.Create(
               Word.Create(3, "Hi"),
               Word.Create(4, "Boo")
            )
         );

         Assert.That(testRule.Sequence, Is.EqualTo(expectedSequence));
      }
   }
}
