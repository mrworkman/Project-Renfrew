// Project Renfrew
// Copyright(C) 2026 Stephen Workman (workman.stephen@gmail.com)
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
using Moq;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.NatSpeakInterop;

namespace GrammarTests {
    /// <summary>
    ///    Exercises <see cref="Grammar.InvokeRule" /> end-to-end: solving the
    ///    spoken words against the active rules and firing the matched
    ///    <c>Do(...)</c> actions with the words their owning rule consumed.
    /// </summary>
    [TestFixture]
    public class ActionDispatchTests {
        private class TestGrammar : Grammar {
            public TestGrammar(IIdGenerator idGenerator)
               : base(
                  new RuleFactory(),
                  idGenerator,
                  new Mock<IGrammarService>(MockBehavior.Loose).Object,
                  new Mock<INatSpeak>(MockBehavior.Loose).Object
               ) { }

            public override void Dispose() { }
            public override void Initialize() { }
        }

        private IdGenerator _idGenerator;
        private TestGrammar _grammar;

        [SetUp]
        public void SetUp() {
            _idGenerator = new IdGenerator();
            _grammar = new TestGrammar(_idGenerator);
        }

        // The ids are assigned deterministically as the rules are built; asking
        // the same generator afterward returns those same ids, so we can compose
        // the spoken words Dragon would have produced.
        private SpokenWord Spoken(string word, string ruleName) {
            return new SpokenWord(
               word,
               _idGenerator.GetWordId(word),
               _idGenerator.GetRuleId(ruleName)
            );
        }

