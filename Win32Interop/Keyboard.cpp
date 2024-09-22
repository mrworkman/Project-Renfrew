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
#include "Keyboard.h"

using namespace System::Diagnostics;

using namespace Renfrew::Win32::Interop;

void Keyboard::PlayKeys(IEnumerable<KeyChord^>^ chords) {
   for each (auto chord in chords) {
      PlayChord(chord);
   }
}

void Keyboard::PlayKeys(... array<KeyChord^>^ chords) {
   return PlayKeys(safe_cast<IEnumerable<KeyChord^>^>(chords));
}

void Keyboard::PlayChord(KeyChord^ chord) {
   auto keys = chord->_keys;
   auto num_inputs = chord->_keys->Count * 2;

   if (num_inputs == 0) {
      return;
   }

   auto inputs = new INPUT[num_inputs];

   ZeroMemory(
      inputs, 
      sizeof(INPUT) * num_inputs
   );

   for (int i = 0, j = 0; i < num_inputs/2; i++, j++) {
      auto key = keys[j];

      Debug::WriteLine("({0}:{1}) Key: {2} DOWN", i, j, key);

      key->KeyDown(&inputs[i]);
   }

   for (int i = num_inputs/2, j = (keys->Count - 1); i < num_inputs; i++, j--) {
      auto key = keys[j];

      Debug::WriteLine("({0}:{1}) Key: {2} UP", i, j, key);

      key->KeyUp(&inputs[i]);
   }

   UINT uSent = SendInput(num_inputs, inputs, sizeof(INPUT));
   
   if (uSent != num_inputs) {
      Debug::WriteLine("SendInput failed.");
      delete inputs;
   }

   delete inputs;
}
