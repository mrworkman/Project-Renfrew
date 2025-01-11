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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Renfrew.Grammar.FluentApi {
   public interface IExpression {

   }

   internal class CompositeExpression : 
      IExpression, IEquatable<CompositeExpression> {

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

      internal static CompositeExpression Create(
         ExpressionModifier expressionModifier,
         params IExpression[] expressions
      ) {
         var compositeExpression = new CompositeExpression(expressionModifier);
         compositeExpression._subExpressions.AddRange(expressions);
         return compositeExpression;
      }

      public void AddExpression(IExpression expression) {
         _subExpressions.Add(expression);
      }

      public void AddExpressions(IEnumerable<IExpression> expressions) {
         _subExpressions.AddRange(expressions);
      }

      public bool Equals(CompositeExpression other) {
         if (other == null) {
            return false;
         }

         if (_subExpressions.Count != other._subExpressions.Count) {
            return false;
         }

         for (int i = 0; i < _subExpressions.Count; i++) {
            var left = _subExpressions[i];
            var right = other._subExpressions[i];

            if (left.GetType() != right.GetType()) {
               return false;
            }

            if (left is Term term) {
               if (!term.Equals(right as Term)) {
                  return false;
               }
            } else if (left is CompositeExpression exp) {
               if (!exp.Equals(right as CompositeExpression)) {
                  return false;
               }
            } else if (!left.Equals(right)) {
               return false;
            }
         }

         return _expressionModifier == other._expressionModifier;
      }

      public override string ToString() {
         var sb = new StringBuilder();

         sb.Append("{\r\n");
         sb.Append($@"  ""ExpressionModifier"": ""{_expressionModifier}"",");
         sb.Append("\r\n");
         sb.Append(@"  ""SubExpressions"": [");
         sb.Append("\r\n");

         var exprStrs = _subExpressions.Select(e => 
            Regex.Replace(e.ToString(), @"^", "    ", RegexOptions.Multiline)
         );

         //sb.Append("  ");
         sb.Append(string.Join(",\r\n", exprStrs));
         sb.Append("\r\n");

         sb.Append("  ]\r\n");
         sb.Append("}");

         return sb.ToString();
      }
   }

   internal abstract class Term : IExpression, IEquatable<Term> {
      protected Term(int id, string value) {
         Id = id;
         Value = value;
      }

      public int Id { get; private set; }
      public string Value { get; private set; }

      public bool Equals(Term other) {
         if (other == null) {
            return false;
         }

         return Id == other.Id && Value == other.Value;
      }

      public override string ToString() {
         var sb = new StringBuilder();

         sb.Append("{\r\n");
         sb.Append($@"  ""Id"": {Id},");
         sb.Append("\r\n");
         sb.Append($@"  ""Value"": ""{Value}"",");
         sb.Append("\r\n");
         sb.Append($@"  ""@Type"": ""{GetType()}"",");
         sb.Append("\r\n");
         sb.Append("}");

         return sb.ToString();
      }
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
