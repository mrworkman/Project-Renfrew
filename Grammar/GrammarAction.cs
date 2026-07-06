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
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;

namespace Renfrew.Grammar {
    /// <summary>
    ///    A <see cref="Do(System.Action)" />-registered action. It lives in a
    ///    rule's sequence as a zero-width member: it matches no spoken words, but
    ///    when the solver reaches it on a matching path it is collected and later
    ///    invoked with the words its owning rule consumed.
    /// </summary>
    public class GrammarAction : ISequenceMember {
        private Action _action;
        private Action<IEnumerable<string>> _actionWithWords;

        public GrammarAction(Action action) {
            _action = action;
        }

        public GrammarAction(Action<IEnumerable<string>> action) {
            _actionWithWords = action;
        }

        public void InvokeAction(IEnumerable<string> words) {
            // Call the parameterless form?
            if (_action != null) {
                _action();
                return;
            }

            // Call the parametered form
            _actionWithWords(words);
        }

        public override string ToString() => "Grammar Action";
    }
}
