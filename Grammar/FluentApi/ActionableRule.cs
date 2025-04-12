// Project Renfrew
// Copyright(C) 2016  Stephen Workman (workman.stephen@gmail.com)
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
using System.Linq.Expressions;
using Renfrew.Grammar.FluentApi.ExpressionParts;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.Grammar.Types;

namespace Renfrew.Grammar.FluentApi {
   internal class ActionableRule : IActionableRule {
      private readonly Rule _rule;

      private ActionableRule(Rule baseRule) {
         _rule = baseRule;
      }

      public uint Id => _rule.Id;
      public string String => _rule.String;

      public Sequence Sequence => _rule.Sequence;
      public IReadOnlyList<Word> Words => _rule.Words;

      public IRule Do(Action action) {
         // FIXME: Support actions.
         //_rule.AddElementToContainer( new GrammarAction(action) );
         return _rule;
      }

      public IRule Do(Action<IEnumerable<string>> action) {
         // FIXME: Support actions.
         //_rule.AddElementToContainer( new GrammarAction(action) );
         return _rule;
      }

      public bool Equals(IIdString other) {
         return Id == other?.Id && String == other.String;
      }

      public static explicit operator ActionableRule(Rule rule) {
         return new ActionableRule(rule);
      }

      #region Defer to Base Rule

      public IActionableRule OneOf(params Expression<Action<IRule>>[] actions) {
         return _rule.OneOf(actions);
      }

      public IActionableRule Optionally(Expression<Action<IRule>> action) {
         return _rule.Optionally(action);
      }

      public IActionableRule OptionallyOneOf(
         params Expression<Action<IRule>>[] actions
      ) {
         return _rule.OptionallyOneOf(actions);
      }

      public IActionableRule OptionallySay(string word) {
         return _rule.OptionallySay(word);
      }

      public IActionableRule OptionallyWithRule(string ruleName) {
         return _rule.OptionallyWithRule(ruleName);
      }

      public IActionableRule Repeat(Expression<Action<IRule>> action) {
         return _rule.Repeat(action);
      }

      public IActionableRule RepeatOneOf(
         params Expression<Action<IRule>>[] actions
      ) {
         return _rule.RepeatOneOf(actions);
      }

      public IActionableRule Say(string word) {
         return _rule.Say(word);
      }

      public IActionableRule Say(string word, string[] additionalWords) {
         return _rule.Say(word, additionalWords);
      }

      public IActionableRule SayOneOf(params string[] words) {
         return _rule.SayOneOf(words);
      }

      public IActionableRule SayOneOf(IEnumerable<string> words) {
         return _rule.SayOneOf(words);
      }

      public IActionableRule WithRule(string ruleName) {
         return _rule.WithRule(ruleName);
      }

      #endregion
   }
}
