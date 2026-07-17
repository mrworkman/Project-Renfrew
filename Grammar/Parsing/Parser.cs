// Project Renfrew
// Copyright(C) 2026 Stephen Workman (workman.stephen@gmail.com)
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
using Renfrew.Grammar.Collections;
using Renfrew.Grammar.Exceptions;
using Renfrew.Grammar.FluentApi.ExpressionParts.SequenceMembers;
using Renfrew.Grammar.FluentApi.Interfaces;

namespace Renfrew.Grammar.Parsing {
    /// <summary>
    ///    Parses a phrase (the words recognized in a single speech event)
    ///    against the rules of a <see cref="Grammar" />.
    /// </summary>
    /// <remarks>
    ///    The parser is a backtracking, recursive-descent matcher written in
    ///    continuation-passing style: each construct is handed a
    ///    <see cref="Continuation" /> representing "the rest of the parse" and
    ///    tries its options <em>together with</em> that remainder, restoring
    ///    the phrase position whenever a branch fails. This lets variable-
    ///    length constructs (optionals, repeats, alternatives) give back words
    ///    across a sub-sequence boundary until the surrounding rule can
    ///    complete.
    ///    <br />
    ///    <para>
    ///        Backtracking is driven off <see cref="Checkpoint" />s (the
    ///        walker's absolute position plus the length of the action trace),
    ///        never a running match count, so a failed branch always leaves
    ///        both the phrase and the collected actions exactly where they
    ///        were.
    ///    </para>
    /// </remarks>
    internal class Parser {
        /// <summary>
        ///    "The rest of the parse." Invoked once a construct has consumed
        ///    its own words; returns whether the remainder matches from the
        ///    current phrase position.
        /// </summary>
        private delegate ParseResult Continuation();

        private readonly Grammar _grammar;
        private readonly ListWalker<SpokenWord> _phrase;

        // Actions encountered on the path currently being explored, each paired
        // with the span of phrase words scoped to it (the words matched in the
        // action's own sequence since the previous action there). Truncated on
        // backtracking, so once the trunk parse succeeds, it holds exactly the
        // matching path's actions in order.
        private readonly List<TracedAction> _trace = new();

        // (ruleId, phraseIndex) pairs for rules we are currently descending
        // into and have not yet consumed past. Re-entering one is
        // left-recursion.
        private readonly HashSet<(uint RuleId, int Index)> _activeDescents = new();

        private Parser(ListWalker<SpokenWord> phrase, Grammar grammar) {
            _phrase = phrase;
            _grammar = grammar;
        }

        /// <summary>
        ///    Parses the given phrase against the grammar. The rule to start
        ///    from is taken from the first spoken word's rule id; the whole
        ///    phrase must be consumed for the parse to succeed. On success the
        ///    result carries the actions to invoke, each with the words matched
        ///    in its own sequence since the previous action there.
        /// </summary>
        public static ParseResult Parse(
           Grammar grammar,
           ListWalker<SpokenWord> phrase
        ) {
            if (grammar == null) {
                throw new ArgumentNullException(nameof(grammar));
            }

            if (phrase == null) {
                throw new ArgumentNullException(nameof(phrase));
            }

            if (phrase.Count == 0) {
                return ParseResult.Failed();
            }

            var startRule = grammar.GetRule(phrase.Current.RuleId);

            if (startRule == null) {
                return ParseResult.Failed();
            }

            var parser = new Parser(phrase, grammar);
            var result =
               parser.MatchRule(startRule, parser.IsPhraseFullyConsumed);

            return result is ParseResult.Success
               ? ParseResult.Succeeded(parser.BuildMatchedActions())
               : result;
        }

        private ParseResult MatchRule(IRule rule, Continuation continuation) {
            var entryIndex = _phrase.CurrentIndex;
            var descent = (rule.Id, entryIndex);

            // Re-entering the same rule at the same position means the grammar
            // is (mutually) left-recursive: it would descend forever without
            // consuming a word. The entry is released the moment the rule's own
            // members are consumed (below), so legitimate sibling references,
            // repeats, and right-recursion — which advance the phrase — do not
            // trip this.
            if (!_activeDescents.Add(descent)) {
                throw new LeftRecursiveRuleException(rule.Id);
            }

            // A rule opens a fresh action scope starting at the word it is about
            // to consume: actions among its members are scoped relative to
            // entryIndex, independent of the enclosing rule's scope.
            return MatchSequence(
               rule.Sequence.Members,
               0,
               rule.Id,
               entryIndex,
               null,
               () => {
                   _activeDescents.Remove(descent);
                   return continuation();
               }
            );
        }

