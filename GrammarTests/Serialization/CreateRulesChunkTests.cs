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
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Serialization;
using Renfrew.Grammar.Serialization.HighLevelTypes;
using Renfrew.Grammar.Serialization.LowLevelTypes;

namespace GrammarTests.Serialization {
   internal class CreateRulesChunkTests {
      private Serializer _serializer;
      private IIdGenerator _idGenerator;

      [OneTimeSetUp]
      public void OneTimeSetup() {
         TestContext.AddFormatter<SrChunk>(
            o => JsonConvert.SerializeObject(
               o,
               new JsonSerializerSettings {
                  //TypeNameHandling = TypeNameHandling.Objects,
                  Formatting = Formatting.Indented
               }
            )
         );
      }

      [SetUp]
      public void Setup() {
         _serializer = new Serializer();
         _idGenerator = new IdGenerator();
      }

      [Test]
      public void ShouldHandleSimpleRuleWithImport() {
         var expectedResult = new SrChunk {
            ChunkId = (uint) Serializer.ChunkType.Rules,
            Rules = new List<ISerializableRule> {
               new SrCfgRule {
                  UniqueId = 1,
                  Symbols = new List<SrCfgSymbol> {
                     new() {
                        Type = (ushort) SymbolType.StartOperation,
                        Value = (uint) OperationType.Sequence,
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 1, // Word ID.
                     },
                     new() {
                        Type = (ushort) SymbolType.Rule,
                        Value = 2, // Rule ID.
                     },
                     new() {
                        Type = (ushort) SymbolType.EndOperation,
                        Value = (uint) OperationType.Sequence,
                     }
                  }
               }
            }
         };

         var actualResult = _serializer.CreateRulesChunk(
            new List<IRule> {
               new Rule("-", _idGenerator).Say("hello").WithRule("dgndictation")
            }
         );

         Assert.AreEqual(expectedResult, actualResult);
         Assert.AreEqual(40, actualResult.ChunkSize);
         Assert.AreEqual(40, actualResult.Rules[0].Size);
      }

      [Test]
      public void ShouldHandleSay_SayOneOf() {
         var expectedResult = new SrChunk {
            ChunkId = (uint) Serializer.ChunkType.Rules,
            Rules = new List<ISerializableRule> {
               new SrCfgRule {
                  UniqueId = 1,
                  Symbols = new List<SrCfgSymbol> {
                     new() {
                        Type = (ushort) SymbolType.StartOperation,
                        Value = (uint) OperationType.Sequence,
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 1, // Word ID (hello).
                     },
                     new() {
                        Type = (ushort) SymbolType.StartOperation,
                        Value = (uint) OperationType.Alternative,
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 2, // Word ID (abe).
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 3, // Word ID (bob).
                     },
                     new() {
                        Type = (ushort) SymbolType.EndOperation,
                        Value = (uint) OperationType.Alternative,
                     },
                     new() {
                        Type = (ushort) SymbolType.EndOperation,
                        Value = (uint) OperationType.Sequence,
                     }
                  }
               }
            }
         };

         var actualResult = _serializer.CreateRulesChunk(
            new List<IRule> {
               new Rule("-", _idGenerator)
                  .Say("hello")
                  .SayOneOf("abe", "bob")
            }
         );

         Assert.AreEqual(expectedResult, actualResult);
         Assert.AreEqual(64, actualResult.ChunkSize);
         Assert.AreEqual(64, actualResult.Rules[0].Size);
      }

      [Test]
      public void ShouldHandleSay_OptionallySay() {
         var expectedResult = new SrChunk {
            ChunkId = (uint) Serializer.ChunkType.Rules,
            Rules = new List<ISerializableRule> {
               new SrCfgRule {
                  UniqueId = 1,
                  Symbols = new List<SrCfgSymbol> {
                     new() {
                        Type = (ushort) SymbolType.StartOperation,
                        Value = (uint) OperationType.Sequence,
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 1, // Word ID (hello).
                     },
                     new() {
                        Type = (ushort) SymbolType.StartOperation,
                        Value = (uint) OperationType.Optional,
                     },
                     new() {
                        Type = (ushort) SymbolType.Word,
                        Value = 2, // Word ID (abe).
                     },
                     new() {
                        Type = (ushort) SymbolType.EndOperation,
                        Value = (uint) OperationType.Optional,
                     },
                     new() {
                        Type = (ushort) SymbolType.EndOperation,
                        Value = (uint) OperationType.Sequence,
                     }
                  }
               }
            }
         };

         var actualResult = _serializer.CreateRulesChunk(
            new List<IRule> {
               new Rule("-", _idGenerator)
                  .Say("hello")
                  .OptionallySay("abe")
            }
         );

         Assert.AreEqual(expectedResult, actualResult);
         Assert.AreEqual(56, actualResult.ChunkSize);
         Assert.AreEqual(56, actualResult.Rules[0].Size);
      }
   }
}
