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

namespace Renfrew.Grammar.Collections {
   internal class ListWalker<T> {
      private readonly List<T> _list;

      public ListWalker(List<T> list) {
         _list = list;
      }

      public T Current => _list[CurrentIndex];
      public int CurrentIndex { get; private set; }
      public bool IsAtEnd { get; private set; }

      public void MoveBack(int steps = 1) {
         if (steps <= 0) {
            throw new ArgumentOutOfRangeException(
               nameof(steps),
               "Negative numbers not allowed."
            );
         }

         if (CurrentIndex - steps < 0) {
            throw new IndexOutOfRangeException();
         }

         CurrentIndex -= steps;
         IsAtEnd = false;
      }

      public void MoveForward(int steps = 1) {
         if (steps <= 0) {
            throw new ArgumentOutOfRangeException(
               nameof(steps),
               "Negative numbers not allowed."
            );
         }

         if (CurrentIndex + steps >= _list.Count) {
            CurrentIndex = _list.Count;
            IsAtEnd = true;
         } else {
            CurrentIndex += steps;
         }
      }
   }
}