        /// <summary>
        ///    Matches <paramref name="members" /> from
        ///    <paramref name="index" /> onward, then hands off to
        ///    <paramref name="continuation" />.
        /// </summary>
        /// <param name="ruleId">
        ///    Id of the (named) rule that owns these members. Inline constructs
        ///    keep the same id; only a <see cref="RuleName" /> reference starts
        ///    a new one. It is checked against each spoken word so words are
        ///    attributed to the correct rule.
        /// </param>
        /// <param name="scopeStart">
        ///    Phrase index at which this sequence began matching. An action here
        ///    that has no earlier sibling action is scoped from this point, so a
        ///    nested construct's actions never bleed into an enclosing
        ///    sequence's scope.
        /// </param>
        /// <param name="lastActionIndex">
        ///    Phrase index of the previous action encountered in <em>this</em>
        ///    sequence, or <c>null</c> if none. The next action is scoped from
        ///    here, so sibling actions partition the words between them.
        /// </param>
        private ParseResult MatchSequence(
           IReadOnlyList<ISequenceMember> members,
           int index,
           uint ruleId,
           int scopeStart,
           int? lastActionIndex,
           Continuation continuation
        ) {
            if (index >= members.Count) {
                return continuation();
            }

            // The continuation for whatever follows this member in the sequence.
            // Non-action members leave the sibling-action cursor untouched.
            Continuation next =
               () => MatchSequence(
                  members, index + 1, ruleId, scopeStart, lastActionIndex,
                  continuation
               );

            switch (members[index]) {
                case Word word:
                    return MatchWord(word, ruleId, next);
                case RuleName ruleName:
                    return MatchRuleName(ruleName, next);
                case Alternatives alternatives:
                    return MatchAlternatives(alternatives, ruleId, next);
                case Optional optional:
                    return MatchOptional(optional, ruleId, next);
                case Repeated repeated:
                    return MatchRepeated(repeated, ruleId, next);
                case GrammarAction action:
                    // Zero-width: record it scoped to the words consumed in this
                    // sequence since the previous action (or the sequence's
                    // start), then carry on with the sibling cursor advanced to
                    // here. Reverted on backtrack via the enclosing construct's
                    // checkpoint.
                    _trace.Add(new TracedAction(
                       action, lastActionIndex ?? scopeStart, _phrase.CurrentIndex
                    ));
                    return MatchSequence(
                       members,
                       index + 1,
                       ruleId,
                       scopeStart,
                       _phrase.CurrentIndex,
                       continuation
                    );
                default:
                    throw new UnrecognizedMemberType(members[index].GetType());
            }
        }

        private ParseResult MatchWord(
           Word word,
           uint ruleId,
           Continuation continuation
        ) {
            if (_phrase.IsAtEnd) {
                return ParseResult.Failed();
            }

            var spoken = _phrase.Current;

            if (word.Id != spoken.WordId
                || word.String != spoken.Word
                || spoken.RuleId != ruleId) {
                return ParseResult.Failed();
            }

            return Advance(continuation);
        }

        private ParseResult MatchRuleName(
           RuleName ruleName,
           Continuation continuation
        ) {
            var rule = _grammar.GetRule(ruleName.Id);

            if (rule == null) {
                return ParseResult.Failed();
            }

            // Descend into the referenced rule; its words carry the rule id,
            // and once it is consumed, the parent's continuation takes over.
            return MatchRule(rule, continuation);
        }

        private ParseResult MatchAlternatives(
           Alternatives alternatives,
           uint ruleId,
           Continuation continuation
        ) {
            var checkpoint = Save();

            foreach (var sequence in alternatives.Sequences) {
                // Each branch is its own action scope, opening where the
                // alternatives began.
                var result = MatchSequence(
                   sequence.Members,
                   0,
                   ruleId,
                   checkpoint.PhraseIndex,
                   null,
                   continuation
                );

                if (result is ParseResult.Success) {
                    return result;
                }

                Restore(checkpoint);
            }

            return ParseResult.Failed();
        }

