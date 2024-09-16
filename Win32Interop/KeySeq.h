#pragma once

#include "Keys.h"

namespace Renfrew::Win32::Interop {
   using namespace System;
   using namespace System::Collections::Generic;

   public ref class KeySeq sealed {
      internal: property List<IKeyBase^>^ keys;

      public: static KeySeq^ Keys(... array<IKeyBase^>^ keys) { throw gcnew System::Exception(); }
      public: static KeySeq^ Keys(IEnumerable<IKeyBase^>^ keys) { throw gcnew System::Exception(); }
   };
}
