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

      private static readonly Dictionary<string, List<KeyChord>> Commands = new();

      private static readonly Dictionary<string, KeyPress> NumberKeys = new() {
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

      private static readonly Dictionary<string, KeyPress> LetterKeys = new () {
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

      private static readonly Dictionary<string, uint> Numbers = new() {
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

      public SteveGlobalsGrammar(IGrammarService grammarService, INatSpeak natSpeak)
         : base(grammarService, natSpeak) {

         AddChordCommand("dismiss", Key.Escape);
         AddChordCommand("slap",    Key.Return);
         AddChordCommand("slam",    Key.Control, Key.Return);
         
         // Cursor movement commands.
         AddChordCommand("york",  Key.Home);
         AddChordCommand("pork",  Key.End);
         AddChordCommand("sky",   Key.Up);
         AddChordCommand("art",   Key.Down);
         AddChordCommand("leaf",  Key.Left);
         AddChordCommand("reef",  Key.Right);
         AddChordCommand("leap",  Key.Control, Key.Left);
         AddChordCommand("reap",  Key.Control, Key.Right);
         AddChordCommand("spike", Key.Prior); // PageUp
         AddChordCommand("punch", Key.Next);  // PageDown

         var down = Enumerable.Repeat(Key.Down, 15);
         var up = Enumerable.Repeat(Key.Up, 15);

         AddSequenceCommand("jimmy", down.Concat<KeyPress>(up));
         AddSequenceCommand("kimmy", up.Concat<KeyPress>(down));

         // Common Shortcuts.
         AddChordCommand("copy",  Key.Control, Key.C);
         AddChordCommand("copy that",  Key.Control, Key.C);
         AddChordCommand("paste", Key.Control, Key.V);
         AddChordCommand("paste it", Key.Control, Key.V);
         AddChordCommand("cut",   Key.Control, Key.X);
         AddChordCommand("undo",  Key.Control, Key.Z);
         AddChordCommand("redo",  Key.Control, Key.Y);
         AddChordCommand("save file",     Key.Control, Key.S);
         // Sometimes NatSpeak hears "save" as its "say" command, so
         // instead of sending CTRL+S it inserts "file" 🙄
         AddChordCommand("say file",      Key.Control, Key.S);
         AddChordCommand("find",          Key.Control, Key.F);
         AddChordCommand("find next",     Key.F3);
         AddChordCommand("find previous", Key.Shift,   Key.F3);

         // Text selection commands.
         AddChordCommand("stall",  Key.Control, Key.A);
         AddChordCommand("ski",    Key.Shift,   Key.Up);
         AddChordCommand("heart",  Key.Shift,   Key.Down);
         AddChordCommand("strike", Key.Shift,   Key.Prior); // PageUp
         AddChordCommand("munch",  Key.Shift,   Key.Next);  // PageDown
         AddChordCommand("leap",   Key.Control, Key.Left);
         AddChordCommand("reap",   Key.Control, Key.Right);
         AddChordCommand("sleep",  Key.Control, Key.Shift, Key.Left);
         AddChordCommand("creep",  Key.Control, Key.Shift, Key.Right);
         AddChordCommand("beef",   Key.Shift,   Key.Left);
         AddChordCommand("chief",  Key.Shift,   Key.Right);
         AddChordCommand("lome",   Key.Shift,   Key.Home);
         AddChordCommand("rend",   Key.Shift,   Key.End);
         AddChordCommand("strobe", Key.Control, Key.Home);
         AddChordCommand("probe",  Key.Control, Key.End);

         AddChordCommand("tab",       Key.Tab);
         AddChordCommand("untab",     Key.Shift, Key.Tab);
         AddChordCommand("space",     Key.Space);
         AddChordCommand("backspace", Key.Back);
         AddChordCommand("delete",    Key.Delete);

         // Specific typable characters.
         AddChordCommand("comma",        CharKey.KeyPress(','));
         AddChordCommand("dot",          CharKey.KeyPress('.'));
         AddChordCommand("slash",        CharKey.KeyPress('/'));
         AddChordCommand("backslash",    CharKey.KeyPress('\\'));
         AddChordCommand("semi",         CharKey.KeyPress(';'));
         AddChordCommand("colon",        CharKey.KeyPress(':'));
         AddChordCommand("at",           CharKey.KeyPress('@'));
         AddChordCommand("percent",      CharKey.KeyPress('%'));
         AddChordCommand("bang",         CharKey.KeyPress('!'));
         AddChordCommand("bar",          CharKey.KeyPress('|'));
         AddChordCommand("ampersand",    CharKey.KeyPress('&'));
         AddChordCommand("prequels",     CharKey.KeyPress('='));
         AddChordCommand("dollar",       CharKey.KeyPress('$'));
         AddChordCommand("less than",    CharKey.KeyPress('<'));
         AddChordCommand("greater than", CharKey.KeyPress('>'));
         AddChordCommand("question",     CharKey.KeyPress('?'));
         AddChordCommand("hyphen",       CharKey.KeyPress('-'));
         AddChordCommand("minus",        CharKey.KeyPress('-'));
         AddChordCommand("plus",         CharKey.KeyPress('+'));
         AddChordCommand("star",         CharKey.KeyPress('*'));
         AddChordCommand("caret",        CharKey.KeyPress('^'));
         AddChordCommand("underscore",   CharKey.KeyPress('_'));
         AddChordCommand("tilde",        CharKey.KeyPress('~'));
         AddChordCommand("pound",        CharKey.KeyPress('#'));

         // Multi-character commands.
         AddSequenceCommand(
            "braces",
            CharKey.KeyPress('{'),
            CharKey.KeyPress('}'),
            Key.Left
         );
         AddSequenceCommand(
            "quotes",
            CharKey.KeyPress('"'),
            CharKey.KeyPress('"'),
            Key.Left
         );
         AddSequenceCommand(
            "singles",
            CharKey.KeyPress('\''),
            CharKey.KeyPress('\''),
            Key.Left
         );
         AddSequenceCommand(
            "brackets",
            CharKey.KeyPress('['),
            CharKey.KeyPress(']'),
            Key.Left
         );
         AddSequenceCommand(
            "prawns",
            CharKey.KeyPress('('),
            CharKey.KeyPress(')'),
            Key.Left
         );
         AddSequenceCommand(
            "angles",
            CharKey.KeyPress('<'),
            CharKey.KeyPress('>'),
            Key.Left
         );
         AddSequenceCommand("nick", '\\', 'n');
         AddSequenceCommand("rick", '\\', 'r');
         AddSequenceCommand("point", '-', '>');
         AddSequenceCommand("joint", '=', '>');

         // Window control commands.
         AddChordCommand("conmen",       Key.Shift, Key.F10);
         AddChordCommand("mini",         Key.LWin, Key.Down);
         AddChordCommand("maxi",         Key.LWin, Key.Up);
         AddChordCommand("window left",  Key.LWin, Key.Shift, Key.Left);
         AddChordCommand("window right", Key.LWin, Key.Shift, Key.Right);
         AddChordCommand("switchy",      Key.Menu, Key.Tab);
         AddChordCommand("switchy boo",  Key.Menu, Key.Control, Key.Tab);

         // Add letters as commands.
         foreach (var entry in LetterKeys) {
            AddChordCommand(entry.Key, entry.Value);
         }
      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddChordCommand(string commandName, params KeyPress[] chordKeys) {
         AddChordCommand(commandName, KeyChord.Keys(chordKeys));
      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddChordCommand(string commandName, params KeyChord[] chords) {
         Commands[commandName] = chords.ToList();
      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddSequenceCommand(string commandName, params KeyPress[] sequenceOfKeyPresses) {
         AddSequenceCommand(commandName, sequenceOfKeyPresses.AsEnumerable());
      }

      private void AddSequenceCommand(string commandName, IEnumerable<KeyPress> sequenceOfKeyPresses) {
         Commands[commandName] = sequenceOfKeyPresses.Select(
            keyPress => KeyChord.Keys(keyPress)
         ).ToList();
      }

      /// <summary>
      ///  Only to be called from the constructor.
      /// </summary>
      private void AddSequenceCommand(string commandName, params char[] sequenceOfChars) {
         AddSequenceCommand(commandName, sequenceOfChars.Select(CharKey.KeyPress));
      }

      public override void Initialize() {
         AddRule("globals", e =>
            e.Repeat(command => command
               .SayOneOf(Commands.Select(c => c.Key))
                  .Do(words => SetCurrentCommand(Commands[words.First()]))
               .OptionallyOneOf(times => times
                  .SayOneOf(Numbers.Select(n => n.Key))
               )
               .Do(words => ExecuteCommand(words.FirstOrDefault()))
            )
         );

         AddRule("snore", e => e.Say("Snore").Do(NatSpeak.MicSleep));

         Load();

         ActivateDefaultRules();
      }

      void ActivateDefaultRules() {
         ActivateRule("globals");
         ActivateRule("snore");
      }

      void ReactivateDefaultRules() {
         ReactivateRule("globals");
         ReactivateRule("snore");
      }

      private void ExecuteCommand(string word) {
         uint times = word != null? Numbers[word] : 1;

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
