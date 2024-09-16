#pragma once

#include "Keys.h"

namespace Renfrew::Win32::Interop {
   using namespace System;
   using namespace System::Collections::Generic;

   public ref class KeySeq sealed {
      internal: property List<IKeyBase^>^ keys;

      private: KeySeq(IEnumerable<IKeyBase^>^ keySequences) {
         keys = gcnew List<IKeyBase^>(keySequences);
      }

      public: static KeySeq^ Keys(... array<IKeyBase^>^ keySequences) {
         return gcnew KeySeq(keySequences);
      }
      public: static KeySeq^ Keys(IEnumerable<IKeyBase^>^ keySequences) {
         return gcnew KeySeq(keySequences);
      }
   };
}
