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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;

namespace GrammarTests.Util {
   /// <summary>
   ///    Parses a Dragon/SRGS-style grammar rule string into the fluent
   ///    <see cref="IRule" /> structure consumed by the solver.
   /// </summary>
   /// <remarks>
   ///    Notation:
   ///    <list type="bullet">
   ///       <item><c>word</c> — a spoken word (a bare token).</item>
   ///       <item><c>a b c</c> — a sequence of terms.</item>
   ///       <item><c>( a | b )</c> — alternatives (one-of); also groups.</item>
   ///       <item><c>[ a ]</c> — an optional section.</item>
   ///       <item><c>x+</c> — repeat; binds to the preceding atom.</item>
   ///       <item><c>&lt;ruleName&gt;</c> — a reference to another rule.</item>
   ///    </list>
   ///    Rules are built through the public fluent API so that word/rule id
   ///    assignment and word deduplication match handwritten rules exactly.
   /// </remarks>
   public class RuleStringParser {
      private static readonly RuleFactory RuleFactory = new();

      public Expression<Action<IRule>> ParseExpression(
         string ruleString
      ) {
         if (ruleString == null) {
            throw new ArgumentNullException(nameof(ruleString));
         }

         var tokens = Tokenize(ruleString);
         var cursor = new TokenCursor(tokens);
         var root = ParseAlternation(cursor);

         cursor.Expect(TokenKind.End);

         return BuildExpression(root);
      }

      #region Tokenizer

      private enum TokenKind {
         Word,
         RuleRef,
         LParen,
         RParen,
         LBracket,
         RBracket,
         Pipe,
         Plus,
         End
      }

      private readonly struct Token {
         public Token(TokenKind kind, string text) {
            Kind = kind;
            Text = text;
         }

         public TokenKind Kind { get; }
         public string Text { get; }
      }

      private static List<Token> Tokenize(string input) {
         var tokens = new List<Token>();
         var i = 0;

         while (i < input.Length) {
            var c = input[i];

            if (char.IsWhiteSpace(c)) {
               i++;
               continue;
            }

            switch (c) {
               case '(':
                  tokens.Add(new Token(TokenKind.LParen, "("));
                  i++;
                  continue;
               case ')':
                  tokens.Add(new Token(TokenKind.RParen, ")"));
                  i++;
                  continue;
               case '[':
                  tokens.Add(new Token(TokenKind.LBracket, "["));
                  i++;
                  continue;
               case ']':
                  tokens.Add(new Token(TokenKind.RBracket, "]"));
                  i++;
                  continue;
               case '|':
                  tokens.Add(new Token(TokenKind.Pipe, "|"));
                  i++;
                  continue;
               case '+':
                  tokens.Add(new Token(TokenKind.Plus, "+"));
                  i++;
                  continue;
               case '<': {
                  var end = input.IndexOf('>', i + 1);

                  if (end < 0) {
                     throw new FormatException(
                        $"Unterminated rule reference at position {i}."
                     );
                  }

                  var name = input.Substring(i + 1, end - i - 1).Trim();

                  if (name.Length == 0) {
                     throw new FormatException(
                        $"Empty rule reference at position {i}."
                     );
                  }

                  tokens.Add(new Token(TokenKind.RuleRef, name));
                  i = end + 1;
                  continue;
               }
            }

            // A bare word: everything up to the next special char or space.
            var start = i;

            while (i < input.Length && !IsSpecial(input[i])) {
               i++;
            }

            tokens.Add(new Token(TokenKind.Word, input.Substring(start, i - start)));
         }

         tokens.Add(new Token(TokenKind.End, string.Empty));

         return tokens;
      }

      private static bool IsSpecial(char c) {
         return char.IsWhiteSpace(c)
                || c == '(' || c == ')'
                || c == '[' || c == ']'
                || c == '<' || c == '>'
                || c == '|' || c == '+';
      }

      private sealed class TokenCursor {
         private readonly IReadOnlyList<Token> _tokens;
         private int _pos;

         public TokenCursor(IReadOnlyList<Token> tokens) {
            _tokens = tokens;
         }

         public Token Peek => _tokens[_pos];

         public Token Next() {
            return _tokens[_pos++];
         }

         public void Expect(TokenKind kind) {
            if (Peek.Kind != kind) {
               throw new FormatException(
                  $"Expected {kind} but found '{Peek.Text}' ({Peek.Kind})."
               );
            }

            _pos++;
         }
      }

