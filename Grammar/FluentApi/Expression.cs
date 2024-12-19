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

namespace Renfrew.Grammar.FluentApi {
   public interface IExpression {

   }

   internal class CompositeExpression : IExpression {
      /// <summary>
      /// Indicates the type of expression.
      /// </summary>
      private readonly ExpressionModifier _expressionModifier;

      /// <summary>
      /// <b><see cref="CompositeExpression"/></b>s/<b><see cref="Term"/></b>s
      /// contained within this expression.
      /// </summary>
      private readonly List<IExpression> _subExpressions = new();

      public IReadOnlyList<IExpression> SubExpressions
         => _subExpressions.AsReadOnly();

      private CompositeExpression(ExpressionModifier expressionModifier) {
         _expressionModifier = expressionModifier;
      }

      public static CompositeExpression Create(
         ExpressionModifier expressionModifier
      ) => new(
         expressionModifier
      );

      public void AddExpression(IExpression expression) {
         _subExpressions.Add(expression);
      }

      public void AddExpressions(IEnumerable<IExpression> expressions) {
         _subExpressions.AddRange(expressions);
      }
   }

   internal abstract class Term : IExpression {
      protected Term(int id, string value) {
         Id = id;
         Value = value;
      }

      public int Id { get; private set; }
      public string Value { get; private set; }
   }

   internal class Word : Term {
      private Word(int id, string value) : base(id, value) { }
      public static Word Create(int id, string value) => new(id, value);
   }

   internal class RuleName : Term {
      private RuleName(int id, string value) : base(id, value) { }
      public static RuleName Create(int id, string value) => new(id, value);
   }

   internal class ListName : Term {
      private ListName(int id, string value) : base(id, value) { }
      public static ListName Create(int id, string value) => new(id, value);
   }

}
