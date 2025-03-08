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

#include "stdafx.h"

#include "SrGramNotifySink.h"

#include "GrammarAlreadyLoadedException.h"
#include "GrammarException.h"
#include "GrammarNotLoadedException.h"
#include "InvalidStateException.h"
#include "SpokenWord.h"
#include "SrErrorCodes.h"

using namespace Renfrew::NatSpeakInterop;
using namespace Renfrew::NatSpeakInterop::Dragon;
using namespace Renfrew::NatSpeakInterop::Dragon::ComInterfaces;
using namespace Renfrew::NatSpeakInterop::Exceptions;
using namespace Renfrew::NatSpeakInterop::Sinks;

#include "GrammarService.h"

GrammarService::GrammarService(
   ISrCentral^ isrCentral,
   IDgnSrEngineControl^ idgnSrEngineControl
) {
   if (isrCentral == nullptr) {
      throw gcnew ArgumentNullException("isrCentral");
   }
   if (idgnSrEngineControl == nullptr) {
      throw gcnew ArgumentNullException("iDgnSrEngineControl");
   }

   _isrCentral = isrCentral;
   _idgnSrEngineControl = idgnSrEngineControl;

   _grammars = gcnew Dictionary<IGrammar^, GrammarExecutive^>();
   _activeRules = gcnew HashSet<String^>();
}

GrammarService::~GrammarService() {
   auto grammars = gcnew List<IGrammar^>(_grammars->Keys);

   Debug::WriteLine("GrammarService: Releasing.");

   for each (auto g in grammars) {
      UnloadGrammar(g);
   }
}

void GrammarService::ActivateRule(
   IGrammar^ grammar,
   HWND hWnd,
   String^ ruleName
) {
   pin_ptr<const WCHAR> wstrRuleName = PtrToStringChars(ruleName);

   if (_grammars->ContainsKey(grammar) == false) {
      throw gcnew GrammarNotLoadedException("FILL ME IN");
   }

   // Check if the handle points to an exsiting window
   if (hWnd != nullptr && IsWindow(hWnd) == false) {
      return; // TODO: Throw exception?
   }

   auto ge = _grammars[grammar];

   // TODO: Check that the grammar actually has the matching rule name!

   try {
      if (_activeRules->Contains(ruleName) == false) {
         ge->GramCommonInterface->Activate(
            hWnd,
            // TODO: Set to hWnd (where applicable)
            false,
            wstrRuleName
         );
         _activeRules->Add(ruleName);
      }
   } catch (COMException^ e) {
      if (e->HResult == SrErrorCodes::SRERR_INVALIDRULE) {
         throw gcnew GrammarException(
            String::Format("Invalid Rule: {0}!", ruleName),
            e
         );
      }
      if (e->HResult == SrErrorCodes::SRERR_GRAMMARTOOCOMPLEX) {
         throw gcnew GrammarException("Grammar too complex!", e);
      }
      if (e->HResult == SrErrorCodes::SRERR_RULEALREADYACTIVE) {
         throw gcnew GrammarException(
            String::Format("Rule Already Active: {0}!", ruleName),
            e
         );
      }
      throw gcnew GrammarException("Unexpected Grammar Error!", e);
   }
}

void GrammarService::ActivateRule(
   IGrammar^ grammar,
   IntPtr hWnd,
   String^ ruleName
) {
   ActivateRule(grammar, static_cast<HWND>(hWnd.ToPointer()), ruleName);
}

void GrammarService::ActivateRules(IGrammar^ grammar) {
   throw gcnew NotImplementedException();
}

GrammarExecutive^ GrammarService::AddGrammarToList(IGrammar^ grammar) {
   if (_grammars->ContainsKey(grammar) == true) {
      throw gcnew GrammarAlreadyLoadedException("FILL ME IN");
   }

   auto grammarExecutive = gcnew GrammarExecutive(grammar);

   _grammars->Add(grammar, grammarExecutive);

   return grammarExecutive;
}

void GrammarService::DeactivateRule(IGrammar^ grammar, String^ ruleName) {
   pin_ptr<const WCHAR> wstrRuleName = PtrToStringChars(ruleName);

   auto ge = GetGrammarExecutive(grammar);

   // TODO: Check that the grammar actually has the matching rule name!

   try {
      if (_activeRules->Contains(ruleName) == true) {
         ge->GramCommonInterface->Deactivate(wstrRuleName);
         _activeRules->Remove(ruleName);
      }
   } catch (COMException^ e) {
      if (e->HResult == SrErrorCodes::SRERR_RULENOTACTIVE) {
         throw gcnew GrammarException(
            String::Format("Rule Is Not Active: {0}!", ruleName),
            e
         );
      }
      throw gcnew GrammarException("Unexpected Grammar Error!", e);
   }
}

GrammarExecutive^ GrammarService::GetGrammarExecutive(IGrammar^ grammar) {
   if (grammar == nullptr) {
      throw gcnew ArgumentNullException("grammar");
   }

   // Make sure the grammar's loaded
   if (_grammars->ContainsKey(grammar) == false) {
      throw gcnew GrammarNotLoadedException("FILL ME IN");
   }

   return _grammars[grammar];
}

void GrammarService::GrammarSerializer::set(
   IGrammarSerializer^ grammarSerializer
) {
   if (grammarSerializer == nullptr) {
      throw gcnew ArgumentNullException("grammarSerializer");
   }

   _grammarSerializer = grammarSerializer;
}

