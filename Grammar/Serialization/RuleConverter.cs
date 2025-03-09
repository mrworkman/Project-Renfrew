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
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Serialization.HighLevelTypes;

namespace Renfrew.Grammar.Serialization {
   /// <summary>
   ///    Converts from fluent rule structure to a structure NatSpeak understands.
   /// </summary>
   public class RuleConverter {
      public List<RuleInfo> Convert(IReadOnlyList<IRule> rules) {
         return rules.Select(ConvertRule).ToList();
      }

      public RuleInfo ConvertRule(IRule rule) {
         return new RuleInfo {
            Id = rule.Id,
            Symbols = ConvertSequence(rule.Sequence)
         };
      }

      internal List<Symbol> ConvertSequence(Sequence sequence) {
         var symbols = new List<Symbol>();

         symbols.Add(
            new Symbol {
               Type = SymbolType.StartOperation,
               OperationType = OperationType.Sequence
            }
         );

         foreach (var sequenceMember in sequence.Members) {
            switch (sequenceMember) {
               case Alternatives alternatives: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.StartOperation,
                        OperationType = OperationType.Alternative
                     }
                  );

                  // Optimise for the case where all of the members are
                  // sequences made up of a single word.
                  if (alternatives.Sequences.All(
                         s => s.Members.Count == 1
                              && s.Members.All(m => m is Word)
                      )) {
                     symbols.AddRange(
                        alternatives.Sequences.Select(
                           s => new Symbol {
                              Type = SymbolType.Word,
                              Id = (s.Members.First() as Word).Id
                           }
                        )
                     );
                  } else {
                     foreach (var alternativeSequence in
                              alternatives.Sequences) {
                        symbols.AddRange(ConvertSequence(alternativeSequence));
                     }
                  }

                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.EndOperation,
                        OperationType = OperationType.Alternative
                     }
                  );
                  break;
               }
               case Optional optional: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.StartOperation,
                        OperationType = OperationType.Optional
                     }
                  );
                  symbols.AddRange(ConvertSequence(optional.Sequence));
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.EndOperation,
                        OperationType = OperationType.Optional
                     }
                  );
                  break;
               }
               case Repeated repeated: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.StartOperation,
                        OperationType = OperationType.Repeat
                     }
                  );
                  symbols.AddRange(ConvertSequence(repeated.Sequence));
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.EndOperation,
                        OperationType = OperationType.Repeat
                     }
                  );
                  break;
               }
               case RuleName ruleName: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.Rule,
                        Id = ruleName.Id
                     }
                  );
                  break;
               }
               case Word word: {
                  symbols.Add(
                     new Symbol {
                        Type = SymbolType.Word,
                        Id = word.Id
                     }
                  );
                  break;
               }
            }
         }

         symbols.Add(
            new Symbol {
               Type = SymbolType.EndOperation,
               OperationType = OperationType.Sequence
            }
         );

         return symbols;
      }

      public OperationType DetermineOperationType(
         ISequenceMember sequenceMember
      ) {
         return sequenceMember switch {
            Alternatives => OperationType.Alternative,
            Optional => OperationType.Optional,
            Repeated => OperationType.Repeat,
            _ => throw new ArgumentException(
               $"Unexpected type {sequenceMember.GetType()}"
            )
         };
      }
   }
}
