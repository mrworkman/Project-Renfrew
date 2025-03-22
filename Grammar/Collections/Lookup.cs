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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Renfrew.Grammar.Types;

namespace Renfrew.Grammar.Collections {
   internal class Lookup<TValue> : IEnumerable<TValue>
      where TValue: IIdString {
      private readonly Dictionary<uint, TValue> _dictById;
      private readonly Dictionary<string, TValue> _dictByString;

      [SuppressMessage("ReSharper", "ConvertConstructorToMemberInitializers")]
      public Lookup() {
         _dictByString =
            new Dictionary<string, TValue>(
               StringComparer.CurrentCultureIgnoreCase
            );
         _dictById = new Dictionary<uint, TValue>();
      }

      public Dictionary<string, TValue>.KeyCollection Keys =>
         _dictByString.Keys;

      public Dictionary<string, TValue>.ValueCollection Values =>
         _dictByString.Values;

      public IEnumerator<TValue> GetEnumerator() {
         return Values.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator() {
         return GetEnumerator();
      }

      public void Add(TValue idString) {
         if (_dictById.ContainsKey(idString.Id)) {
            return;
         }

         _dictById.Add(idString.Id, idString);
         _dictByString.Add(idString.String, idString);
      }

      public bool ContainsKey(string key) {
         return _dictByString.ContainsKey(key);
      }

      public bool ContainsKey(uint key) {
         return _dictById.ContainsKey(key);
      }

      public TValue Get(string key) {
         return _dictByString[key];
      }

      public TValue Get(uint key) {
         return _dictById[key];
      }

      public bool Remove(IIdString idString) {
         return _dictById.Remove(idString.Id)
                && _dictByString.Remove(idString.String);
      }

      public bool Remove(string key) {
         var exists = _dictByString.TryGetValue(key, out var idString);

         return exists && Remove(idString);
      }

      public bool Remove(uint key) {
         var exists = _dictById.TryGetValue(key, out var idString);

         return exists && Remove(idString);
      }
   }
}
