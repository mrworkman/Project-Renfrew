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
// along with this program.If not, see<http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Linq;

using Renfrew.NatSpeakInterop;
using Renfrew.Win32.Interop;

namespace Renfrew.Core.Grammars {
   using Grammar;

   [GrammarExport("Steve's Grammar", "Steve's own grammar.")]
   public class SteveGlobalsGrammar : Grammar {

      private IEnumerable<KeyChord> _currentCommand = new List<KeyChord>();

      private static readonly Dictionary<string, List<KeyChord>> _commands = new();

      private static readonly Dictionary<string, KeyPress> _numberKeys = new() {
         #region Number -> Key
         { "zero",  Key.D0 },
         { "one",   Key.D1 },
         { "two",   Key.D2 },
         { "three", Key.D3 },
         { "four",  Key.D4 },
         { "five",  Key.D5 },
         { "six",   Key.D6 },
         { "seven", Key.D7 },
         { "eight", Key.D8 },
         { "nine",  Key.D9 },
         #endregion
      };

      private static readonly Dictionary<string, KeyPress> _letterKeys = new () {
         #region Letter -> Key
         { "alpha",    Key.A },
         { "bravo",    Key.B },
         { "charlie",  Key.C },
         { "delta",    Key.D },
         { "echo",     Key.E },
         { "foxtrot",  Key.F },
         { "golf",     Key.G },
         { "hotel",    Key.H },
         { "india",    Key.I },
         { "juliet",   Key.J },
         { "kilo",     Key.K },
         { "lima",     Key.L },
         { "lincoln",  Key.L },
         { "mike",     Key.M },
         { "november", Key.N },
         { "oscar",    Key.O },
         { "papa",     Key.P },
         { "quebec",   Key.Q },
         { "romeo",    Key.R },
         { "sierra",   Key.S },
         { "tango",    Key.T },
         { "uniform",  Key.U },
         { "victor",   Key.V },
         { "whiskey",  Key.W },
         { "x ray",    Key.X },
         { "yankee",   Key.Y },
         { "zulu",     Key.Z },
         #endregion
      };

      private static readonly Dictionary<string, uint> _numbers = new() {
         #region Numbers
         { "zero",         0 },
         { "one",          1 },
         { "two",          2 },
         { "three",        3 },
         { "four",         4 },
         { "five",         5 },
         { "six",          6 },
         { "seven",        7 },
         { "eight",        8 },
         { "nine",         9 },
         { "ten",          10 },
         { "eleven",       11 },
         { "twelve",       12 },
         { "thirteen",     13 },
         { "fourteen",     14 },
         { "fifteen",      15 },
         { "sixteen",      16 },
         { "seventeen",    17 },
         { "eighteen",     18 },
         { "nineteen",     19 },
         { "twenty",       20 },
         { "twenty one",   21 },
         { "twenty two",   22 },
         { "twenty three", 23 },
         { "twenty four",  24 },
         { "twenty five",  25 },
         { "twenty six",   26 },
         { "twenty seven", 27 },
         { "twenty eight", 28 },
         { "twenty nine",  29 },
         { "thirty",       30 },
         #endregion
      };

