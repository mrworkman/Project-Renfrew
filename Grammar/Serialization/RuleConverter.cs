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

using System;
using System.Collections.Generic;
using System.Linq;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Serialization.HighLevelTypes;

namespace Renfrew.Grammar.Serialization {
   /// <summary>
   /// Converts from fluent rule structure to a structure NatSpeak understands.
   /// </summary>
   public class RuleConverter {
      public List<RuleInfo> Convert(IReadOnlyList<IRule> rules) {
         return rules.Select(ConvertRule).ToList();
      }

      public RuleInfo ConvertRule(IRule rule) {
         return new RuleInfo {
            Id = rule.Id,
            Symbols = ConvertCompositeExpression(rule.Expression),
         };
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
                  symbols.AddRange(ConvertCompositeExpression(composite));
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
   }
}
