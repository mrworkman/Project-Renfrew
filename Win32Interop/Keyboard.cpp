
#include "Stdafx.h"
#include "Keyboard.h"

#include <new>

#include "KeySeq.h"

using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace System::Linq;

using namespace Renfrew::Win32::Interop;


//private ref class Lookups {
//   static Dictionary<Modifier, int>^ _modifierMapping;
//
//   static Lookups() {
//      _modifierMapping = gcnew Dictionary<Modifier, int>();
//
//      _modifierMapping->Add(Modifier::Shift,      VK_SHIFT);
//      _modifierMapping->Add(Modifier::LeftShift,  VK_LSHIFT);
//      _modifierMapping->Add(Modifier::RightShift, VK_RSHIFT);
//   }
//};

void Keyboard::PlayKeys(IEnumerable<KeySeq^>^ keySequences) {
   for each (auto seq in keySequences) {
      SendKeySequence(seq);
   }
}

void Keyboard::PlayKeys(... array<KeySeq^>^ keySequences) {
   return PlayKeys(safe_cast<IEnumerable<KeySeq^>^>(keySequences));
}

void Keyboard::SendKeySequence(KeySeq^ keySequence) {
   auto keys = keySequence->keys;
   auto num_inputs = keySequence->keys->Count * 2;

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

      Debug::WriteLine("({0}:{1}) Key: {2}, DOWN, {3}", i, j, key->VirtCode, key->IsExtended);

      inputs[i].type = INPUT_KEYBOARD;
      inputs[i].ki.wVk = key->VirtCode;
      inputs[i].ki.dwFlags = key->IsExtended ? KEYEVENTF_EXTENDEDKEY : 0;
   }

   for (int i = num_inputs/2, j = (keys->Count - 1); i < num_inputs; i++, j--) {
      auto key = keys[j];

      Debug::WriteLine("({0}:{1}) Key: {2}, UP, {3}", i, j, key->VirtCode, key->IsExtended);

      inputs[i].type = INPUT_KEYBOARD;
      inputs[i].ki.wVk = key->VirtCode;
      inputs[i].ki.dwFlags = KEYEVENTF_KEYUP | (
         key->IsExtended ? KEYEVENTF_EXTENDEDKEY : 0
      );
   }

   UINT uSent = SendInput(num_inputs, inputs, sizeof(INPUT));
   
   if (uSent != num_inputs) {
      delete inputs;
      Debug::WriteLine("SendInput failed.");
   }

   delete inputs;
}
