// Project Renfrew
// Copyright(C) 2024 Stephen Workman (workman.stephen@gmail.com)
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
using System.Linq;

using Renfrew.Grammar.Dragon;
using Renfrew.Grammar.Elements;
using Renfrew.Grammar.Exceptions;

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
         return _grammar.AllRules.Values.Select(rule => 
            new RuleInfo {
               Id = rule.Discriminant,
               Symbols = ConvertRule(rule.Inner.Elements),
            }
         ).ToList();
      }

      public List<RuleSymbol> ConvertRule(IElementContainer ruleContainer) {
         var symbols = new List<RuleSymbol>();

         foreach (var ruleElement in ruleContainer.Elements) {
            if (ruleElement is IElementContainer container) {
               if (container.HasElements) {
                  CreateOperationSymbols(container);
               } else {
                  symbols.AddRange(ConvertRule(container));
               }
               continue;
            }

            if (CreateRuleSymbol(ruleElement) is RuleSymbol symbol) {
               symbols.Add(symbol);
            }
         }

         return symbols;
      }

      public List<RuleSymbol> CreateOperationSymbols(IElementContainer container) {
         var symbols = new List<RuleSymbol>();

         symbols.Add(new RuleSymbol {
            Type = SymbolType.StartOperation,
            Value = (uint)SymbolOperation.Sequence,
         });
         symbols.AddRange(ConvertRule(container));
         symbols.Add(new RuleSymbol {
            Type = SymbolType.EndOperation,
            Value = (uint)SymbolOperation.Sequence,
         });

         return symbols;
      }

      public RuleSymbol CreateRuleSymbol(IElement ruleElement) =>
         ruleElement switch {
            IGrammarAction => null,
            IRuleElement element => new RuleSymbol {
               Type = SymbolType.Rule,
               Value = _grammar.Words[element.ToString()].Discriminant,
            },
            IWordElement element => new RuleSymbol {
               Type = SymbolType.Word,
               Value = _grammar.Words[element.ToString()].Discriminant,
            },
            _ => throw new InvalidGrammarElementException(
               $"Unrecognized grammar element type '{ruleElement.GetType().Name}'"
            )
         };

   }
}
