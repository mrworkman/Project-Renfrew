// Project Renfrew
// Copyright(C) 2017 Stephen Workman (workman.stephen@gmail.com)
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
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;

namespace Renfrew.Grammar.Solving {
   internal class Solver {
      private readonly List<ISequenceMember> _members;
      private readonly ListWalker<SpokenWord> _phrase;
      private readonly Grammar _grammar;

      private readonly bool _isTrunkSequence;
      private int _numberOfMatches = 0;

      private Solver(
         Sequence sequence,
         bool isTrunkSequence,
         ListWalker<SpokenWord> phrase,
         Grammar grammar
      ) {
         _members = new List<ISequenceMember>(sequence.Members);
         _phrase = phrase;
         _grammar = grammar;
         _isTrunkSequence = isTrunkSequence;
      }

      public SolveResult VisitMember(int memberIndex) {
         if (memberIndex >= _members.Count) {
            if (_phrase.IsAtEnd) {
               return SolveResult.Succeeded(_numberOfMatches);
            }

            return _isTrunkSequence ?
               SolveResult.Failed() :
               SolveResult.Succeeded(_numberOfMatches);
         }

         if (_phrase.IsAtEnd && _members[memberIndex] is not Optional) {
            return SolveResult.Failed();
         }

         switch (_members[memberIndex]) {
            case Optional optional: {
               return VisitOptional(optional, memberIndex + 1);
            }
            case Word word: {
               if (word.Id == _phrase.Current.WordId
                   && word.String == _phrase.Current.Word) {
                  _numberOfMatches++;
               } else {
                  return SolveResult.Failed();
               }

               break;
            }
         }

         _phrase.MoveForward();
         return VisitMember(memberIndex + 1);
      }

      public SolveResult VisitOptional(Optional optional, int memberIndex) {
         var leftResult = VisitSequence(
            optional.Sequence,
            false,
            _phrase,
            _grammar
         );
         var rightResult = VisitMember(memberIndex);

         if (rightResult is SolveResult.Success) {
            return rightResult;
         }

         if (leftResult is SolveResult.Success success) {
            _phrase.MoveBack(success.NumberOfMatches);

            rightResult = VisitMember(memberIndex);

            if (rightResult is SolveResult.Failure) {
               return SolveResult.Failed();
            }
         } else {
            return SolveResult.Failed();
         }

         // Success.
         return rightResult;
      }

      public static SolveResult VisitSequence(
         Sequence sequence,
         bool isTrunkSequence,
         ListWalker<SpokenWord> phrase,
         Grammar grammar
      ) {
         return new Solver(sequence, isTrunkSequence, phrase, grammar)
            .VisitMember(0);
      }
   }
}
