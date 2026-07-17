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
using System.Linq;

namespace Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers {
    public class Alternatives : ISequenceMember, IEquatable<Alternatives> {
        private Alternatives() { }

        public List<Sequence> Sequences { get; private set; }

        public bool Equals(Alternatives other) {
            if (other is null) {
                return false;
            }

            if (Sequences.Count != other.Sequences.Count) {
                return false;
            }

            return !Sequences.Where((t, i) => !t.Equals(other.Sequences[i])).Any();
        }

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

        internal static Alternatives Create(
           IEnumerable<ISequenceMember> sequenceMembers
        ) {
            return Create(sequenceMembers.Select(Sequence.Create));
        }

        internal static Alternatives Create(
           ISequenceMember sequenceMember,
           params ISequenceMember[] additionalSequenceMembers
        ) {
            return Create(
               Sequence.Create(sequenceMember),
               additionalSequenceMembers.Select(Sequence.Create).ToArray()
            );
        }

        internal void Add(Sequence sequence) {
            Sequences.Add(sequence);
        }
    }
}
