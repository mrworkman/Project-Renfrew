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
// along with this program. If not, see<http://www.gnu.org/licenses/>.
//

namespace Renfrew.Grammar.FluentApi {
   public enum ExpressionModifier {
      /// <summary>
      /// Any ONE of the terms or sub-expressions can be spoken to match the
      /// expression.
      /// </summary>
      Alternatives,

      /// <summary>
      /// Same as <b><see cref="Sequence"/></b>, but the whole sequence is
      /// optional.
      /// </summary>
      Optionals,

      /// <summary>
      /// Same as <b><see cref="Sequence"/></b>, but the whole sequence of terms
      /// can be repeated.
      /// </summary>
      Repeated,

      /// <summary>
      /// All of the terms and sub-expressions in the sequence must be spoken,
      /// in the order defined, so that the expression will be matched.
      /// </summary>
      Sequence,
   }
}
