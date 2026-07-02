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
                Symbols = ConvertSequence(rule.Sequence, optimize: false)
            };
        }

        private List<Symbol> ConvertAlternatives(Alternatives alternatives) {
            return WrapOperation(
               OperationType.Alternative,
               symbols => {
                   foreach (var alternativeSequence in alternatives.Sequences) {
                       symbols.AddRange(ConvertSequence(alternativeSequence));
                   }

                   return symbols;
               }
            );
        }

        private List<Symbol> ConvertOptional(Optional optional) {
            return WrapOperation(
               OperationType.Optional,
               _ => ConvertSequence(optional.Sequence)
            );
        }

        private List<Symbol> ConvertRepeated(Repeated repeated) {
            return WrapOperation(
               OperationType.Repeat,
               _ => ConvertSequence(repeated.Sequence)
            );
        }

        private Symbol ConvertRuleName(RuleName ruleName) {
            return new Symbol {
                Type = SymbolType.Rule,
                Id = ruleName.Id
            };
        }

        private List<Symbol> ConvertSequence(
           Sequence sequence,
           bool optimize = true
        ) {
            // Optimizing will avoid wrapping symbols in a start/end sequence
            // operation if there is only one member in the Sequence. Dragon
            // supports either way though. TODO: Ignore ACTION members.
            if (optimize && sequence.Members.Count == 1) {
                return ConvertSequence(sequence, symbols: null);
            }

            return WrapOperation(
               OperationType.Sequence,
               symbols => ConvertSequence(sequence, symbols)
            );
        }

        private List<Symbol> ConvertSequence(
           Sequence sequence,
           List<Symbol> symbols
        ) {
            symbols ??= new List<Symbol>();

            foreach (var sequenceMember in sequence.Members) {
                switch (sequenceMember) {
                    case Alternatives alternatives: {
                        symbols.AddRange(ConvertAlternatives(alternatives));
                        break;
                    }
                    case Optional optional: {
                        symbols.AddRange(ConvertOptional(optional));
                        break;
                    }
                    case Repeated repeated: {
                        symbols.AddRange(ConvertRepeated(repeated));
                        break;
                    }
                    case RuleName ruleName: {
                        symbols.Add(ConvertRuleName(ruleName));
                        break;
                    }
                    case Word word: {
                        symbols.Add(ConvertWord(word));
                        break;
                    }
                }
            }

            return symbols;
        }

        private Symbol ConvertWord(Word word) {
            return new Symbol {
                Type = SymbolType.Word,
                Id = word.Id
            };
        }

        private List<Symbol> WrapOperation(
           OperationType operationType,
           Func<List<Symbol>, List<Symbol>> func
        ) {
            var symbols = new List<Symbol> {
            new() {
               Type = SymbolType.StartOperation,
               OperationType = operationType
            }
         };

            symbols.AddRange(func(new List<Symbol>()));

            symbols.Add(
               new Symbol {
                   Type = SymbolType.EndOperation,
                   OperationType = operationType
               }
            );

            return symbols;
        }
    }
}
