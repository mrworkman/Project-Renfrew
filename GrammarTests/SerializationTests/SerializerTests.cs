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
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Serialization;
using Renfrew.Grammar.Serialization.HighLevelTypes;
using Renfrew.Grammar.Serialization.LowLevelTypes;
using Renfrew.NatSpeakInterop;

namespace GrammarTests.SerializationTests {
    [TestFixture]
    internal class SerializerTests {
        [OneTimeSetUp]
        public void OneTimeSetup() {
            var settings = new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.Indented
            };

            TestContext.AddFormatter<SrHeader>(
               o => JsonConvert.SerializeObject(o, settings)
            );
            TestContext.AddFormatter<SrChunk>(
               o => JsonConvert.SerializeObject(o, settings)
            );
        }

        [SetUp]
        public void Setup() {
            _serializer = new Serializer();

            _grammar = new TestGrammar(
               new Mock<IGrammarService>().Object,
               new Mock<INatSpeak>().Object
            );
        }

        private class TestGrammar : Grammar {
            #region Boilerplate

            public TestGrammar(
               IGrammarService grammarService,
               INatSpeak natSpeak
            ) :
               base(grammarService, natSpeak) { }

            public override void Dispose() { }
            public override void Initialize() { }

            public void Initialize(string ruleName, Func<IRule, IRule> ruleFunc) {
                AddRule(ruleName, ruleFunc);
            }

            #endregion
        }

        private Serializer _serializer;
        private TestGrammar _grammar;

        [Test]
        public void Repeat_SayOneOf_SayOneOf() {
            var expectedChunks = new List<SrChunk> {
            #region Expected Chunks

            new() {
               ChunkId = (uint) Serializer.ChunkType.ExportRules,
               Rules = new List<ISerializableRule> {
                  new SrCfgXRule {
                     RuleNumber = 1,
                     String = "test_rule"
                  }
               }
            },
            new() {
               ChunkId = (uint) Serializer.ChunkType.Words,
               Rules = new List<ISerializableRule> {
                  new SrCfgXRule {
                     RuleNumber = 1,
                     String = "alpha"
                  },
                  new SrCfgXRule {
                     RuleNumber = 2,
                     String = "bravo"
                  },
                  new SrCfgXRule {
                     RuleNumber = 3,
                     String = "charlie"
                  },
                  new SrCfgXRule {
                     RuleNumber = 4,
                     String = "one"
                  },
                  new SrCfgXRule {
                     RuleNumber = 5,
                     String = "two"
                  }
               }
            },
            new() {
               ChunkId = (uint) Serializer.ChunkType.Rules,
               Rules = new List<ISerializableRule> {
                  new SrCfgRule {
                     UniqueId = 1,
                     Symbols = new List<SrCfgSymbol> {
                        new() {
                           Type = (ushort) SymbolType.StartOperation,
                           Value = (uint) OperationType.Sequence
                        },
                        new() {
                           Type = (ushort) SymbolType.StartOperation,
                           Value = (uint) OperationType.Repeat
                        },
                        new() {
                           Type = (ushort) SymbolType.StartOperation,
                           Value = (uint) OperationType.Sequence
                        },
                        // First SayOneOf().
                        new() {
                           Type = (ushort) SymbolType.StartOperation,
                           Value = (uint) OperationType.Alternative
                        },
                        new() {
                           Type = (ushort) SymbolType.Word,
                           Value = 1
                        },
                        new() {
                           Type = (ushort) SymbolType.Word,
                           Value = 2
                        },
                        new() {
                           Type = (ushort) SymbolType.Word,
                           Value = 3
                        },
                        new() {
                           Type = (ushort) SymbolType.EndOperation,
                           Value = (uint) OperationType.Alternative
                        },
                        // Second SayOneOf().
                        new() {
                           Type = (ushort) SymbolType.StartOperation,
                           Value = (uint) OperationType.Alternative
                        },
                        new() {
                           Type = (ushort) SymbolType.Word,
                           Value = 4
                        },
                        new() {
                           Type = (ushort) SymbolType.Word,
                           Value = 5
                        },
                        new() {
                           Type = (ushort) SymbolType.EndOperation,
                           Value = (uint) OperationType.Alternative
                        },
                        new() {
                           Type = (ushort) SymbolType.EndOperation,
                           Value = (uint) OperationType.Sequence
                        },
                        new() {
                           Type = (ushort) SymbolType.EndOperation,
                           Value = (uint) OperationType.Repeat
                        },
                        new() {
                           Type = (ushort) SymbolType.EndOperation,
                           Value = (uint) OperationType.Sequence
                        }
                     }
                  }
               }
            }

            #endregion
         };

            _grammar.Initialize(
               "test_rule",
               rule => rule
                  .Repeat(
                     command => command
                        .SayOneOf("alpha", "bravo", "charlie")
                        .SayOneOf("one", "two")
                  )
            );


            var (actualHeader, actualChunks) =
               _serializer.CreateDataStructures(_grammar);

            Assert.AreEqual(
               new SrHeader {
                   Flags = (uint)Serializer.SrHeaderFlags.Unicode
               },
               actualHeader
            );
            Assert.AreEqual(expectedChunks, actualChunks);
        }
    }
}
