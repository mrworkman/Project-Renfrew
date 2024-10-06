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

namespace Renfrew.Grammar {
   public class Id<TId, TInner> {
      protected Id(TId id, TInner inner) {
         Discriminant = id;
         Inner = inner;
      }

      public static Id<TId, TInner> Wrap(TInner obj, TId id) => new(id, obj);

      public TId Discriminant { get; }

      public TInner Inner { get; }
   }

   public class Id<TInner> : Id<uint, TInner> {
      private Id(uint id, TInner inner)
         : base(id, inner) {
      }

      public new static Id<TInner> Wrap(TInner obj, uint id) => new(id, obj);
   }
}
