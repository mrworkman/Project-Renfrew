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

using System.Collections.Generic;
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;

namespace Renfrew.Grammar.Solving {
   internal class Solver {
      private readonly Grammar _grammar;

      private readonly bool _isTrunkSequence;
      private readonly List<ISequenceMember> _members;
      private readonly ListWalker<SpokenWord> _phrase;
      private int _numberOfMatches;

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

      // TODO: Make sure the rule ID is checked to make sure we're looking at
      //  the right one.
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
            case Alternatives alternatives: {
               return VisitAlternatives(alternatives, memberIndex + 1);
            }
            case Optional optional: {
               return VisitOptional(optional, memberIndex + 1);
            }
            case Repeated repeated: {
               return VisitRepeated(repeated, memberIndex + 1);
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
            default: {
               throw new UnrecognizedMemberType(
                  _members[memberIndex].GetType()
               );
            }
         }

         _phrase.MoveForward();

         var result = VisitMember(memberIndex + 1);

         if (result is SolveResult.Failure) {
            _phrase.MoveBack();
         }

         return result;
      }

      public SolveResult VisitAlternatives(
         Alternatives alternatives,
         int memberIndex
      ) {
         foreach (var alternativeSequence in alternatives.Sequences) {
            // Visit the alternative sequence.
            var leftResult = VisitSequence(
               alternativeSequence,
               false,
               _phrase,
               _grammar
            );

            // Visit the next member of the current sequence.
            var rightResult = VisitMember(memberIndex);

            if (leftResult is SolveResult.Success
                && rightResult is SolveResult.Success) {
               return rightResult;
            }

            if (leftResult is SolveResult.Success leftSuccess) {
               _phrase.MoveBack(leftSuccess.NumberOfMatches);
            }
         }

         return SolveResult.Failed();
      }

      public SolveResult VisitOptional(Optional optional, int memberIndex) {
         // Visit the optional sequence.
         var leftResult = VisitSequence(
            optional.Sequence,
            false,
            _phrase,
            _grammar
         );

         // Visit the next member of the current sequence.
         var rightResult = VisitMember(memberIndex);

         if (rightResult is SolveResult.Success) {
            return rightResult;
         }

         if (leftResult is SolveResult.Success leftSuccess) {
            _phrase.MoveBack(leftSuccess.NumberOfMatches);

            rightResult = VisitMember(memberIndex);

            if (rightResult is SolveResult.Failure) {
               return SolveResult.Failed();
            }
         } else {
            return SolveResult.Failed();
         }

         return rightResult;
      }

      public SolveResult VisitRepeated(Repeated repeated, int memberIndex) {
         while (true) {
            // Visit the repeated sequence.
            var leftResult = VisitSequence(
               repeated.Sequence,
               false,
               _phrase,
               _grammar
            );

            if (leftResult is SolveResult.Failure) {
               return SolveResult.Failed();
            }

            // Visit the next member of the current sequence.
            var rightResult = VisitMember(memberIndex);

            if (rightResult is SolveResult.Success) {
               return rightResult;
            }
         }
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
