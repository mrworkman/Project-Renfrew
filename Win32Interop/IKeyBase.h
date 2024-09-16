#pragma once

namespace Renfrew::Win32::Interop {
   // Note: Named IKeyBase to not conflict with auto-generated IKey class.
   public interface class IKeyBase {
      public: property unsigned short VirtCode {
         unsigned short get();
      }
      public: property unsigned short ScanCode {
         unsigned short get();
      };
      public: property bool IsExtended {
         bool get();
      };
   };
}