void GrammarService::LoadGrammar(IGrammar^ grammar) {
   LPUNKNOWN pUnknown;
   array<byte>^ grammarBytes;

   if (grammar == nullptr) {
      throw gcnew ArgumentNullException("grammar");
   }

   if (_grammarSerializer == nullptr) {
      throw gcnew InvalidStateException("GrammarSerializer hasn't been set!");
   }

   auto grammarExecutive = AddGrammarToList(grammar);

   grammarBytes = _grammarSerializer->Serialize(grammar);

   // Pinning any sub-element of a managed array pins the entire array
   pin_ptr<byte> bytes = &grammarBytes[0];

   SDATA data;
   data.dwSize = grammarBytes->Length;
   data.pData = bytes;

   ISrGramNotifySink^ isrGramNotifySink = gcnew SrGramNotifySink(
      gcnew Action<UInt32, Object^, ISrResBasic^>(
         this,
         &GrammarService::PhraseFinishedCallback
      ),
      grammarExecutive
   );

   IntPtr iSrGramNotifySinkPtr = Marshal::GetIUnknownForObject(
      isrGramNotifySink
   );

   try {
      _isrCentral->GrammarLoad(
         SRGRMFMT_CFG,
         data,
         iSrGramNotifySinkPtr,
         __uuidof(ISrGramNotifySink^),
         &pUnknown
      );
   } catch (COMException^ e) {
      if (e->HResult == SrErrorCodes::SRERR_INVALIDCHAR) {
         throw gcnew GrammarException("Invalid Word/Character in Grammar", e);
      }
      if (e->HResult == SrErrorCodes::SRERR_GRAMMARERROR) {
         throw gcnew GrammarException("Grammar Error", e);
      }
      throw gcnew GrammarException("Unexpected Grammar Error!", e);
   }

   auto isrGramCommon = static_cast<ISrGramCommon^>(
      Marshal::GetTypedObjectForIUnknown(
         IntPtr(pUnknown),
         ISrGramCommon::typeid
      ));

   pUnknown->Release();
   Marshal::Release(iSrGramNotifySinkPtr);

   // Store isrGramCommon with our grammar
   grammarExecutive->GramCommonInterface = isrGramCommon;
}

void GrammarService::PausedProcessor(UInt64 cookie) {
   Debug::WriteLine(__FUNCTION__ + "(cookie: " + cookie + ")");

   // TODO: Call grammar activation method(s)

   Debug::WriteLine(__FUNCTION__ + ", Resuming.");
   _idgnSrEngineControl->Resume(cookie);
}

void GrammarService::PhraseFinishedCallback(
   UInt32 flags,
   Object^ grammarObj,
   ISrResBasic^ isrResBasic
) {
   Debug::WriteLine(__FUNCTION__);

   Debug::Assert(grammarObj != nullptr);
   Debug::Assert(isrResBasic != nullptr);

   auto grammarExecutive = safe_cast<GrammarExecutive^>(grammarObj);

   if (grammarExecutive == nullptr) {
      throw gcnew InvalidStateException("grammarObj is unexpectedly NULL!");
   }

   if ((flags & ISRNOTEFIN_RECOGNIZED) == 0) {
      Debug::WriteLine("Phrase rejected.");
      return;
   }

   if ((flags & ISRNOTEFIN_THISGRAMMAR) == 0) {
      Debug::WriteLine("Phrase is not from this grammar.");
      return;
   }

   auto isrResGraph = safe_cast<ISrResGraph^>(isrResBasic);

   const auto path = new DWORD[MaxPathEntries];
   DWORD pathSize = sizeof(DWORD) * MaxPathEntries;

   isrResGraph->BestPathWord(0, path, pathSize, &pathSize);

   Debug::Assert(pathSize != 0);
   Debug::Assert(pathSize <= sizeof(DWORD) * MaxPathEntries);

   auto spokenWords = gcnew List<SpokenWord^>();

   const auto numWords = pathSize / sizeof(DWORD);

   for (DWORD i = 0; i < numWords; i++) {
      SRRESWORDNODE node;

      DWORD bufferSize = sizeof(SRWORDW) + MaxWordSize;
      const auto buffer = new BYTE[bufferSize];
      const auto pWord = reinterpret_cast<PSRWORDW>(buffer);

      isrResGraph->GetWordNode(path[i], &node, pWord, bufferSize, &bufferSize);

      auto word = gcnew String(pWord->szWord);
      auto wordNumber = pWord->dwWordNum;
      auto ruleNumber = node.dwCFGParse;

      spokenWords->Add(gcnew SpokenWord(word, wordNumber, ruleNumber));

      delete[] buffer;
   }

   delete[] path;

   grammarExecutive->Grammar->InvokeRule(spokenWords);
}

GrammarExecutive^ GrammarService::RemoveGrammarFromList(IGrammar^ grammar) {
   auto ge = GetGrammarExecutive(grammar);

   _grammars->Remove(grammar);

   return ge;
}

void GrammarService::SetExclusiveGrammar(IGrammar^ grammar, bool exclusive) {
   auto ge = GetGrammarExecutive(grammar);

   safe_cast<IDgnSrGramCommon^>(ge->GramCommonInterface)->SpecialGrammar(
      exclusive
   );
}

void GrammarService::UnloadGrammar(IGrammar^ grammar) {
   if (grammar == nullptr) {
      throw gcnew ArgumentNullException("grammar");
   }

   Debug::WriteLine("GrammarService: Unloading " + grammar + ".");

   auto ge = RemoveGrammarFromList(grammar);

   if (ge->GramCommonInterface == nullptr) {
      throw gcnew InvalidStateException("isrGramCommon interface is not set!");
   }

   Marshal::ReleaseComObject(ge->GramCommonInterface);
   ge->GramCommonInterface = nullptr;
}
