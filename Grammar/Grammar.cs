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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar {
   public abstract class Grammar : IGrammar, IDisposable {
      private readonly IGrammarService _grammarService;

      private readonly Dictionary<string, IRule> _allRules =
         new(StringComparer.CurrentCultureIgnoreCase);

      private readonly Dictionary<string, IRule> _exportedRules =
         new(StringComparer.CurrentCultureIgnoreCase);

      private readonly Dictionary<string, IRule> _importedRules =
         new(StringComparer.CurrentCultureIgnoreCase);

      private readonly Dictionary<string, Word> _allWords = new();

      private readonly RuleFactory _ruleFactory;
      private readonly IIdGenerator _idGenerator;

      private readonly Dictionary<string, IRule> _activeRules = new();

      protected Grammar(IGrammarService grammarService, INatSpeak natSpeak)
         : this(
            new RuleFactory(),
            new IdGenerator(),
            grammarService,
            natSpeak
         ) { }

      protected Grammar(
         RuleFactory ruleFactory,
         IIdGenerator idGenerator,
         IGrammarService grammarService,
         INatSpeak natSpeak
      ) {
         Debug.Assert(ruleFactory != null);
         Debug.Assert(grammarService != null);
         Debug.Assert(natSpeak != null);

         _ruleFactory = ruleFactory;
         _idGenerator = idGenerator;

         _grammarService = grammarService;
         NatSpeak = natSpeak;
      }

      protected INatSpeak NatSpeak { get; }

      internal IReadOnlyList<IRule> AllRules => _allRules.Values.ToList();

      internal IReadOnlyList<IRule> ExportedRules =>
         _exportedRules.Values.ToList();

      internal IReadOnlyList<IRule> ImportedRules =>
         _importedRules.Values.ToList();

      internal IReadOnlyList<Word> Words => _allWords.Values.ToList();

      public IReadOnlyList<string> WordList => _allWords.Keys
         .OrderBy(word => word.ToLowerInvariant())
         .ToList();

      public void ActivateRule(string name) {
         _grammarService.ActivateRule(this, IntPtr.Zero, name);

         if (!_activeRules.ContainsKey(name)) {
            _activeRules.Add(name, _exportedRules[name]);
         }
      }

      public void AddRule(string name, IRule rule) {
         if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(
               "Value cannot be null or whitespace.",
               nameof(name)
            );
         }

         if (rule == null) {
            throw new ArgumentNullException(nameof(rule));
         }

         EnforceRuleNaming(name);

         if (_allRules.ContainsKey(name)) {
            throw new ArgumentException(
               $"Grammar already contains a rule called '{name}'.",
               nameof(name)
            );
         }

         foreach (var word in rule.Words) {
            if (!_allWords.ContainsKey(word.String)) {
               _allWords.Add(word.String, word);
            }
         }

         _allRules.Add(rule.String, rule);
         _exportedRules.Add(rule.String, rule);
      }

      public void AddRule(string name, Func<IRule, IRule> ruleFunc) {
         AddRule(
            name,
            rule: ruleFunc?.Invoke(_ruleFactory.Create(name, _idGenerator))
         );
      }

      public void DeactivateRule(string name) {
         _grammarService.DeactivateRule(this, name);

         if (_activeRules.ContainsKey(name)) {
            _activeRules.Remove(name);
         }
      }

      public abstract void Dispose();

      private void EnforceRuleNaming(string ruleName) {
         var validChars = @"[a-zA-Z0-9_]";

         if (!Regex.IsMatch(ruleName, $@"^{validChars}+$")) {
            throw new ArgumentOutOfRangeException(
               nameof(ruleName),
               $@"Rule name '{ruleName}' contains invalid character(s): '{
                  Regex.Replace(ruleName, validChars, string.Empty)
               }'"
            );
         }
      }

      public void ImportRule(string name) {
         if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(
               "Value cannot be null or whitespace.",
               nameof(name)
            );
         }

         var rule = _ruleFactory.Create(name, _idGenerator);

         EnforceRuleNaming(name);

         if (_allRules.ContainsKey(name)) {
            throw new ArgumentException(
               $"Grammar already contains a rule called '{name}'.",
               nameof(name)
            );
         }

         _allRules.Add(rule.String, rule);
         _importedRules.Add(rule.String, rule);
      }

      public abstract void Initialize();

      protected void Load() {
         _grammarService.LoadGrammar(this);
      }

      protected void MakeGrammarExclusive() {
         _grammarService.SetExclusiveGrammar(this, true);
      }

      protected void MakeGrammarNotExclusive() {
         _grammarService.SetExclusiveGrammar(this, false);
      }

      protected void RemoveRule(string name) {
         if (string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(
               "Value cannot be null or whitespace.",
               nameof(name)
            );
         }

         _exportedRules.Remove(name);
         _importedRules.Remove(name);
         _allRules.Remove(name);
      }

      // TODO: Refactor this to use the rule number returned from NatSpeak.
      public void InvokeRule(IEnumerable<string> spokenWords) {
         //if (spokenWords == null) {
         //   throw new ArgumentNullException(nameof(spokenWords));
         //}

         //// Make sure there is at least one rule activated.
         //if (!_activeRules.Any()) {
         //   throw new NoActiveRulesException();
         //}

         //var result = false;

         //// Iterate over each rule in the grammar, trying to invoke each one.
         //// The one that "works" will be assumed to be the correct rule.
         //foreach (var rule in _activeRules.Values.OrderBy(e => e)) {

         //   var spokenWordsStack = new Stack<string>(spokenWords.Reverse());
         //   var callbacks = new List<KeyValuePair<IGrammarAction, IEnumerable<string>>>();

         //   // Check the word sequence to see if it's a match.
         //   result = ProcessSpokenWords(
         //      rule.Elements,
         //      spokenWordsStack,
         //      callbacks
         //   );

         //   // Make sure there are no words left in the stack
         //   if (spokenWordsStack.Any()) {
         //      Debug.WriteLine(
         //         $"There are extra words in the callback: {string.Join(", ", spokenWords)}"
         //      );

         //      // The result of ProcessSpokenWords above could be "true", but
         //      // that doesn't mean that this rule is the correct one. If there
         //      // are spoken words that aren't accounted for, then it's the wrong
         //      // rule.
         //      result = false;
         //   }

         //   // Invoke callback(s)
         //   if (!result) {
         //      continue;
         //   }

         //   foreach (var callback in callbacks) {
         //      callback.Key.InvokeAction(callback.Value);
         //   }

         //   break;
         //}

         //// Did the spoken words match the rule's structure?
         //if (!result) {
         //   throw new InvalidSequenceInCallbackException();
         //}
         //throw new NotImplementedException();
      }

      //private bool ProcessSpokenWords(
      //   IElementContainer elementContainer, Stack<string> spokenWordsStack,
      //   List<KeyValuePair<IGrammarAction, IEnumerable<string>>> callbacks,
      //   List<string> aw = null
      //) {

      //   if (callbacks == null) {
      //      throw new ArgumentNullException(nameof(callbacks));
      //   }

      //   var actionWords = new List<string>();

      //   foreach (var element in elementContainer.Elements) {

      //      if (element is IWordElement wordElement) {
      //         var spokenWord = spokenWordsStack.FirstOrDefault();

      //         // If the words don't match, then this sub-rule doesn't match.
      //         if (spokenWord == null || 
      //            !string.Equals(
      //               spokenWord, wordElement.ToString(),
      //               StringComparison.CurrentCultureIgnoreCase
      //            )
      //         ) {
      //            return false;
      //         }

      //         // Add word to callback stack
      //         spokenWordsStack.Pop();
      //         actionWords.Add(spokenWord);

      //         continue;
      //      }

      //      // This rule refers to another rule, so we need to
      //      // look it up and traverse it as well...
      //      if (element is IRuleElement ruleElement) {
      //         var nestedRule = _allRules[ruleElement.ToString()].Subject;

      //         var nestedResult = ProcessSpokenWords(
      //            nestedRule.Elements,
      //            spokenWordsStack,
      //            callbacks,
      //            actionWords
      //         );

      //         if (!nestedResult) {
      //            return false;
      //         }

      //         continue;
      //      }

      //      // Check if we need to descend into a sub-rule (Optional, Repeats, Alternatives...)
      //      if (element is IElementContainer subRule) {
      //         var subRuleResult = false;

      //         if (subRule is IOptionals) {
      //            ProcessSpokenWords(subRule, spokenWordsStack, callbacks, actionWords);
      //            subRuleResult = true;
      //         } else if (subRule is IAlternatives rule) {
      //            var alternatives = rule?.Elements;

      //            foreach (var alternative in alternatives) {

      //               // Encapsulate in a sequence
      //               var sequence = new Sequence();
      //               sequence.AddElement(alternative);

      //               subRuleResult = ProcessSpokenWords(
      //                  sequence,
      //                  spokenWordsStack,
      //                  callbacks,
      //                  actionWords
      //               );

      //               if (subRuleResult) {
      //                  break;
      //               }
      //            }

      //         } else if (subRule is IRepeats repeats) {
      //            while (ProcessSpokenWords(repeats, spokenWordsStack, callbacks, actionWords)) {
      //               subRuleResult = true;
      //            }
      //         } else { // Must be an ISequence
      //            subRuleResult = ProcessSpokenWords(subRule, spokenWordsStack, callbacks, actionWords);
      //         }

      //         if (!subRuleResult) {
      //            return false;
      //         }

      //         continue;
      //      }

      //      if (element is IGrammarAction action) {
      //         callbacks.Add(
      //            new KeyValuePair<IGrammarAction, IEnumerable<string>>(
      //               action, actionWords
      //            )
      //         );

      //         actionWords = new List<string>();
      //      }
      //   }

      //   aw?.AddRange(actionWords);

      //   // If we get here, the rule matches the spoken words
      //   return true;
      //}

      /// <summary>
      /// Due to a problem with Dragon 15, rules we want to remain active
      /// need to be explicitly re-activated when another is de-activated.
      /// </summary>
      /// <param name="name">The name of the rule</param>
      public void ReactivateRule(string name) {
         DeactivateRule(name);
         ActivateRule(name);
      }
   }
}
