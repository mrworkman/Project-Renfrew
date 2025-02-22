using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.Serialization;
using Renfrew.Grammar.Serialization.HighLevelTypes;
using Renfrew.Grammar.Serialization.LowLevelTypes;

namespace GrammarTests.Serialization {
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
      public void ShouldConvertSimpleSay() {
         var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Value = 1,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
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
      public void ConvertSimpleSayOptionallySay() {
         var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Value = 1,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Optional,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Value = 2,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Optional,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
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
      public void ShouldConvertSaySayOneOf() {
         var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Value = 1,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Alternative,
            },
            new() {
               Type = SymbolType.Word,
               Value = 2,
            },
            new() {
               Type = SymbolType.Word,
               Value = 3,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Alternative,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
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
      public void ShouldConvertOptionallySay_SayOneOf() {
         var expectedSymbols = new List<Symbol> {
            #region Expected Symbols

            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Optional,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.Word,
               Value = 1,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Optional,
            },
            new() {
               Type = SymbolType.StartOperation,
               Value = (uint) OperationType.Alternative,
            },
            new() {
               Type = SymbolType.Word,
               Value = 2,
            },
            new() {
               Type = SymbolType.Word,
               Value = 3,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Alternative,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
            }

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
