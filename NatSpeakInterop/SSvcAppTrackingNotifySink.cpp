// Project Renfrew
// Copyright(C) 2019 Stephen Workman (workman.stephen@gmail.com)
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

#include "Stdafx.h"
#include "SSvcAppTrackingNotifySink.h"

using namespace Renfrew::NatSpeakInterop::Sinks;

SSvcAppTrackingNotifySink::SSvcAppTrackingNotifySink() {

}

void SSvcAppTrackingNotifySink::AppLoaded(DWORD a, const wchar_t* b, const wchar_t* c) {
   Debug::WriteLine(__FUNCTION__);
}

void SSvcAppTrackingNotifySink::AppTerminated(DWORD a, const wchar_t* b, const wchar_t* c) {
   Debug::WriteLine(__FUNCTION__);
}
