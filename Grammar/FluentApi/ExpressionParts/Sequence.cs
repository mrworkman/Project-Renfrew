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

namespace Renfrew.Grammar.FluentApi.ExpressionParts {
   public class Sequence : IEquatable<Sequence> {
      private readonly List<ISequenceMember> _members = new();
      internal Sequence() { }

      internal IReadOnlyList<ISequenceMember> Members => _members.AsReadOnly();

      public bool Equals(Sequence other) {
         if (other is null) {
            return false;
         }

         if (_members.Count != other._members.Count) {
            return false;
         }

         // TODO: Is this loop actually necessary?
         for (var i = 0; i < _members.Count; i++) {
            var left = _members[i];
            var right = other._members[i];

            // TODO: Consider.
            if (left.GetType() != right.GetType()) {
               return false;
            }

            if (!left.Equals(right)) {
               return false;
            }
         }

         return true;
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