      public SteveGlobalsGrammar(IGrammarService grammarService)
         : base(grammarService) {

         AddCommand("slap", Key.Return);
         AddCommand("slam", Key.Control, Key.Return);
         
         // Cursor movement commands.
         AddCommand("york",  Key.Home);
         AddCommand("pork",  Key.End);
         AddCommand("sky",   Key.Up);
         AddCommand("art",   Key.Down);
         AddCommand("leaf",  Key.Left);
         AddCommand("reef",  Key.Right);
         AddCommand("leap",  Key.Control, Key.Left);
         AddCommand("reap",  Key.Control, Key.Right);
         AddCommand("spike", Key.Prior); // PageUp
         AddCommand("punch", Key.Next);  // PageDown
         //AddCommand("jimmy", );

         // Copy paste, etc.
         AddCommand("copy",  Key.Control, Key.C);
         AddCommand("paste", Key.Control, Key.V);
         AddCommand("cut",   Key.Control, Key.X);

         AddCommand("undo", Key.Control, Key.Z);
         AddCommand("redo", Key.Control, Key.Y);

         // Text selection commands.
         AddCommand("stall",  Key.Control, Key.A);
         AddCommand("ski",    Key.Shift,   Key.Up);
         AddCommand("heart",  Key.Shift,   Key.Down);
         AddCommand("strike", Key.Shift,   Key.Prior); // PageUp
         AddCommand("munch",  Key.Shift,   Key.Next);  // PageDown
         AddCommand("leap",   Key.Control, Key.Left);
         AddCommand("reap",   Key.Control, Key.Right);
         AddCommand("sleep",  Key.Control, Key.Shift, Key.Left);
         AddCommand("creep",  Key.Control, Key.Shift, Key.Right);
         AddCommand("beef",   Key.Shift,   Key.Left);
         AddCommand("chief",  Key.Shift,   Key.Right);
         AddCommand("lome",   Key.Shift,   Key.Home);
         AddCommand("rend",   Key.Shift,   Key.End);
         AddCommand("strobe", Key.Control, Key.Home);
         AddCommand("probe",  Key.Control, Key.End);

         AddCommand("tab",       Key.Tab);
         AddCommand("untab",     Key.Shift, Key.Tab);
         AddCommand("space",     Key.Space);
         AddCommand("backspace", Key.Back);
         AddCommand("delete",    Key.Delete);

         // Specific typable characters.
         AddCommand("comma",        CharKey.KeyPress(','));
         AddCommand("dot",          CharKey.KeyPress('.'));
         AddCommand("slash",        CharKey.KeyPress('/'));
         AddCommand("backslash",    CharKey.KeyPress('\\'));
         AddCommand("semi",         CharKey.KeyPress(';'));
         AddCommand("colon",        CharKey.KeyPress(':'));
         AddCommand("at",           CharKey.KeyPress('@'));
         AddCommand("percent",      CharKey.KeyPress('%'));
         AddCommand("bang",         CharKey.KeyPress('!'));
         AddCommand("bar",          CharKey.KeyPress('|'));
         AddCommand("ampersand",    CharKey.KeyPress('&'));
         AddCommand("prequels",     CharKey.KeyPress('='));
         AddCommand("dollar",       CharKey.KeyPress('$'));
         AddCommand("less than",    CharKey.KeyPress('<'));
         AddCommand("greater than", CharKey.KeyPress('>'));
         AddCommand("question",     CharKey.KeyPress('?'));
         AddCommand("hyphen",       CharKey.KeyPress('-'));
         AddCommand("minus",        CharKey.KeyPress('-'));
         AddCommand("plus",         CharKey.KeyPress('+'));
         AddCommand("star",         CharKey.KeyPress('*'));
         AddCommand("caret",        CharKey.KeyPress('^'));
         AddCommand("underscore",   CharKey.KeyPress('_'));
         AddCommand("tilde",        CharKey.KeyPress('~'));
         AddCommand("pound",        CharKey.KeyPress('#'));

         // Key combinations.
         AddCommand(
            "point", 
            KeyChord.Keys(CharKey.KeyPress('-')),
            KeyChord.Keys(CharKey.KeyPress('>'))
         );
         AddCommand(
            "joint",
            KeyChord.Keys(CharKey.KeyPress('=')),
            KeyChord.Keys(CharKey.KeyPress('>'))
         );

         // Window control commands.
         AddCommand("conmen", Key.Shift, Key.F10);



         //AddCommand("");



      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddCommand(string commandName, params KeyPress[] chordKeys) {
         _commands[commandName] = new() {
            KeyChord.Keys(chordKeys)
         };
      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddCommand(string commandName, params KeyChord[] chords) {
         _commands[commandName] = chords.ToList();
      }

      public override void Initialize() {

         AddRule("globals", e =>
            e.Repeat(command => command
               .SayOneOf(_commands.Select(c => c.Key))
                  .Do(words => SetCurrentCommand(_commands[words.First()]))
               .OptionallyOneOf(times => times
                  .SayOneOf(_numbers.Select(n => n.Key))
               )
               .Do(words => ExecuteCommand(words.FirstOrDefault()))
            )
         );

         Load();

         ActivateDefaultRules();
      }

      void ActivateDefaultRules() {
         ActivateRule("globals");
      }

      void ReactivateDefaultRules() {
         ReactivateRule("globals");
      }

      private void ExecuteCommand(string word) {
         uint times = word != null? _numbers[word] : 1;

         for (int i = 0; i < times; i++) {
            Keyboard.PlayKeys(_currentCommand);
         }
      }

      private void SetCurrentCommand(IEnumerable<KeyChord> chords) {
         _currentCommand = chords;
      }

      public override void Dispose() {
         throw new NotImplementedException();
      }
   }

}
