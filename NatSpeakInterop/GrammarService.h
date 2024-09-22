// Project Renfrew
// Copyright(C) 2017  Stephen Workman (workman.stephen@gmail.com)
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

#pragma once

#include "IGrammar.h"
#include "IGrammarSerializer.h"
#include "IGrammarService.h"
#include "GrammarExecutive.h"

namespace Renfrew::NatSpeakInterop {
   private ref class GrammarService :
      public IGrammarService {

      ISrCentral ^_isrCentral;
      IDgnSrEngineControl ^_idgnSrEngineControl;
      IGrammarSerializer ^_grammarSerializer;

      Dictionary<IGrammar^, GrammarExecutive^> ^_grammars;
      HashSet<String^> ^_activeRules;

      GrammarExecutive^ AddGrammarToList(IGrammar^ grammar);
      GrammarExecutive^ RemoveGrammarFromList(IGrammar^ grammar);
      GrammarExecutive^ GetGrammarExecutive(IGrammar^ grammar);

   public:
      GrammarService(
         ISrCentral ^isrCentral, 
         IDgnSrEngineControl ^idgnSrEngineControl
      );
      ~GrammarService();

      virtual void ActivateRule(IGrammar ^grammar, HWND hWnd, String ^ruleName);
      virtual void ActivateRule(IGrammar ^grammar, IntPtr hWnd, String ^ruleName);
      virtual void ActivateRules(IGrammar ^grammar);
      virtual void DeactivateRule(IGrammar ^grammar, String ^ruleName);

      virtual void SetExclusiveGrammar(IGrammar ^grammar, bool exclusive);

      virtual property IGrammarSerializer ^GrammarSerializer {
         void set(IGrammarSerializer ^grammarSerializer);
      }

      virtual void LoadGrammar(IGrammar ^grammar);
      virtual void UnloadGrammar(IGrammar ^grammar);

      void PausedProcessor(UInt64 cookie);
      void PhraseFinishedCallback(UInt32 flags, Object ^grammarObj, ISrResBasic^ isrResBasic);
   };
}