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
using NLog;
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;
using Renfrew.NatSpeakInterop;

namespace Renfrew.Grammar {
   public abstract class Grammar : IGrammar, IDisposable {
      private static readonly Logger Logger =
         LogManager.GetCurrentClassLogger();

      private readonly IGrammarService _grammarService;

      private readonly Lookup<IRule> _allRules = new();
      private readonly Lookup<IRule> _exportedRules = new();
      private readonly Lookup<IRule> _importedRules = new();
      private readonly Lookup<IRule> _activeRules = new();

      private readonly Lookup<Word> _allWords = new();

      private readonly RuleFactory _ruleFactory;
      private readonly IIdGenerator _idGenerator;


      protected Grammar(IGrammarService grammarService, INatSpeak natSpeak) :
         this(
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
            _activeRules.Add(_exportedRules.Get(name));
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
               _allWords.Add(word);
            }
         }

         _allRules.Add(rule);
         _exportedRules.Add(rule);
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

         _allRules.Add(rule);
         _importedRules.Add(rule);
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

      public void InvokeRule(List<SpokenWord> spokenWords) {
         Logger.Debug("InvokeRule: {0}", string.Join(", ", spokenWords));

         if (!_activeRules.Any()) {
            throw new NoActiveRulesException();
         }

         var startRuleId = spokenWords.First().RuleId;
         var startRule = _allRules.Get(startRuleId);

         Debug.Assert(_activeRules.Contains(startRule));

         //ResolveSequence(startRule.Sequence, spokenWords, 0);
      }

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
