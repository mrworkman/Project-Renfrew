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
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;

namespace Renfrew.Grammar.FluentApi {
   internal class Rule : IRule {
      private readonly IIdGenerator _idGenerator;

      private readonly CompositeExpression _expression =
         CompositeExpression.Create(
            ExpressionModifier.Sequence
         );

      private readonly Dictionary<string, Word> _words = new(
         StringComparer.CurrentCultureIgnoreCase
      );

      /// <summary>
      /// Numeric rule identifier.
      /// </summary>
      public uint Id { get; }

      /// <summary>
      /// The rule's name.
      /// </summary>
      public string String { get; }

      public CompositeExpression Expression => _expression;

      public IReadOnlyList<Word> Words => _words.Select(entry => entry.Value)
         .OrderBy(word => word.Id)
         .ToList();

      internal Rule(
         string name,
         IIdGenerator idGenerator
      ) {
         _idGenerator = idGenerator
                        ?? throw new ArgumentNullException(
                           nameof(idGenerator)
                        );

         String = name ?? throw new ArgumentNullException(nameof(name));
         Id = _idGenerator.GetRuleId(String);
      }

      private void AdoptWordsFromRule(Rule r) {
         foreach (var kvp in r._words) {
            if (!_words.ContainsKey(kvp.Key)) {
               _words.Add(kvp.Key, kvp.Value);
            }
         }
      }

      private void InvokeWithNestedRule(
         Expression<Action<IRule>> action,
         CompositeExpression wrapper,
         bool addWrapperToRuleExpression = true
      ) {
         var nestedRule = new Rule("-", _idGenerator);

         action.Compile()(nestedRule);
         wrapper.AddExpression(nestedRule._expression);

         AdoptWordsFromRule(nestedRule);

         if (addWrapperToRuleExpression) {
            _expression.AddExpression(wrapper);
         }
      }

      public IActionableRule OneOf(params Expression<Action<IRule>>[] actions) {
         var wrapper = CompositeExpression.Create(
            ExpressionModifier.Alternatives
         );

         foreach (var action in actions) {
            InvokeWithNestedRule(
               action,
               wrapper,
               addWrapperToRuleExpression: false
            );
         }

         _expression.AddExpression(wrapper);

         return (ActionableRule) this;
      }

      public IActionableRule Optionally(Expression<Action<IRule>> action) {
         InvokeWithNestedRule(
            action,
            CompositeExpression.Create(
               ExpressionModifier.Optionals
            )
         );

         return (ActionableRule) this;
      }

      public IActionableRule OptionallyOneOf(
         params Expression<Action<IRule>>[] actions
      ) {
         return Optionally(r => r.OneOf(actions));
      }

      public IActionableRule OptionallySay(string word) {
         return Optionally(r => r.Say(word));
      }

      public IActionableRule OptionallyWithRule(string ruleName) {
         return Optionally(r => r.WithRule(ruleName));
      }

      // Repeats: A+
      public IActionableRule Repeat(Expression<Action<IRule>> action) {
         InvokeWithNestedRule(
            action,
            CompositeExpression.Create(
               ExpressionModifier.Repeated
            )
         );

         return (ActionableRule) this;
      }

      // Repeats + Alternatives: ( A | B | C )+
      public IActionableRule RepeatOneOf(
         params Expression<Action<IRule>>[] actions
      ) {
         return Repeat(r => r.OneOf(actions));
      }

      public IActionableRule Say(string word) {
         var wordExpr = Word.Create(_idGenerator.GetWordId(word), word);

         if (!_words.ContainsKey(word)) {
            _words.Add(word, wordExpr);
         }

         _expression.AddExpression(wordExpr);

         return (ActionableRule) this;
      }

      public IActionableRule SayOneOf(params string[] words) {
         return SayOneOf(words as IEnumerable<string>);
      }

      public IActionableRule SayOneOf(IEnumerable<string> words) {
         var alternatives = CompositeExpression.Create(
            ExpressionModifier.Alternatives
         );

         foreach (var word in words) {
            var wordExpr = Word.Create(_idGenerator.GetWordId(word), word);

            if (!_words.ContainsKey(word)) {
               _words.Add(word, wordExpr);
            }

            alternatives.AddExpression(wordExpr);
         }

         _expression.AddExpression(alternatives);

         return (ActionableRule) this;
      }

      public IActionableRule WithRule(string ruleName) {
         _expression.AddExpression(
            RuleName.Create(_idGenerator.GetRuleId(ruleName), ruleName)
         );

         return (ActionableRule) this;
      }

      public bool Equals(IIdString other) {
         return Id == other?.Id && String == other.String;
      }
   }
}
