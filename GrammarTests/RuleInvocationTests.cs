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
using System.Diagnostics;
using Moq;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.NatSpeakInterop;

namespace GrammarTests {
    [TestFixture]
    public class RuleInvocationTests {
        private static int rule1Result = 0;
        private static int rule2Result = 0;
        private static int rule3Result = 0;

        #region TestGrammar

        private class TestGrammar : Grammar {
            public TestGrammar(
               IIdGenerator idGenerator,
               IGrammarService grammarService
            ) : base(
               new RuleFactory(),
               idGenerator,
               grammarService,
               new Mock<INatSpeak>().Object
            ) { }

            public override void Dispose() { }

            public override void Initialize() { }

            public void InitializeRule1() {
                AddRule(
                   "test_rule_01",
                   r =>
                      r.Say("Hello")
                         .Say("Jello")
                         .Do(
                            words => {
                                rule1Result = 1;

                                foreach (var word in words) {
                                    Debug.WriteLine($"Rule1: RECEIVED WORD: {word}");
                                }
                            }
                         )
                );
            }

            public void InitializeRule2() {
                AddRule(
                   "test_rule_02",
                   r =>
                      r.Say("Hello")
                         .Say("Jello")
                         .OptionallySay("Cheese")
                         .Say("Please")
                         .Do(
                            words => {
                                rule2Result = 1;

                                foreach (var word in words) {
                                    Debug.WriteLine($"Rule2: RECEIVED WORD: {word}");
                                }
                            }
                         )
                );
            }

            public void InitializeRule3() {
                AddRule(
                   "test_rule_03",
                   r =>
                      r.Say("Hello")
                         .Optionally(o => o.Say("Skee"))
                         .RepeatOneOf(
                            rp => rp.SayOneOf("a", "b", "c"),
                            rp => rp.Say("Hi"),
                            rp => rp.Say("Sty")
                         )
                         .Do(
                            words => {
                                rule3Result = 1;
                                foreach (var word in words) {
                                    Debug.WriteLine($"Rule3: RECEIVED WORD: {word}");
                                }
                            }
                         )
                );
            }
        }

        #endregion

        private IdGenerator _idGenerator;
        private TestGrammar _grammar;

        [SetUp]
        public void Initialize() {
            var grammarServiceMock = new Mock<IGrammarService>(MockBehavior.Loose);

            _idGenerator = new IdGenerator();
            _grammar = new TestGrammar(_idGenerator, grammarServiceMock.Object);
            _grammar.Initialize();

            rule1Result = 0;
            rule2Result = 0;
            rule3Result = 0;
        }

        // The word/rule ids are assigned as the rules are built; asking the same
        // generator afterward returns those ids, so we can compose the spoken
        // words Dragon would have produced for a given phrase.
        private SpokenWord Spoken(string word, string ruleName) {
            return new SpokenWord(
               word,
               _idGenerator.GetWordId(word),
               _idGenerator.GetRuleId(ruleName)
            );
        }

        [Test]
        public void ComplexRuleActionShouldBeInvoked_Variant1() {
            _grammar.InitializeRule3();
            _grammar.ActivateRule("test_rule_03");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_03"),
               Spoken("Skee", "test_rule_03"),
               Spoken("Sty", "test_rule_03"),
               Spoken("Sty", "test_rule_03"),
               Spoken("Hi", "test_rule_03"),
               Spoken("Hi", "test_rule_03"),
               Spoken("Sty", "test_rule_03")
            });