        private ParseResult MatchOptional(
           Optional optional,
           uint ruleId,
           Continuation continuation
        ) {
            var checkpoint = Save();

            // Prefer taking the optional section (its own action scope)...
            var withSection = MatchSequence(
               optional.Sequence.Members,
               0,
               ruleId,
               checkpoint.PhraseIndex,
               null,
               continuation
            );

            if (withSection is ParseResult.Success) {
                return withSection;
            }

            // ...otherwise skip it entirely and let the remainder try to match.
            Restore(checkpoint);
            return continuation();
        }

        private ParseResult MatchRepeated(
           Repeated repeated,
           uint ruleId,
           Continuation continuation
        ) {
            // One-or-more: one mandatory pass, then zero-or-more. Each pass is
            // its own action scope, opening at the current phrase position.
            return MatchSequence(
               repeated.Sequence.Members,
               0,
               ruleId,
               _phrase.CurrentIndex,
               null,
               () => MatchRepetitions(repeated, ruleId, continuation)
            );
        }

        /// <summary>
        ///    Zero-or-more of the repeated section (the tail of a one-or-more).
        ///    Greedy: consumes another pass when it can, but gives passes back
        ///    when doing so lets the remainder complete.
        /// </summary>
        private ParseResult MatchRepetitions(
           Repeated repeated,
           uint ruleId,
           Continuation continuation
        ) {
            var checkpoint = Save();

            var withAnother = MatchSequence(
               repeated.Sequence.Members,
               0,
               ruleId,
               checkpoint.PhraseIndex,
               null,
               () => _phrase.CurrentIndex == checkpoint.PhraseIndex
                  // A pass that consumes nothing would loop forever; stop here.
                  ? continuation()
                  : MatchRepetitions(repeated, ruleId, continuation)
            );

            if (withAnother is ParseResult.Success) {
                return withAnother;
            }

            Restore(checkpoint);
            return continuation();
        }

        /// <summary>
        ///    Consumes the current word and runs
        ///    <paramref name="continuation" />, rewinding if it fails so a
        ///    sibling branch can try the same word.
        /// </summary>
        private ParseResult Advance(Continuation continuation) {
            var checkpoint = Save();

            _phrase.MoveForward();

            var result = continuation();

            if (result is ParseResult.Failure) {
                Restore(checkpoint);
            }

            return result;
        }

        private ParseResult IsPhraseFullyConsumed() {
            // The trunk parse only succeeds when every spoken word was matched.
            return _phrase.IsAtEnd
               ? ParseResult.Succeeded()
               : ParseResult.Failed();
        }

        private IReadOnlyList<MatchedAction> BuildMatchedActions() {
            var actions = new List<MatchedAction>(_trace.Count);

            foreach (var traced in _trace) {
                var words = new List<string>();

                for (var i = traced.StartIndex; i < traced.EndIndex; i++) {
                    words.Add(_phrase[i].Word);
                }

                actions.Add(new MatchedAction(traced.Action, words));
            }

            return actions;
        }

        private Checkpoint Save() {
            return new Checkpoint(_phrase.CurrentIndex, _trace.Count);
        }

        private void Restore(Checkpoint checkpoint) {
            _phrase.MoveTo(checkpoint.PhraseIndex);

            if (_trace.Count > checkpoint.TraceLength) {
                _trace.RemoveRange(
                   checkpoint.TraceLength,
                   _trace.Count - checkpoint.TraceLength
                );
            }
        }

        private readonly struct Checkpoint {
            public Checkpoint(int phraseIndex, int traceLength) {
                PhraseIndex = phraseIndex;
                TraceLength = traceLength;
            }

            public int PhraseIndex { get; }
            public int TraceLength { get; }
        }

        /// <summary>
        ///    An action encountered while parsing, together with the half-open
        ///    span of phrase words scoped to it: everything matched in the
        ///    action's own sequence since the previous sibling action (or the
        ///    sequence's start) up to the action's position.
        /// </summary>
        private readonly struct TracedAction {
            public TracedAction(GrammarAction action, int startIndex, int endIndex) {
                Action = action;
                StartIndex = startIndex;
                EndIndex = endIndex;
            }

            public GrammarAction Action { get; }
            public int StartIndex { get; }
            public int EndIndex { get; }
        }
    }
}
