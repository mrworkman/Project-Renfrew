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

#include "Stdafx.h"
#include "KeyPress.h"

using namespace Renfrew::Win32::Interop;

DWORD KeyPress::KeyFlags::get() {
   return 0;
}

WORD KeyPress::ScanCode::get() {
   return 0;
}

WORD KeyPress::VirtualKeyCode::get() {
   return 0;
}

void KeyPress::KeyDown(LPINPUT input) {
   if (input == nullptr) {
      throw gcnew ArgumentNullException("input");
   }
   input->type = INPUT_KEYBOARD;
   input->ki.wVk = VirtualKeyCode;
   input->ki.wScan = ScanCode;
   input->ki.dwFlags = KeyFlags;
}

void KeyPress::KeyUp(LPINPUT input) {
   if (input == nullptr) {
      throw gcnew ArgumentNullException("input");
   }
   input->type = INPUT_KEYBOARD;
   input->ki.wVk = VirtualKeyCode;
   input->ki.wScan = ScanCode;
   input->ki.dwFlags = KEYEVENTF_KEYUP | KeyFlags;
}

bool KeyPress::Equals(KeyPress^ other) {
   return VirtualKeyCode == other->VirtualKeyCode &&
          ScanCode == other->ScanCode &&
          KeyFlags == other->KeyFlags;
}

String^ KeyPress::ToString() {
   return String::Format(
      "{{ VirtualKeyCode={0}, ScanCode={1}, KeyFlags={2} }}",
      VirtualKeyCode, ScanCode, KeyFlags
   );
}
