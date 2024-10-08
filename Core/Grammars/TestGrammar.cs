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
using System.Windows.Forms;

using Renfrew.NatSpeakInterop;

namespace Renfrew.Core.Grammars {
   using Grammar;

   [GrammarExport("Test Grammar", "This is a test grammar.")]
   public class TestGrammar : Grammar {

      public TestGrammar(IGrammarService grammarService, INatSpeak natSpeak)
         : base(grammarService, natSpeak) {

      }

      public override void Initialize() {
         AddRule("test_rule", e =>
            e.Say("hello").Say("jello").Do(() => MessageBox.Show("Hello!"))
         );

         Load();

         ActivateRule("test_rule");
      }

      public override void Dispose() {
         throw new NotImplementedException();
      }
   }

}