        [Test]
        public void MatchingPhraseInvokesActionWithConsumedWords() {
            IEnumerable<string> received = null;

            _grammar.AddRule(
               "greet",
               r => r.Say("Hello").Say("World").Do(words => received = words)
            );
            _grammar.ActivateRule("greet");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "greet"),
               Spoken("World", "greet")
            });

            Assert.That(received, Is.EqualTo(new[] { "Hello", "World" }));
        }

        [Test]
        public void ParameterlessActionIsInvokedOnMatch() {
            var invoked = false;

            _grammar.AddRule(
               "greet",
               r => r.Say("Hello").Do(() => invoked = true)
            );
            _grammar.ActivateRule("greet");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "greet")
            });

            Assert.That(invoked, Is.True);
        }

        [Test]
        public void NonMatchingPhraseThrowsAndDoesNotInvokeAction() {
            var invoked = false;

            _grammar.AddRule(
               "greet",
               r => r.Say("Hello").Say("World").Do(() => invoked = true)
            );
            _grammar.ActivateRule("greet");

            // "World" is missing: no path matches, so InvokeRule throws and the
            // action is never reached.
            Assert.That(
               () => _grammar.InvokeRule(new List<SpokenWord> {
                   Spoken("Hello", "greet")
               }),
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );

            Assert.That(invoked, Is.False);
        }

        [Test]
        public void OmittedOptionalIsExcludedFromTheActionWords() {
            IEnumerable<string> received = null;

            _grammar.AddRule(
               "opt",
               r => r.Say("a").OptionallySay("b").Say("c")
                  .Do(words => received = words)
            );
            _grammar.ActivateRule("opt");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("a", "opt"),
               Spoken("c", "opt")
            });

            Assert.That(received, Is.EqualTo(new[] { "a", "c" }));
        }

        [Test]
        public void IncludedOptionalIsPresentInTheActionWords() {
            IEnumerable<string> received = null;

            _grammar.AddRule(
               "opt",
               r => r.Say("a").OptionallySay("b").Say("c")
                  .Do(words => received = words)
            );
            _grammar.ActivateRule("opt");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("a", "opt"),
               Spoken("b", "opt"),
               Spoken("c", "opt")
            });

            Assert.That(received, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void EachRuleActionReceivesItsOwnSubtreeWords() {
            IEnumerable<string> outerWords = null;
            IEnumerable<string> innerWords = null;

            _grammar.AddRule(
               "outer",
               r => r.Say("Something").WithRule("inner").Say("To").Say("Eat")
                  .Do(words => outerWords = words)
            );
            _grammar.AddRule(
               "inner",
               r => r.Say("Good").Do(words => innerWords = words)
            );
            _grammar.ActivateRule("outer");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Something", "outer"),
               Spoken("Good", "inner"),
               Spoken("To", "outer"),
               Spoken("Eat", "outer")
            });

            // Decision (a): each action gets the words its owning rule's subtree
            // consumed. The outer rule spans the whole phrase (including the
            // referenced inner rule's word); the inner rule spans only its own.
            Assert.That(
               innerWords,
               Is.EqualTo(new[] { "Good" })
            );
            Assert.That(
               outerWords,
               Is.EqualTo(new[] { "Something", "Good", "To", "Eat" })
            );
        }

        [Test]
        public void ActionsAreInvokedInPathOrder() {
            var order = new List<string>();

            _grammar.AddRule(
               "outer",
               r => r.Say("Something").WithRule("inner").Say("Eat")
                  .Do(() => order.Add("outer"))
            );
            _grammar.AddRule(
               "inner",
               r => r.Say("Good").Do(() => order.Add("inner"))
            );
            _grammar.ActivateRule("outer");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Something", "outer"),
               Spoken("Good", "inner"),
               Spoken("Eat", "outer")
            });

            // The inner action is reached mid-phrase, before the outer rule's
            // trailing action.
            Assert.That(order, Is.EqualTo(new[] { "inner", "outer" }));
        }

        [Test]
        public void IncludedOptionalSubRuleFiresItsActionAndScopesWords() {
            IEnumerable<string> outerWords = null;
            IEnumerable<string> innerWords = null;

            _grammar.AddRule(
               "outer",
               r => r.Say("Something").OptionallyWithRule("inner").Say("To")
                  .Say("Eat").Do(words => outerWords = words)
            );
            _grammar.AddRule(
               "inner",
               r => r.Say("Good").Do(words => innerWords = words)
            );
            _grammar.ActivateRule("outer");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Something", "outer"),
               Spoken("Good", "inner"),
               Spoken("To", "outer"),
               Spoken("Eat", "outer")
            });

            Assert.That(innerWords, Is.EqualTo(new[] { "Good" }));
            Assert.That(
               outerWords,
               Is.EqualTo(new[] { "Something", "Good", "To", "Eat" })
            );
        }

        [Test]
        public void OmittedOptionalSubRuleFiresOnlyTheOuterAction() {
            var innerInvoked = false;
            IEnumerable<string> outerWords = null;

            _grammar.AddRule(
               "outer",
               r => r.Say("Something").OptionallyWithRule("inner").Say("To")
                  .Say("Eat").Do(words => outerWords = words)
            );
            _grammar.AddRule(
               "inner",
               r => r.Say("Good").Do(() => innerInvoked = true)
            );
            _grammar.ActivateRule("outer");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Something", "outer"),
               Spoken("To", "outer"),
               Spoken("Eat", "outer")
            });

            Assert.That(innerInvoked, Is.False);
            Assert.That(
               outerWords,
               Is.EqualTo(new[] { "Something", "To", "Eat" })
            );
        }

        [Test]
        public void TwoLevelNestedRuleChainFiresEveryActionInOrder() {
            var order = new List<string>();

            _grammar.AddRule(
               "outer",
               r => r.Say("Something").WithRule("inner").Say("To").Say("Eat")
                  .Do(() => order.Add("outer"))
            );
            _grammar.AddRule(
               "inner",
               r => r.Say("Good").OptionallyWithRule("inner2")
                  .Do(() => order.Add("inner"))
            );
            _grammar.AddRule(
               "inner2",
               r => r.Say("Is").Say("Nice").Do(() => order.Add("inner2"))
            );
            _grammar.ActivateRule("outer");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Something", "outer"),
               Spoken("Good", "inner"),
               Spoken("Is", "inner2"),
               Spoken("Nice", "inner2"),
               Spoken("To", "outer"),
               Spoken("Eat", "outer")
            });

            Assert.That(order, Is.EqualTo(new[] { "inner2", "inner", "outer" }));
        }

        [Test]
        public void InvokingWithNoActiveRulesThrows() {
            _grammar.AddRule(
               "greet",
               r => r.Say("Hello").Do(() => { })
            );

            // Note: never activated.
            Assert.That(
               () => _grammar.InvokeRule(new List<SpokenWord> {
                   Spoken("Hello", "greet")
               }),
               Throws.InstanceOf<NoActiveRulesException>()
            );
        }

        [Test]
        public void InvokingWithAnEmptyPhraseThrows() {
            _grammar.AddRule("greet", r => r.Say("Hello").Do(() => { }));
            _grammar.ActivateRule("greet");

            Assert.That(
               () => _grammar.InvokeRule(new List<SpokenWord>()),
               Throws.InstanceOf<ArgumentException>()
            );
        }

        [Test]
        public void InvokingWhoseStartRuleIsNotActiveThrows() {
            _grammar.AddRule("greet", r => r.Say("Hello").Do(() => { }));
            _grammar.AddRule("other", r => r.Say("Bye").Do(() => { }));

            // Activate a rule, but speak words belonging to a different,
            // inactive rule.
            _grammar.ActivateRule("other");

            Assert.That(
               () => _grammar.InvokeRule(new List<SpokenWord> {
                   Spoken("Hello", "greet")
               }),
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );
        }

        [Test]
        public void ParsingALeftRecursiveRuleThrowsInsteadOfOverflowing() {
            // A rule that references itself before consuming any word would
            // recurse forever; the parser detects this and throws.
            _grammar.AddRule("loop", r => r.WithRule("loop"));
            _grammar.ActivateRule("loop");

            Assert.That(
               () => _grammar.InvokeRule(new List<SpokenWord> {
                   Spoken("anything", "loop")
               }),
               Throws.InstanceOf<LeftRecursiveRuleException>()
            );
        }
    }
}
