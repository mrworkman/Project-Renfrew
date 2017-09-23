﻿// Project Renfrew
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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using Renfrew.Grammar.Elements;
using Renfrew.Grammar.FluentApi;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar {

   public abstract class Grammar : IGrammar, IDisposable {
      
      private readonly Dictionary<String, IRule> _rules;
      private readonly Dictionary<UInt32, IRule> _rulesById;

      private UInt32 _wordCount = 1;
      private readonly Dictionary<String, UInt32> _wordIds;

      private UInt32 _ruleCount = 1;
      private readonly Dictionary<String, UInt32> _ruleIds;
      
      protected Grammar() 
         : this(new RuleFactory()) {

         // This is a list of the rules themselves (by name)
         _rules = new Dictionary<String, IRule>(StringComparer.CurrentCultureIgnoreCase);
         _rulesById = new Dictionary<UInt32, IRule>();

         // These are lookups to find the numeric ids for words/rule names
         _wordIds = new Dictionary<String, UInt32>(StringComparer.CurrentCultureIgnoreCase);
         _ruleIds = new Dictionary<String, UInt32>(StringComparer.CurrentCultureIgnoreCase);
      }

      protected Grammar(RuleFactory ruleFactory) {
         RuleFactory = ruleFactory;
      }

      protected void AddRule(String name, IRule rule) {
         if (String.IsNullOrWhiteSpace(name) == true)
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

         if (rule == null)
            throw new ArgumentNullException(nameof(rule));

         EnforceRuleNaming(name);
         
         if (_rules.ContainsKey(name) == true)
            throw new ArgumentException($"Grammar already contains a rule called '{name}'.", nameof(name));

         foreach (var word in GetWordsFromRule(rule)) {
            Debug.WriteLine($"WORD: {word}");

            if (_wordIds.ContainsKey(word) == false)
               _wordIds.Add(word, _wordCount++);

         }

         var ruleId = _ruleCount++;

         if (_ruleIds.ContainsKey(name) == false)
            _ruleIds.Add(name, ruleId);

         _rules.Add(name, rule);
         _rulesById.Add(ruleId, rule);
      }

      protected void AddRule(String name, Func<IRule, IRule> ruleFunc) =>
         AddRule(name, ruleFunc?.Invoke(RuleFactory.Create()));

      public abstract void Dispose();

      private void EnforceRuleNaming(String ruleName) {
         var validChars = @"[a-zA-Z0-9_]";

         if (Regex.IsMatch(ruleName, $@"^{validChars}+$") == false) {
            throw new ArgumentOutOfRangeException(nameof(ruleName), 
               $@"Rule name '{ruleName}' contains invalid character(s): '{
                  Regex.Replace(ruleName, validChars, String.Empty)
               }'"
            );
         }  
      }

      public abstract void Initialize();
      
      protected void RemoveRule(String name) {
         if (String.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

         _rules.Remove(name);

         _rulesById.Remove(_ruleIds[name]);
         _ruleIds.Remove(name);
         
      }

      private IEnumerable<String> GetWordsFromRule(IRule rule) {
         return GetWordsFromRuleElements(rule.Elements.Elements);
      }

      private IEnumerable<String> GetWordsFromRuleElements(IEnumerable<IElement> elements) {
         foreach (var element in elements) {

            // Ignore action elements
            if (element is IGrammarAction)
               continue;

            // Get the word from the element/sub-elements
            if (element is IElementContainer == false) {
               yield return element.ToString();
            } else {
               foreach (var word in GetWordsFromRuleElements((element as IElementContainer).Elements))
                  yield return word;
            }
         }
      }

      public void InvokeRule(UInt32 ruleNumber, IEnumerable<String> spokenWords) {

         if (spokenWords == null)
            throw new ArgumentNullException(nameof(spokenWords));

         if (_rulesById.ContainsKey(ruleNumber) == false)
            throw new ArgumentOutOfRangeException();

         var rule = _rulesById[ruleNumber];

         var spokenWordStack = new Stack<String>(spokenWords.Reverse());

         foo(rule.Elements, spokenWordStack);
         
         if (spokenWordStack.Any() == true)
            throw new Exception();

      }

      private void foo(IElementContainer elementContainer, Stack<String> spokenWordsStack, List<String> callbackWords = null) {

         callbackWords = callbackWords ?? new List<String>();

         foreach (var element in elementContainer.Elements) {
            
            if (element is IElementContainer) {
               var subGroup = element as IElementContainer;

               foo(subGroup, spokenWordsStack, callbackWords);

            } else if (element is IGrammarAction) {
               (element as GrammarAction).InvokeAction(callbackWords);
            } else if (element is IWordElement) {
               var comparer = StringComparison.CurrentCultureIgnoreCase;

               var ruleWord = (element as IWordElement).ToString();
               var firstStackWord = spokenWordsStack.FirstOrDefault();

               if (firstStackWord == null)
                  throw new Exception();

               if (String.Equals(ruleWord, firstStackWord, comparer) == true) {
                  callbackWords.Add(spokenWordsStack.Pop());
               } else {
                  if (elementContainer is IOptionals)
                     continue;
                  throw new Exception();
               }


            }
         }

      }

      protected RuleFactory RuleFactory { get; private set; }

      public IReadOnlyDictionary<String, UInt32> RuleIds => _ruleIds;
      
      // Expose internally for serialization
      internal IReadOnlyCollection<IRule> Rules =>
         _rules.OrderBy(e => e.Key).Select(e => e.Value).ToList();

      public IReadOnlyDictionary<String, UInt32> WordIds => _wordIds;

   }
   
}
