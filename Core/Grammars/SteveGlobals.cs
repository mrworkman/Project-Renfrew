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
using System.Linq;

using Renfrew.NatSpeakInterop;
using Renfrew.Win32.Interop;

namespace Renfrew.Core.Grammars {
   using Grammar;

   [GrammarExport("Steve's Grammar", "Steve's own grammar.")]
   public class SteveGlobalsGrammar : Grammar {

      public SteveGlobalsGrammar(IGrammarService grammarService)
         : base(grammarService) {

      }

      public override void Initialize() {
         var ks = KeySeq.Keys(
            Key.Shift//, Key.A
         );

         AddRule("globals", e =>
            e.Repeat(command => command
               .SayOneOf("slack")
               .OptionallyOneOf(times => times
                  .SayOneOf("one", "two")
                  .Do(words =>
                     Keyboard.PlayKeys()
                  )
               )
            )
         );
         //Keyboard.PlayKeys(ks, ks);

         //KeySeq.Modifiers().Build();
         //Keyboard.PlayKeys();


         // .PlayKeys(chord)     // E.g., CTRL+S
         // .PlayKeys(string)    // E.g., "some string"
         // .PlayKeys(sequence)  // E.g., (chord+ string+)+ ? CTRL+K,CTRL+C vs CTRL+(K,C)

         Load();

         ActivateDefaultRules();
      }

      void ActivateDefaultRules() {
         ActivateRule("globals");
      }

      void ReactivateDefaultRules() {
         ReactivateRule("globals");
      }

      public override void Dispose() {
         throw new NotImplementedException();
      }
   }

}
