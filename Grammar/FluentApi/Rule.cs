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

using Renfrew.Grammar.FluentApi.Interfaces;

namespace Renfrew.Grammar.FluentApi {
   internal class Rule : IRule {
      private CompositeExpression _expression;

      internal Rule(string name) {
         Name = name ?? throw new ArgumentNullException(nameof(name));
      }

      public string Name { get; }

      public IExpression Expression => _expression;

      public IActionableRule OneOf(params Expression<Action<IRule>>[] actions) {
         var nestedRule = new Rule("-") {
            _expression = CompositeExpression.Create(
               ExpressionModifier.Alternatives
            )
         };

         foreach (var action in actions) {
            action.Compile()(nestedRule);
         }

         _expression.AddExpression(nestedRule._expression);

         return (ActionableRule) this;
      }

      public IActionableRule Optionally(Expression<Action<IRule>> action) {
         var nestedRule = new Rule("-") {
            _expression = CompositeExpression.Create(
               ExpressionModifier.Optionals
            )
         };

         action.Compile()(nestedRule);

         _expression.AddExpression(nestedRule._expression);

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
         var nestedRule = new Rule("-") {
            _expression = CompositeExpression.Create(
               ExpressionModifier.Repeated
            )
         };

         action.Compile()(nestedRule);

         _expression.AddExpression(nestedRule._expression);

         return (ActionableRule) this;
      }

      // Repeats + Alternatives: ( A | B | C )+
      public IActionableRule RepeatOneOf(
         params Expression<Action<IRule>>[] actions
      ) {
         return Repeat(r => r.OneOf(actions));
      }

      public IActionableRule Say(string word) {
         _expression ??= CompositeExpression.Create(
            ExpressionModifier.Sequence
         );
         
         // FIXME: Needs an ID generator.
         _expression.AddExpression(Word.Create(0, word));

         return (ActionableRule)this;
      }

      public IActionableRule SayOneOf(params string[] words) {
         return SayOneOf(words as IEnumerable<string>);
      }

      public IActionableRule SayOneOf(IEnumerable<string> words) {
         var alternatives = CompositeExpression.Create(
            ExpressionModifier.Alternatives
         );

         // FIXME: Needs an ID generator.
         alternatives.AddExpressions(
            words.Select(word => Word.Create(0, word))
         );

         if (_expression == null) {
            _expression = alternatives;
         } else {
            _expression.AddExpression(alternatives);
         }

         return (ActionableRule)this;
      }

      public IActionableRule WithRule(string ruleName) {
         _expression ??= CompositeExpression.Create(
            ExpressionModifier.Sequence
         );

         // FIXME: Needs an ID generator.
         _expression.AddExpression(RuleName.Create(0, ruleName));

         return (ActionableRule)this;
      }

   }
}
