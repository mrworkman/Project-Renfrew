using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Renfrew.Grammar;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.Serialization;
using Renfrew.Grammar.Serialization.HighLevelTypes;

namespace GrammarTests.SerializerTests {
   [TestFixture]
   internal class RuleConverterTests {
      private IdGenerator _generator;
      private RuleConverter _converter = new();

      [SetUp]
      public void Setup() {
         _generator = new();
      }

      [Test]
      public void ConvertSimpleSay() {
         var rule = new Rule("-", _generator);

         rule.Say("hello");

         var convertedRule = _converter.ConvertRule(rule);

         var expectedSymbols = new List<Symbol> {
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
         };

         Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
      }

      [Test]
      public void ConvertSimpleSayOptionallySay() {
         var rule = new Rule("-", _generator);

         rule.Say("hello").OptionallySay("jello");

         var convertedRule = _converter.ConvertRule(rule);

         var expectedSymbols = new List<Symbol> {
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
               Type = SymbolType.Word,
               Value = 2,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Optional,
            },
            new() {
               Type = SymbolType.EndOperation,
               Value = (uint) OperationType.Sequence,
            },
         };

         Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
      }


      [Test]
      public void ConvertSaySayOneOf() {
         var rule = new Rule("-", _generator);

         rule.Say("hello").SayOneOf("abe", "bob");

         var convertedRule = _converter.ConvertRule(rule);

         var expectedSymbols = new List<Symbol> {
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
            },
         };

         Assert.AreEqual(expectedSymbols, convertedRule.Symbols);
      }
   }
}
