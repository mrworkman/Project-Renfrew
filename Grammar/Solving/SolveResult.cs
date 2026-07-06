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

namespace Renfrew.Grammar.Solving {
    internal class SolveResult {
        private SolveResult() { }

        /// <summary>
        ///    A bare success used internally to signal that a matching path was
        ///    found. The actions are attached once, by the top-level solve.
        /// </summary>
        public static SolveResult Succeeded() {
            return new Success(new List<MatchedAction>());
        }

        public static SolveResult Succeeded(IReadOnlyList<MatchedAction> actions) {
            return new Success(actions);
        }

        public static SolveResult Failed() {
            return new Failure();
        }

        public class Failure : SolveResult { }

        public class Success : SolveResult {
            internal Success(IReadOnlyList<MatchedAction> actions) {
                Actions = actions;
            }

            /// <summary>
            ///    The actions found along the matching path, in the order they
            ///    should be invoked.
            /// </summary>
            public IReadOnlyList<MatchedAction> Actions { get; }
        }
    }
}
