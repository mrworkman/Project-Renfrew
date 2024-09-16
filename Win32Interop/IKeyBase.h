#pragma once

namespace Renfrew::Win32::Interop {
   // Note: Named IKeyBase to not conflict with auto-generated IKey class.
   public interface class IKeyBase {
      public: property int VirtCode {
         int get();
      }
      public: property int ScanCode {
         int get();
      };
      public: property bool IsExtended {
         bool get();
      };
   };
}
