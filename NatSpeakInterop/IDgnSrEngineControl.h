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

#define DGNMIC_DISABLED	0
#define DGNMIC_OFF 		1
#define DGNMIC_ON		   2
#define DGNMIC_SLEEPING	3
#define DGNMIC_PAUSE	   4
#define DGNMIC_RESUME	5

#define IDgnSrEngineControlGUID "dd109000-6205-11cf-ae61-0000e8a28647"

namespace Renfrew::NatSpeakInterop::Dragon::ComInterfaces {

   [ComImport, Guid(IDgnSrEngineControlGUID)]
   [InterfaceType(ComInterfaceType::InterfaceIsIUnknown)]
   public interface class
      DECLSPEC_UUID(IDgnSrEngineControlGUID) IDgnSrEngineControl {

      void GetVersion(WORD*, WORD*, WORD*);
      void GetMicState(WORD*);
      void SetMicState(WORD, BOOL);
      void SaveSpeaker(BOOL);
      void GetChangedInfo(BOOL*, DWORD*);
      void Resume(QWORD);
      void RecognitionMimic(DWORD, SDATA, DWORD);
      void Preinitialize();
      void SpeakerRename(const WCHAR*, const WCHAR*);
   };
}
