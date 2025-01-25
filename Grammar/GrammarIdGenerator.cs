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

using System.Collections.Generic;

using Renfrew.Grammar.FluentApi.Interfaces;

namespace Renfrew.Grammar {
   internal class GrammarIdGenerator : IIdGenerator {
      private readonly Dictionary<string, int> _listNames = new();
      private readonly Dictionary<string, int> _ruleNames = new();
      private readonly Dictionary<string, int> _words = new();

      private int _currentListNameId = 1;
      private int _currentRuleNameId = 1;
      private int _currentWordId = 1;

      public int GetListId(string listName) {
         var lower = listName.ToLowerInvariant();

         if (!_listNames.ContainsKey(lower)) {
            _listNames[lower] = _currentListNameId++;
         }

         return _listNames[lower];
      }

      public int GetRuleId(string ruleName) {
         var lower = ruleName.ToLowerInvariant();

         if (!_ruleNames.ContainsKey(lower)) {
            _ruleNames[lower] = _currentRuleNameId++;
         }

         return _ruleNames[lower];
      }

      public int GetWordId(string word) {
         var lower = word.ToLowerInvariant();

         if (!_words.ContainsKey(lower)) {
            _words[lower] = _currentWordId++;
         }

         return _words[lower];
      }
   }
}
