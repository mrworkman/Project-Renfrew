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

using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.Serialization;
using Renfrew.Grammar.Serialization.HighLevelTypes;

namespace GrammarTests.SerializationTests {
    [TestFixture]
    internal class RuleConverterTests {
        private IdGenerator _generator;
        private RuleConverter _converter = new();

        [OneTimeSetUp]
        public void OneTimeSetup() {
            var settings = new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            TestContext.AddFormatter<Symbol>(
               o => JsonConvert.SerializeObject(o, settings)
            );
        }

        [SetUp]
        public void Setup() {
            _generator = new();
        }

        [Test]
        public void ShouldConvert_Say() {
            var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Id = 1,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Sequence,
            }

            #endregion
         };

            var convertedRule = _converter.ConvertRule(
               new Rule("-", _generator)
                  .Say("hello")
            );

            Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
        }

        [Test]
        public void ShouldConvert_SimpleSayOptionallySay() {
            var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Id = 1,
            },
            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Optional,
            },
            new() {
               Type = SymbolType.Word,
               Id = 2,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Optional,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Sequence,
            }

            #endregion
         };

            var convertedRule = _converter.ConvertRule(
               new Rule("-", _generator)
                  .Say("hello")
                  .OptionallySay("jello")
            );

            Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
        }

        [Test]
        public void ShouldConvert_ActionMembersEmitNoSymbols() {
            // A Do(...) action lives in the sequence but produces no Dragon
            // symbols, so a rule that carries one must serialize identically to
            // the same rule without it — including not gaining an extra Sequence
            // wrapper around an otherwise single-member nested sequence.
            var withoutAction = _converter.ConvertRule(
               new Rule("-", new IdGenerator())
                  .Say("hello")
                  .OptionallySay("jello")
            );

            var withAction = _converter.ConvertRule(
               new Rule("-", new IdGenerator())
                  .Say("hello")
                  .Optionally(
                     o => o.Say("jello").Do(() => Debug.WriteLine("noop"))
                  )
            );

            Assert.AreEqual(withoutAction.Symbols, withAction.Symbols);
        }


        [Test]
        public void ShouldConvert_Say_SayOneOf() {
            var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Id = 1,
            },
            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Alternative,
            },
            new() {
               Type = SymbolType.Word,
               Id = 2,
            },
            new() {
               Type = SymbolType.Word,
               Id = 3,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Alternative,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Sequence,
            }

            #endregion
         };

            var convertedRule = _converter.ConvertRule(
               new Rule("-", _generator)
                  .Say("hello")
                  .SayOneOf("abe", "bob")
            );

            Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
        }

        [Test]
        public void ShouldConvert_OptionallySay_SayOneOf() {
            var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Sequence,
            },
            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Optional,
            },
            new() {
               Type = SymbolType.Word,
               Id = 1,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Optional,
            },
            new() {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Alternative,
            },
            new() {
               Type = SymbolType.Word,
               Id = 2,
            },
            new() {
               Type = SymbolType.Word,
               Id = 3,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Alternative,
            },
            new() {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Sequence,
            },

            #endregion
         };

            var convertedRule = _converter.ConvertRule(
               new Rule("-", _generator)
                  .OptionallySay("hello")
                  .SayOneOf("abe", "bob")
            );

            Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
        }
    }
}
