
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
   //auto num_keys = keySequence->keys->Length;

   //if (num_keys == 0) {
   //   return;
   //}

   //auto num_inputs = (keySequence->modifiers->Count + keySequence->keys->Length) * 2;


   //INPUT* inputs = new INPUT[num_inputs];

   //ZeroMemory(
   //   inputs, 
   //   sizeof(INPUT) * num_inputs
   //);

   //auto chars = keySequence->keys->ToCharArray();

   //for (int i = 0, j = 0; i < num_inputs; i += 2, j++) {
   //   inputs[i].type = INPUT_KEYBOARD;
   //   inputs[i].ki.wVk = chars[j];

   //   inputs[i + 1].type = INPUT_KEYBOARD;
   //   inputs[i + 1].ki.wVk = chars[j];
   //   inputs[i + 1].ki.dwFlags = KEYEVENTF_KEYUP;
   //}

   ////inputs[0].type = INPUT_KEYBOARD;
   ////inputs[0].ki.wVk = 'a';

   ////inputs[1].type = INPUT_KEYBOARD;
   ////inputs[1].ki.wVk = 'a';
   ////inputs[1].ki.dwFlags = KEYEVENTF_KEYUP;

   //UINT uSent = SendInput(num_inputs, inputs, sizeof(INPUT));

   //if (uSent != num_inputs) {

   //   Debug::WriteLine("SendInput failed.");
   //}
}

void TestSequence() {
   OutputDebugString(L"Sending test sequence.");

   INPUT inputs[6] = {};
   ZeroMemory(inputs, sizeof(inputs));

   inputs[0].type = INPUT_KEYBOARD;
   inputs[0].ki.wVk = VK_SHIFT;
   inputs[0].ki.dwFlags = KEYEVENTF_EXTENDEDKEY;

   inputs[1].type = INPUT_KEYBOARD;
   inputs[1].ki.wVk = VK_MENU;
   inputs[1].ki.dwFlags = KEYEVENTF_EXTENDEDKEY;

   inputs[2].type = INPUT_KEYBOARD;
   inputs[2].ki.wVk = VK_LEFT;

   inputs[3].type = INPUT_KEYBOARD;
   inputs[3].ki.wVk = VK_LEFT;
   inputs[3].ki.dwFlags = KEYEVENTF_KEYUP;

   inputs[4].type = INPUT_KEYBOARD;
   inputs[4].ki.wVk = VK_MENU;
   inputs[4].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

   inputs[5].type = INPUT_KEYBOARD;
   inputs[5].ki.wVk = VK_SHIFT;
   inputs[5].ki.dwFlags = KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP;

   UINT uSent = SendInput(ARRAYSIZE(inputs), inputs, sizeof(INPUT));

   if (uSent != ARRAYSIZE(inputs)) {

      OutputDebugString(L"SendInput failed.");
   }
}