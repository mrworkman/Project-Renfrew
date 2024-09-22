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
#include "KeyChord.h"

using namespace Renfrew::Win32::Interop;

KeyChord::KeyChord(IEnumerable<KeyPress^>^ keys) {
   _keys = gcnew List<KeyPress^>(keys);
}

KeyChord^ KeyChord::Keys(... array<KeyPress^>^ keys) {
   return gcnew KeyChord(keys);
}

KeyChord^ KeyChord::Keys(IEnumerable<KeyPress^>^ keys) {
   return gcnew KeyChord(keys);
}

bool KeyChord::Equals(KeyChord^ other) {
   if (_keys->Count != other->_keys->Count) {
      return false;
   }

   for (int i = 0; i < _keys->Count; i++) {
      if (!_keys[i]->Equals(other->_keys[i])) {
         return false;
      }
   }

   return true;
}

String^ KeyChord::ToString() {
   auto builder = gcnew StringBuilder();

   builder->Append("[\r\n");
   for each (auto key in _keys) {
      builder->AppendFormat("  {0},\r\n", key);
   }
   builder->Append("]");

   return builder->ToString();
}
