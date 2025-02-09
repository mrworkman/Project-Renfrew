//// Project Renfrew
//// Copyright(C) 2024 Stephen Workman (workman.stephen@gmail.com)
////
//// This program is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 3 of the License, or
//// (at your option) any later version.
////
//// This program is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//// GNU General Public License for more details.
////
//// You should have received a copy of the GNU General Public License
//// along with this program.If not, see<http://www.gnu.org/licenses/>.
////

using System;
using System.Collections.Generic;
using System.Linq;
using Renfrew.Grammar.Dragon;
using Renfrew.Grammar.Dragon.SpeechRecognition;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi;

namespace Renfrew.Grammar {
   /// <summary>
   /// Converts from fluent rule structure to a structure NatSpeak understands.
   /// </summary>
   public class GrammarRuleConverter {
      private readonly Grammar _grammar;

      public GrammarRuleConverter(Grammar grammar) {
         _grammar = grammar;
      }

      public List<RuleInfo> Convert() {
         return _grammar.ExportedRules.Select(
               rule =>
                  new RuleInfo {
                     Id = rule.Id,
                     Symbols = ConvertCompositeExpression(
                        // FIXME: Remove cast.
                        (CompositeExpression) rule.Expression
                     ),
                  }
            )
            .ToList();
      }

      internal List<Symbol> ConvertCompositeExpression(
         CompositeExpression compositeExpression
      ) {
         var symbols = new List<Symbol>();

         var operationType = ConvertModifierToOperationType(
            compositeExpression.Modifier
         );

         symbols.Add(
            new Symbol {
               Type = SymbolType.StartOperation,
               Value = (uint) operationType,
            }
         );

         foreach (var subExpression in compositeExpression.SubExpressions) {
            switch (subExpression) {
               case CompositeExpression composite: {
                  ConvertCompositeExpression(composite);
                  break;
               }
               case RuleName rule: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.Rule,
                        Value = rule.Id,
                     }
                  );
                  break;
               }
               case Word word: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.Word,
                        Value = word.Id,
                     }
                  );
                  break;
               }
            }
         }

         symbols.Add(
            new Symbol {
               Type = SymbolType.EndOperation,
               Value = (uint) operationType,
            }
         );

         return symbols;
      }

      public OperationType ConvertModifierToOperationType(
         ExpressionModifier modifier
      ) {
         return modifier switch {
            ExpressionModifier.Alternatives => OperationType.Alternative,
            ExpressionModifier.Sequence => OperationType.Sequence,
            ExpressionModifier.Optionals => OperationType.Optional,
            ExpressionModifier.Repeated => OperationType.Repeat,
            _ => throw new ArgumentException($"Unexpected type {modifier}")
         };
      }

      //      public List<Symbol> ConvertRule(IElementContainer ruleContainer) {
      //         var symbols = new List<Symbol>();

      //         foreach (var ruleElement in ruleContainer.Elements) {
      //            if (ruleElement is IElementContainer container) {
      //               if (container.HasElements) {
      //                  //CreateOperationSymbols(container);
      //               } else {
      //                  symbols.AddRange(ConvertRule(container));
      //               }
      //               continue;
      //            }

      //            if (CreateRuleSymbol(ruleElement) is Symbol symbol) {
      //               symbols.Add(symbol);
      //            }
      //         }

      //         return symbols;
      //      }

      //      //public List<Symbol> CreateOperationSymbols(IElementContainer container) {
      //      //   var ruleSymbols = ConvertRule(container);

      //      //   if (container.Elements.Count() > 1) {
      //      //      var operation = new List<Symbol>();
      //      //      var sequenceType = (uint) GetSequenceType(container);

      //      //      operation.Add(new Symbol {
      //      //         Type = SymbolType.StartOperation,
      //      //         Value = sequenceType,
      //      //      });
      //      //      operation.AddRange(ruleSymbols);
      //      //      operation.Add(new Symbol {
      //      //         Type = SymbolType.EndOperation,
      //      //         Value = sequenceType,
      //      //      });

      //      //      return operation;
      //      //   }

      //      //   return ruleSymbols;
      //      //}

      //      public Symbol CreateRuleSymbol(IElement ruleElement) =>
      //         ruleElement switch {
      //            IGrammarAction => null,
      //            IRuleElement element => new Symbol {
      //               Type = SymbolType.Rule,
      //               Value = _grammar.Words[element.ToString()].Id,
      //            },
      //            IWordElement element => new Symbol {
      //               Type = SymbolType.Word,
      //               Value = _grammar.Words[element.ToString()].Id,
      //            },
      //            _ => throw new InvalidGrammarElementException(
      //               $"Unrecognized grammar element type '{ruleElement.GetType().Name}'"
      //            )
      //         };

      //      // I don't really like having to do this. Should def refactor!
      //      public OperationType GetSequenceType(IElementContainer container) =>
      //         container switch {
      //            IAlternatives => OperationType.Alternative,
      //            IOptionals => OperationType.Optional,
      //            IRepeats => OperationType.Repeat,
      //            ISequence => OperationType.Sequence,
      //            _ => throw new ArgumentException(
      //               $"Unexpected type '{container.GetType()}'", nameof(container)
      //            )
      //         };
   }
}
