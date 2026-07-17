// Project Renfrew
// Copyright(C) 2026 Stephen Workman (workman.stephen@gmail.com)
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

namespace Renfrew.Grammar.Parsing {
    /// <summary>
    ///    An action found on a matching path, paired with the words its owning
    ///    rule consumed. Produced by the <see cref="Parser" /> and invoked by
    ///    <see cref="Grammar.InvokeRule" />.
    /// </summary>
    internal class MatchedAction {
        public MatchedAction(GrammarAction action, IReadOnlyList<string> words) {
            Action = action;
            Words = words;
        }

        public GrammarAction Action { get; }
        public IReadOnlyList<string> Words { get; }
    }
}