            Assert.That(rule3Result, Is.EqualTo(1));
        }

        [Test]
        public void ComplexRuleActionShouldBeInvoked_Variant2() {
            _grammar.InitializeRule3();
            _grammar.ActivateRule("test_rule_03");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_03"),
               Spoken("Hi", "test_rule_03"),
               Spoken("Hi", "test_rule_03")
            });

            Assert.That(rule3Result, Is.EqualTo(1));
        }

        [Test]
        public void ComplexRuleActionShouldBeInvoked_Variant3() {
            _grammar.InitializeRule3();
            _grammar.ActivateRule("test_rule_03");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_03"),
               Spoken("Hi", "test_rule_03"),
               Spoken("a", "test_rule_03"),
               Spoken("a", "test_rule_03"),
               Spoken("Sty", "test_rule_03"),
               Spoken("Hi", "test_rule_03"),
               Spoken("c", "test_rule_03"),
               Spoken("b", "test_rule_03"),
               Spoken("c", "test_rule_03"),
               Spoken("c", "test_rule_03")
            });

            Assert.That(rule3Result, Is.EqualTo(1));
        }

        [Test]
        public void SimpleRuleActionShouldBeInvoked() {
            _grammar.InitializeRule1();
            _grammar.ActivateRule("test_rule_01");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_01"),
               Spoken("Jello", "test_rule_01")
            });

            Assert.That(rule1Result, Is.EqualTo(1));
        }

        [Test]
        public void SimpleRuleActionWithExtraWordShouldThrowException() {
            Assert.That(
               () => {
                   _grammar.InitializeRule1();
                   _grammar.ActivateRule("test_rule_01");
                   _grammar.InvokeRule(new List<SpokenWord> {
                      Spoken("Hello", "test_rule_01"),
                      Spoken("Jello", "test_rule_01"),
                      Spoken("Smello", "test_rule_01")
                   });
               },
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );
        }

        [Test]
        public void SimpleRuleActionWithInvalidWordShouldThrowException() {
            Assert.That(
               () => {
                   _grammar.InitializeRule1();
                   _grammar.ActivateRule("test_rule_01");
                   _grammar.InvokeRule(new List<SpokenWord> {
                      Spoken("Hello", "test_rule_01"),
                      Spoken("Mellow", "test_rule_01")
                   });
               },
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );
        }

        [Test]
        public void SimpleRuleActionWithNotEnoughWordsShouldThrowException() {
            Assert.That(
               () => {
                   _grammar.InitializeRule1();
                   _grammar.ActivateRule("test_rule_01");
                   _grammar.InvokeRule(new List<SpokenWord> {
                      Spoken("Hello", "test_rule_01")
                   });
               },
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );
        }

        [Test]
        public void SimpleRuleActionWithMissiongOptionalWordShouldBeInvoked() {
            _grammar.InitializeRule2();
            _grammar.ActivateRule("test_rule_02");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_02"),
               Spoken("Jello", "test_rule_02"),
               Spoken("Please", "test_rule_02")
            });

            Assert.That(rule2Result, Is.EqualTo(1));
        }

        [Test]
        public void SimpleRuleActionWithOptionalWordShouldBeInvoked() {
            _grammar.InitializeRule2();
            _grammar.ActivateRule("test_rule_02");

            _grammar.InvokeRule(new List<SpokenWord> {
               Spoken("Hello", "test_rule_02"),
               Spoken("Jello", "test_rule_02"),
               Spoken("Cheese", "test_rule_02"),
               Spoken("Please", "test_rule_02")
            });

            Assert.That(rule2Result, Is.EqualTo(1));
        }

        [Test]
        public void
           SimpleRuleActionWithInvalidWordInPlaceOfOptionalWordShouldThrowException() {
            Assert.That(
               () => {
                   _grammar.InitializeRule2();
                   _grammar.ActivateRule("test_rule_02");
                   _grammar.InvokeRule(new List<SpokenWord> {
                      Spoken("Hello", "test_rule_02"),
                      Spoken("Jello", "test_rule_02"),
                      Spoken("Sneeze", "test_rule_02"),
                      Spoken("Please", "test_rule_02")
                   });
               },
               Throws.InstanceOf<InvalidSequenceInCallbackException>()
            );
        }

        [Test]
        public void
           TryingToInvokeARuleWhenNoRulesAreActiveShouldThrowAnException() {
            Assert.That(
               () => {
                   _grammar.InvokeRule(new List<SpokenWord> {
                      Spoken("Hello", "test_rule_01")
                   });
               },
               Throws.InstanceOf<NoActiveRulesException>()
            );
        }
    }
}
