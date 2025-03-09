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

namespace Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers {
   public class Alternatives : ISequenceMember {
      private Alternatives() { }

      internal List<Sequence> Sequences { get; private set; }

      internal static Alternatives Create(Sequence sequence) {
         return new Alternatives {
            Sequences = new List<Sequence> {
               sequence
            }
         };
      }

      internal static Alternatives Create(
         Sequence sequence,
         params Sequence[] additionalSequences
      ) {
         var alternatives = Create(sequence);
         alternatives.Sequences.AddRange(additionalSequences);

         return alternatives;
      }

      internal static Alternatives Create(IEnumerable<Sequence> sequences) {
         var alternatives = new Alternatives {
            Sequences = new List<Sequence>(sequences)
         };

         if (alternatives.Sequences.Count == 0) {
            throw new ArgumentException(
               "At least one sequence must be provided.",
               nameof(sequences)
            );
         }

         return alternatives;
      }


      internal void Add(Sequence sequence) {
         Sequences.Add(sequence);
      }
   }
}
