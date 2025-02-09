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
   public interface IExpression { }

   internal class CompositeExpression :
      IExpression,
      IEquatable<CompositeExpression> {
      /// <summary>
      /// Indicates the type of expression.
      /// </summary>
      private readonly ExpressionModifier _expressionModifier;

      /// <summary>
      /// <b><see cref="IExpression"/></b>s contained within this expression.
      /// </summary>
      private readonly List<IExpression> _subExpressions = new();

      public ExpressionModifier Modifier => _expressionModifier;

      public IReadOnlyList<IExpression> SubExpressions =>
         _subExpressions.AsReadOnly();

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

            switch (left) {
               case IIdString term: {
                  if (!term.Equals(right as IIdString)) {
                     return false;
                  }

                  break;
               }
               case CompositeExpression exp: {
                  if (!exp.Equals(right as CompositeExpression)) {
                     return false;
                  }

                  break;
               }
               default: {
                  if (!left.Equals(right)) {
                     return false;
                  }

                  break;
               }
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

         var exprStrs = _subExpressions.Select(
            e =>
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

   public interface IIdString : IExpression, IEquatable<IIdString> {
      uint Id { get; }
      string String { get; }
   }

   public class Word : IIdString {
      public uint Id { get; }
      public string String { get; }

      private Word(uint id, string str) {
         Id = id;
         String = str;
      }

      internal static Word Create(uint id, string value) {
         return new Word(id, value);
      }

      public bool Equals(IIdString other) {
         return Id == other?.Id && String == other.String;
      }
   }

   public class ListName : IIdString {
      public uint Id { get; }
      public string String { get; }

      private ListName(uint id, string str) {
         Id = id;
         String = str;
      }

      internal static ListName Create(uint id, string value) {
         return new ListName(id, value);
      }

      public bool Equals(IIdString other) {
         return Id == other?.Id && String == other.String;
      }
   }

   public class RuleName : IIdString {
      public uint Id { get; }
      public string String { get; }

      private RuleName(uint id, string str) {
         Id = id;
         String = str;
      }

      internal static RuleName Create(uint id, string value) {
         return new RuleName(id, value);
      }

      public bool Equals(IIdString other) {
         return Id == other?.Id && String == other.String;
      }
   }
}