      #endregion

      #region Abstract Syntax Tree

      private abstract class Node { }

      private sealed class WordNode : Node {
         public WordNode(string word) => Word = word;
         public string Word { get; }
      }

      private sealed class RuleRefNode : Node {
         public RuleRefNode(string name) => Name = name;
         public string Name { get; }
      }

      private sealed class SequenceNode : Node {
         public SequenceNode(IReadOnlyList<Node> terms) => Terms = terms;
         public IReadOnlyList<Node> Terms { get; }
      }

      private sealed class AlternationNode : Node {
         public AlternationNode(IReadOnlyList<Node> branches) =>
            Branches = branches;

         public IReadOnlyList<Node> Branches { get; }
      }

      private sealed class OptionalNode : Node {
         public OptionalNode(Node child) => Child = child;
         public Node Child { get; }
      }

      private sealed class RepeatNode : Node {
         public RepeatNode(Node child) => Child = child;
         public Node Child { get; }
      }

      #endregion

      #region Recursive Descent Parser

      // alternation := sequence ( '|' sequence )*
      private static Node ParseAlternation(TokenCursor cursor) {
         var branches = new List<Node> { ParseSequence(cursor) };

         while (cursor.Peek.Kind == TokenKind.Pipe) {
            cursor.Next();
            branches.Add(ParseSequence(cursor));
         }

         return branches.Count == 1
            ? branches[0]
            : new AlternationNode(branches);
      }

      // sequence := term*
      private static Node ParseSequence(TokenCursor cursor) {
         var terms = new List<Node>();

         while (StartsTerm(cursor.Peek.Kind)) {
            terms.Add(ParseTerm(cursor));
         }

         return new SequenceNode(terms);
      }

      // term := atom '+'*
      private static Node ParseTerm(TokenCursor cursor) {
         var atom = ParseAtom(cursor);

         while (cursor.Peek.Kind == TokenKind.Plus) {
            cursor.Next();
            atom = new RepeatNode(atom);
         }

         return atom;
      }

      // atom := word | ruleRef | '(' alternation ')' | '[' alternation ']'
      private static Node ParseAtom(TokenCursor cursor) {
         var token = cursor.Peek;

         switch (token.Kind) {
            case TokenKind.Word:
               cursor.Next();
               return new WordNode(token.Text);
            case TokenKind.RuleRef:
               cursor.Next();
               return new RuleRefNode(token.Text);
            case TokenKind.LParen: {
               cursor.Next();
               var node = ParseAlternation(cursor);
               cursor.Expect(TokenKind.RParen);
               return node;
            }
            case TokenKind.LBracket: {
               cursor.Next();
               var node = ParseAlternation(cursor);
               cursor.Expect(TokenKind.RBracket);
               return new OptionalNode(node);
            }
            default:
               throw new FormatException(
                  $"Unexpected token '{token.Text}' ({token.Kind})."
               );
         }
      }

      private static bool StartsTerm(TokenKind kind) {
         return kind == TokenKind.Word
                || kind == TokenKind.RuleRef
                || kind == TokenKind.LParen
                || kind == TokenKind.LBracket;
      }

      #endregion

      #region Fluent API Builder

      private Expression<Action<IRule>> BuildExpression(Node node) {
         // Drive the fluent API imperatively. The nested-rule fluent methods
         // require an Expression, so wrap the (captured) build delegate in one
         // it can compile and invoke against the nested rule.
         Action<IRule> apply = rule => Apply(rule, node);
         return rule => apply(rule);
      }

      private void Apply(IRule rule, Node node) {
         switch (node) {
            case WordNode word:
               rule.Say(word.Word);
               break;
            case RuleRefNode ruleRef:
               rule.WithRule(ruleRef.Name);
               break;
            case SequenceNode sequence:
               foreach (var term in sequence.Terms) {
                  Apply(rule, term);
               }

               break;
            case AlternationNode alternation:
               rule.OneOf(
                  alternation.Branches.Select(BuildExpression).ToArray()
               );
               break;
            case OptionalNode optional:
               rule.Optionally(BuildExpression(optional.Child));
               break;
            case RepeatNode repeat:
               rule.Repeat(BuildExpression(repeat.Child));
               break;
            default:
               throw new InvalidOperationException(
                  $"Unhandled node type '{node.GetType().Name}'."
               );
         }
      }

      #endregion
   }
}
