// Project Renfrew
// Copyright(C) 2016 Stephen Workman (workman.stephen@gmail.com)
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

#include "IDgnGetSinkFlags.h"
#include "IDgnSREngineNotifySink.h"
#include "ISRNotifySink.h"

namespace Renfrew::NatSpeakInterop::Sinks {
   public ref class SrNotifySink :
      public Dragon::ComInterfaces::IDgnGetSinkFlags,
      public Dragon::ComInterfaces::IDgnSrEngineNotifySink,
      public Dragon::ComInterfaces::ISrNotifySink {

      Action<UInt64> ^_pausedProcessingCallback;

   public:
      SrNotifySink(Action<UInt64> ^pausedProcessingCallback);
      void virtual SinkFlagsGet(DWORD *pdwFlags);

      // IDgnSREngineNotifySink Methods
      void virtual AttribChanged2(DWORD);
      void virtual Paused(QWORD cookie);
      void virtual MimicDone(DWORD, LPUNKNOWN);
      void virtual ErrorHappened(LPUNKNOWN);
      void virtual Progress(int, const WCHAR *);

      // ISRNotifySink Methods
      void virtual AttribChanged(DWORD);
      void virtual Interference(QWORD, QWORD, DWORD);
      void virtual Sound(QWORD, QWORD);
      void virtual UtteranceBegin(QWORD);
      void virtual UtteranceEnd(QWORD, QWORD);
      void virtual VUMeter(QWORD, WORD);
   };
}