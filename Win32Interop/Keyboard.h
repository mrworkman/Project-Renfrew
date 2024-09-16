#pragma once

#include "KeySeq.h"

namespace Renfrew::Win32::Interop {
   public ref class Keyboard abstract sealed {
      public: static void PlayKeys(IEnumerable<KeySeq^>^ keySequences);
      public: static void PlayKeys(... array<KeySeq^>^ keySequences);

      private: static void SendKeySequence(KeySeq^ keySequence);
   };
}