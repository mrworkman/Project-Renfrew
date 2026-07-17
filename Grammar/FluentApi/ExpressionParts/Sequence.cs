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
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.Types;

namespace Renfrew.Grammar.FluentApi.ExpressionParts {
    public class Sequence : IEquatable<Sequence> {
        private readonly List<ISequenceMember> _members = new();
        internal Sequence() { }

        public IReadOnlyList<ISequenceMember> Members => _members.AsReadOnly();

        public bool Equals(Sequence other) {
            if (other is null) {
                return false;
            }

            if (_members.Count != other._members.Count) {
                return false;
            }

            for (var i = 0; i < _members.Count; i++) {
                var left = _members[i];
                var right = other._members[i];

                if (left.GetType() != right.GetType()) {
                    return false;
                }

                switch (left) {
                    case IIdString term: {
                        if (!term.Equals(right as IIdString)) {
                            return false;
                        }

                        break;
                    }
                    case Alternatives alternatives: {
                        if (!alternatives.Equals(right as Alternatives)) {
                            return false;
                        }

                        break;
                    }
                    case Optional optional: {
                        if (!optional.Equals(right as Optional)) {
                            return false;
                        }

                        break;
                    }
                    case Repeated repeated: {
                        if (!repeated.Equals(right as Repeated)) {
                            return false;
                        }

                        break;
                    }
                    case GrammarAction: {
                        // Actions are behavioral, not structural: two sequences
                        // that differ only in their (non-serializable) action
                        // delegates are still considered equal.
                        break;
                    }
                    default: {
                        if (!left.Equals(right)) {
                            return false;
                        }

                        break;
                    }
                }
            }

            return true;
        }

        internal static Sequence Create(ISequenceMember sequenceMember) {
            var sequence = new Sequence();
            sequence._members.Add(sequenceMember);
            return sequence;
        }

        internal static Sequence Create(
           ISequenceMember sequenceMember,
           params ISequenceMember[] additionalSequenceMembers
        ) {
            var sequence = new Sequence();
            sequence._members.Add(sequenceMember);
            sequence._members.AddRange(additionalSequenceMembers);
            return sequence;
        }

        internal static Sequence Create(
           IEnumerable<ISequenceMember> sequenceMembers
        ) {
            var sequence = new Sequence();
            sequence._members.AddRange(sequenceMembers);
            return sequence;
        }

        internal void AddMember(ISequenceMember member) {
            _members.Add(member);
        }
    }
}
